using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Bounce : GH_Component
    {
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
            pManager.AddTextParameter("Debug", "out", "debug", GH_ParamAccess.item);
            pManager.AddPlaneParameter("mirrorPlane", "mP", "debug", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            String debug = "Start; ";
            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d vector = Rhino.Geometry.Vector3d.Unset;
            //List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper> geometry = new List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper>();
            //List<Rhino.Geometry.GeometryBase> geometry = new List<Rhino.Geometry.GeometryBase>();
            List<Object> geometry = new List<object>();
            double slowdown = 1;


            // Daclare a variable for the output
            Rhino.Geometry.Point3d outPoint = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d outVector = Rhino.Geometry.Vector3d.Unset;
            Plane outPlane = Plane.Unset;

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetDataList(2, geometry)) { return; }
            if (!DA.GetData(3, ref slowdown)) { return; }

            if ((geometry.Count == 0) || (geometry == null)) { return; }
            if (point == null) { return; }

            debug += "collected inputs; ";

            //temporary outputs

            double minSquaredDistance = double.MaxValue;

            //Rhino.Geometry.Line tempLine = new Rhino.Geometry.Line(point, point + vector);
            List<Point3d> points = new List<Point3d>();
            points.Add(point);
            points.Add(point + vector);
            Rhino.Geometry.Curve tempCurve = Curve.CreateControlPointCurve(points);
            //Grasshopper.Kernel.GH_Convert.ToCurve(tempLine, ref tempCurve, GH_Conversion.Both);
            debug += "created curve; ";

            Rhino.Geometry.Vector3d projectedVector = Rhino.Geometry.Vector3d.Unset;
            Rhino.Geometry.Vector3d refVector = Rhino.Geometry.Vector3d.Unset;
            Rhino.Geometry.Plane mirrorPlane = Rhino.Geometry.Plane.Unset;

            //foreach (Grasshopper.Kernel.Types.GH_GeometricGooWrapper geo in geometry)
            //foreach (Rhino.Geometry.GeometryBase geo in geometry)
            foreach (Object geo in geometry)
            {
                debug += "in da loop; ";
                Rhino.Geometry.Point3d testPoint = Rhino.Geometry.Point3d.Unset;
                Rhino.Geometry.Plane testPlane = Rhino.Geometry.Plane.Unset;

                //Grasshopper.Kernel.GH_Convert.ToPlane(geo, ref testPlane, GH_Conversion.Both);
                testPlane = (Plane)geo;
                // geo.CastTo<Rhino.Geometry.Plane>(ref testPlane);
                debug += "testing plane; ";
                if (testPlane.IsValid)
                {
                    debug += "is a plane: " + testPlane.ToString() + "; ";
                    Rhino.Geometry.Intersect.CurveIntersections intersection = Rhino.Geometry.Intersect.Intersection.CurvePlane(tempCurve, testPlane, tempCurve.GetLength() * 0.01);
                    debug += "intersection.Count=" + intersection.Count + "; ";
                    if (intersection.Count > 0)
                    {
                        debug += "found an intersection; ";
                        debug += "intersection:" + intersection.First().ToString() + "; ";
                        testPoint = intersection.First().PointA;
                        debug += "point assigned; ";
                        Rhino.Geometry.Transform project = Rhino.Geometry.Transform.PlanarProjection(testPlane);
                        refVector = vector;
                        refVector.Transform(project);
                        mirrorPlane = new Plane(testPoint, refVector);
                        debug += "created a mirro plane; ";
                    }
                }
                /*
                Rhino.Geometry.Mesh testMesh = null;
                geo.CastTo<Rhino.Geometry.Mesh>(ref testMesh);
                if (testMesh != null)
                {
                    if (testMesh.IsValid)
                    {
                        Rhino.Geometry.Vector3d testNormal = Rhino.Geometry.Vector3d.Unset;
                        testMesh.ClosestPoint(point, out testPoint, out testNormal, distances.T1);
                        testPlane = new Rhino.Geometry.Plane(testPoint, testNormal);
                        refVector = vector;
                    }
                }

                Rhino.Geometry.Brep testBrep = null;
                geo.CastTo<Rhino.Geometry.Brep>(ref testBrep);
                if (testBrep != null)
                {
                    if (testBrep.IsValid)
                    {
                        Rhino.Geometry.Vector3d testNormal = Rhino.Geometry.Vector3d.Unset;
                        double s;
                        double t;
                        Rhino.Geometry.ComponentIndex compIndex;
                        testBrep.ClosestPoint(point, out testPoint, out compIndex, out s, out t, distances.T1, out testNormal);
                        testPlane = new Rhino.Geometry.Plane(testPoint, testNormal);
                        refVector = vector;
                    }
                }
                */


                if (mirrorPlane.IsValid)
                {
                    debug += "the mirror plane is vaid; ";
                    Rhino.Geometry.Vector3d testVector = new Rhino.Geometry.Vector3d(testPoint - point);
                    double squaredDistance = testVector.SquareLength;
                    if (squaredDistance < minSquaredDistance)
                    {
                        debug += "and the intersection point is the closesd one; ";
                        minSquaredDistance = squaredDistance;
                        Transform mirror = Transform.Mirror(mirrorPlane);
                        Point3d mirroredPoint = point;
                        mirroredPoint.Transform(mirror);
                        debug += "point mirrored; ";
                        outVector = new Vector3d(mirroredPoint - testPoint);
                        debug += "vector created; ";
                        //outPoint = testPoint + outVector * (((vector.Length - testVector.Length) / outVector.Length) * slowdown);
                        outPoint = testPoint;
                        outVector *= vector.Length / outVector.Length;
                        outVector *= slowdown;
                        debug += "output ready; ";
                        outPlane = mirrorPlane;
                    }
                }

            }
            // output
            debug += "out; ";
            DA.SetData(0, outPoint);
            DA.SetData(1, outVector);
            DA.SetData(2, debug);
            DA.SetData(3, outPlane);
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
