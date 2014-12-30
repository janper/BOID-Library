using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Stick : GH_Component
    {

        public Stick()
            : base("Stick to geometry", "Stick",
                "Stick to the closest geometry",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Reference geometry", "G", "Reference geometry to stick to.", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Search distance", "D", "Search distance domain. x to 0 =  infinity", GH_ParamAccess.item, new Rhino.Geometry.Interval(0, 0));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector to the closest geomerty", "V", "Vector towards the closest geomerty", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper> geometry = new List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper>();
            Rhino.Geometry.Interval distances = Rhino.Geometry.Interval.Unset;


            // Daclare a variable for the output
            Rhino.Geometry.Vector3d minVector = Rhino.Geometry.Vector3d.Unset;

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetDataList(1, geometry)) { return; }
            if (!DA.GetData(2, ref distances)) { return; }

            if ((geometry.Count == 0) || (geometry == null)) { return; }
            if (point == null) { return; }

            //for each point in the list of points

            double minDistance = double.MaxValue;

            double minSquaredDistance = distances.T0 * distances.T0;
            double maxSquaredDistance = distances.T1 * distances.T1;

            foreach (Grasshopper.Kernel.Types.GH_GeometricGooWrapper geo in geometry)
            {
                Rhino.Geometry.Point3d testPoint = Rhino.Geometry.Point3d.Unset;
                geo.CastTo<Rhino.Geometry.Point3d>(ref testPoint);

                Rhino.Geometry.Plane testPlane = Rhino.Geometry.Plane.Unset;
                geo.CastTo<Rhino.Geometry.Plane>(ref testPlane);
                if (testPlane != null)
                {
                    if (testPlane.IsValid)
                    {
                        testPoint = testPlane.Origin;
                    }
                }

                Rhino.Geometry.Curve testCurve = null;
                geo.CastTo<Rhino.Geometry.Curve>(ref testCurve);
                if (testCurve != null)
                {
                    if (testCurve.IsValid)
                    {
                        double t;
                        testCurve.ClosestPoint(point, out t);
                        testPoint = testCurve.PointAt(t);
                    }
                }

                Rhino.Geometry.Mesh testMesh = null;
                geo.CastTo<Rhino.Geometry.Mesh>(ref testMesh);
                if (testMesh != null)
                {
                    if (testMesh.IsValid)
                    {
                        testPoint = testMesh.ClosestPoint(point);
                    }
                }

                Rhino.Geometry.Brep testBrep = null;
                geo.CastTo<Rhino.Geometry.Brep>(ref testBrep);
                if (testBrep != null)
                {
                    if (testBrep.IsValid)
                    {
                        testPoint = testBrep.ClosestPoint(point);
                    }
                }

                if (testPoint.IsValid)
                {
                    Rhino.Geometry.Vector3d tempVector = new Rhino.Geometry.Vector3d(testPoint - point);
                    double squaredDistance = tempVector.SquareLength;
                    if ((squaredDistance < minDistance) && ((squaredDistance > minSquaredDistance) && ((squaredDistance < maxSquaredDistance) || (maxSquaredDistance <= 0))))
                    {
                        minDistance = squaredDistance;                        
                        minVector = tempVector;
                    }
                }
            }

            // output
            DA.SetData(0, minVector);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.stick;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920035}"); }
        }
    }
}
