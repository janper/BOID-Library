using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Boid
{
    public class Bounce : GH_Component
    {
        String debug = "start;'\r\n";
        public Bounce()
            : base("Move and bounce from geometry", "BounceMove",
                "Bounce from geometry in case the agent hits its wall. The non-bounced agents will be moved to their new location. Warning: the component is slow.",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.item);
            pManager.AddVectorParameter("Agent motion vector", "V", "Current motion vectors of the agents", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Reference geometry", "G", "Reference geometry to stick to.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Bounce slowdown", "S", "Bounce slowdown ratio. 1 = no slowdown after bounce; 0 = agent stops after bounce", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("New location point", "P", "New location after bounce", GH_ParamAccess.item);
            pManager.AddVectorParameter("New motion vector", "V", "Motion vector of an agent after the bounce", GH_ParamAccess.item);
            //pManager.AddTextParameter("Out", "out", "debug", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            debug += "------------------------\r\n";
            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d vector = Rhino.Geometry.Vector3d.Unset;
            List<Object> geometry = new List<object>();
            double slowdown = 1;

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetDataList(2, geometry)) { return; }
            if (!DA.GetData(3, ref slowdown)) { return; }

            if ((geometry.Count == 0) || (geometry == null)) { return; }
            if (point == null) { return; }

            Point3d backupPoint = point;
            Vector3d backupVector = vector;

            // Daclare a variables for the output
            Rhino.Geometry.Point3d outPoint = Point3d.Unset;
            Rhino.Geometry.Vector3d outVector = Vector3d.Unset;

            Boolean didAnythingHappen = false;

            double minSquaredDistance = double.MaxValue;

            Rhino.Geometry.Plane mirrorPlane = Rhino.Geometry.Plane.Unset;

            foreach (Object geo in geometry)
            {
                Rhino.Geometry.Plane testPlane = Rhino.Geometry.Plane.Unset;

                //test for BRep
                GH_Brep ghBrep;
                try
                {
                    ghBrep = (GH_Brep)geo;
                    if (ghBrep.IsValid)
                    {
                        Brep testBrep;
                        ghBrep.CastTo<Brep>(out testBrep);
                        if (testBrep.IsValid)
                        {
                            debug += "valid Brep;\r\n";

                            List<Point3d> curvePoints = new List<Point3d>();
                            curvePoints.Add(point);
                            curvePoints.Add(point + vector);
                            Curve testCurve = Curve.CreateInterpolatedCurve(curvePoints, 1);

                            Curve[] overlapCurves;
                            Point3d[] intersectionPoints;
                            Rhino.Geometry.Intersect.Intersection.CurveBrep(testCurve, testBrep, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 0.01, out overlapCurves, out intersectionPoints);
                            if (intersectionPoints.Count() > 0)
                            {
                                List<Point3d> intersectPoints = new List<Point3d>(intersectionPoints);
                                intersectPoints.OrderBy(o => o.DistanceTo(point));
                                Point3d tempPoint = intersectPoints.First();
                                double tt;
                                testCurve.ClosestPoint(tempPoint, out tt);
                                if (testCurve.Domain.IncludesParameter(tt))
                                {
                                    Point3d closestPoint = Point3d.Unset;
                                    ComponentIndex ci;
                                    double s;
                                    double t;
                                    Vector3d tempNormal = Vector3d.Unset;
                                    testBrep.ClosestPoint(tempPoint, out closestPoint, out ci, out s, out t, vector.Length, out tempNormal);

                                    testPlane = new Plane(tempPoint, tempNormal);

                                    Vector3d tempVector = Vector3d.Unset;
                                    double tempMinSquareDistance = minSquaredDistance;

                                    if (TestSituation(point, vector, testPlane, slowdown, ref tempPoint, ref tempVector, ref tempMinSquareDistance))
                                    {
                                        outPoint = tempPoint;
                                        outVector = tempVector;
                                        minSquaredDistance = tempMinSquareDistance;
                                        didAnythingHappen = true;
                                        debug += "brep bounce;\r\n";
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    debug += "no brep;\r\n";
                }

                //test for mesh
                GH_Mesh ghMesh;
                try
                {
                    ghMesh = (GH_Mesh)geo;
                    if (ghMesh.IsValid)
                    {
                        Mesh testMesh = new Mesh();
                        ghMesh.CastTo<Mesh>(out testMesh);
                        if (testMesh.IsValid)
                        {
                            debug += "valid Mesh;\r\n";
                            int[] faces;
                            Ray3d testRay = new Ray3d(point, vector);
                            double t = Rhino.Geometry.Intersect.Intersection.MeshRay(testMesh, testRay, out faces);
                            if (t > 0)
                            {
                                Point3d tempPoint = testRay.PointAt(t);

                                MeshFace tempFace = testMesh.Faces[faces[0]];
                                Vector3d tempNormal = (Vector3d)testMesh.Normals[tempFace.A];
                                tempNormal += (Vector3d)testMesh.Normals[tempFace.B] + (Vector3d)testMesh.Normals[tempFace.C];
                                if (tempFace.IsQuad) { tempNormal += (Vector3d)testMesh.Normals[tempFace.D]; }

                                testPlane = new Plane(tempPoint, tempNormal);

                                Vector3d tempVector = Vector3d.Unset;
                                double tempMinSquareDistance = minSquaredDistance;

                                if (TestSituation(point, vector, testPlane, slowdown, ref tempPoint, ref tempVector, ref tempMinSquareDistance))
                                {
                                    outPoint = tempPoint;
                                    outVector = tempVector;
                                    minSquaredDistance = tempMinSquareDistance;
                                    didAnythingHappen = true;
                                    debug += "mesh bounce;\r\n";
                                }
                            }
                        }
                    }
                }
                catch
                {
                    debug += "no mesh;\r\n";
                }


                //test for plane

                GH_Plane ghPlane;
                try
                {
                    ghPlane = (GH_Plane)geo;
                    if (ghPlane.IsValid) { ghPlane.CastTo<Plane>(out testPlane); }
                    if (testPlane.IsValid)
                    {
                        debug += "valid Plane;\r\n";
                        List<Point3d> curvePoints = new List<Point3d>();
                        curvePoints.Add(point);
                        curvePoints.Add(point + vector * 1.1);
                        Curve testCurve = Curve.CreateInterpolatedCurve(curvePoints, 1);

                        Rhino.Geometry.Intersect.CurveIntersections intersections = Rhino.Geometry.Intersect.Intersection.CurvePlane(testCurve, testPlane, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                        if (intersections != null)
                        {
                            if (intersections.Count() > 0)
                            {
                                Rhino.Geometry.Intersect.IntersectionEvent intersectionEvent = intersections.First();
                                if (intersectionEvent.IsPoint)
                                {
                                    double tt = intersectionEvent.ParameterA;
                                    if (testCurve.Domain.IncludesParameter(tt))
                                    {
                                        testPlane.Origin = intersectionEvent.PointA;

                                        Point3d tempPoint = Point3d.Unset;
                                        Vector3d tempVector = Vector3d.Unset;
                                        double tempMinSquareDistance = minSquaredDistance;


                                        if (TestSituation(point, vector, testPlane, slowdown, ref tempPoint, ref tempVector, ref tempMinSquareDistance))
                                        {
                                            outPoint = tempPoint;
                                            outVector = tempVector;
                                            minSquaredDistance = tempMinSquareDistance;
                                            didAnythingHappen = true;
                                            debug += "plane bounce;\r\n";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    debug += "no plane;\r\n";
                }


            }

            if (!didAnythingHappen)
            {
                outPoint = new Point3d(backupPoint + backupVector);
                outVector = backupVector;
                debug += "no bounce;\r\n";
            }

            // output
            DA.SetData(0, outPoint);
            DA.SetData(1, outVector);
            //DA.SetData(2, debug);
        }

        private Boolean TestSituation(Rhino.Geometry.Point3d point, Rhino.Geometry.Vector3d vector, Rhino.Geometry.Plane testPlane, double slowdown, ref Rhino.Geometry.Point3d outPoint, ref Rhino.Geometry.Vector3d outVector, ref double minSquaredDistance)
        {
            Point3d testPoint = testPlane.Origin;
            double intersectionLength = new Vector3d(testPoint - point).SquareLength;
            if ((intersectionLength <= vector.SquareLength) && (intersectionLength < minSquaredDistance))
            {
                Plane mirrorPlane = MakeMirrorPlane(vector, testPoint, testPlane);
                if (mirrorPlane.IsValid)
                {
                    Rhino.Geometry.Vector3d testVector = new Rhino.Geometry.Vector3d(mirrorPlane.Origin - point);
                    double squaredDistance = testVector.SquareLength;
                    if (squaredDistance < minSquaredDistance)
                    {
                        minSquaredDistance = squaredDistance;
                        Transform mirror = Transform.Mirror(mirrorPlane);
                        Point3d mirroredPoint = point;
                        mirroredPoint.Transform(mirror);
                        outVector = new Vector3d(mirroredPoint - mirrorPlane.Origin);
                        outVector.Unitize();
                        Vector3d tempVector = outVector;
                        tempVector *= (vector.Length - testVector.Length) * slowdown;
                        outVector *= vector.Length * slowdown;
                        outPoint = mirrorPlane.Origin + tempVector;
                        return true;
                    }
                    else { return false; }
                }
                else { return false; }
            }
            else { return false; }
        }

        private static Rhino.Geometry.Plane MakeMirrorPlane(Rhino.Geometry.Vector3d vector, Rhino.Geometry.Point3d intersectionPoint, Rhino.Geometry.Plane testPlane)
        {
            Rhino.Geometry.Transform project = Rhino.Geometry.Transform.PlanarProjection(testPlane);
            Vector3d refVector = vector;
            refVector.Transform(project);
            return new Plane(intersectionPoint, refVector);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.bounce;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920038}"); }
        }
    }
}
