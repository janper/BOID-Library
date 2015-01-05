using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Slide : GH_Component
    {

        public Slide()
            : base("Slide over geometry", "Slide",
                "Slide over the closest geometry",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.item);
            pManager.AddVectorParameter("Agent motion vector", "V", "Current motion vectors of the agents", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Reference geometry", "G", "Reference geometry to stick to.", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Search distance", "D", "Search distance domain. x to 0 =  infinity", GH_ParamAccess.item, new Rhino.Geometry.Interval(0, 0));
            pManager.AddNumberParameter("Multiplier", "*", "Output vector length multiplier. Optimal values around 0.10. Less than 0 = negative effect, 0 = no motion, 1 = immediate effect, above 1 = overdone effect", GH_ParamAccess.item, 0.1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector to the closest geometry", "V", "Vector towards the closest geometry", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d vector = Rhino.Geometry.Vector3d.Unset;
            List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper> geometry = new List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper>();
            Rhino.Geometry.Interval distances = Rhino.Geometry.Interval.Unset;
            double multiplier = 1;


            // Daclare a variable for the output
            Rhino.Geometry.Vector3d minVector = Rhino.Geometry.Vector3d.Unset;

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetDataList(2, geometry)) { return; }
            if (!DA.GetData(3, ref distances)) { return; }
            if (!DA.GetData(4, ref multiplier)) { return; }
            

            if ((geometry.Count == 0) || (geometry == null)) { return; }
            if (point == null) { return; }

            //for each point in the list of points

            double minDistance = double.MaxValue;
            Rhino.Geometry.Vector3d projectedVector = Rhino.Geometry.Vector3d.Unset;

            double minSquaredDistance = distances.T0 * distances.T0;
            double maxSquaredDistance = distances.T1 * distances.T1;

            Rhino.Geometry.Vector3d refVector = Rhino.Geometry.Vector3d.Unset;

            foreach (Grasshopper.Kernel.Types.GH_GeometricGooWrapper geo in geometry)
            {
                Rhino.Geometry.Point3d testPoint = Rhino.Geometry.Point3d.Unset;
                Rhino.Geometry.Plane testPlane = Rhino.Geometry.Plane.Unset;

                geo.CastTo<Rhino.Geometry.Plane>(ref testPlane);
                if (testPlane.IsValid)
                {
                    testPoint = testPlane.Origin;
                    refVector = vector;
                }

                Rhino.Geometry.Curve testCurve = null;
                geo.CastTo<Rhino.Geometry.Curve>(ref testCurve);
                if (testCurve != null)
                {
                    if (testCurve.IsValid)
                    {
                        double t;
                        testCurve.ClosestPoint(point, out t);
                        if (testCurve.Domain.IncludesParameter(t))
                        {
                            Rhino.Geometry.Vector3d tempVector = testCurve.TangentAt(t);
                            testPoint = testCurve.PointAt(t);
                            testCurve.FrameAt(t, out testPlane);
                            Rhino.Geometry.Vector3d tempVectorRev = tempVector;
                            tempVector *= vector.Length / tempVector.Length;
                            tempVectorRev.Reverse();
                            double angle1 = Rhino.Geometry.Vector3d.VectorAngle(tempVector, vector);
                            double angle2 = Rhino.Geometry.Vector3d.VectorAngle(tempVectorRev, vector);
                            refVector = (angle1 < angle2) ? tempVector : tempVectorRev;
                        }
                    }
                }

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
                        BrepFace face = testBrep.Faces.ElementAt(compIndex.Index);
                        if (face.Domain(0).IncludesParameter(s) && face.Domain(1).IncludesParameter(t))
                        {
                            testPlane = new Rhino.Geometry.Plane(testPoint, testNormal);
                            refVector = vector;
                        }
                    }
                }

                if ((testPlane != null) && (testPlane.IsValid))
                {
                    Rhino.Geometry.Vector3d testVector = new Rhino.Geometry.Vector3d(testPoint - point);
                    double squaredDistance = testVector.SquareLength;
                    if ((squaredDistance > minSquaredDistance) && ((squaredDistance < maxSquaredDistance) || (maxSquaredDistance <= 0)) && (squaredDistance<minDistance))
                    {
                        minDistance = squaredDistance;
                        Rhino.Geometry.Transform project = Rhino.Geometry.Transform.PlanarProjection(testPlane);
                        projectedVector = refVector;
                        projectedVector.Transform(project);
                    }
                }
            }

            // output
            DA.SetData(0, projectedVector*multiplier);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.slide;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920036}"); }
        }
    }
}
