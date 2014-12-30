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
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d vector = Rhino.Geometry.Vector3d.Unset;
            List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper> geometry = new List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper>();
            double slowdown = 1;


            // Daclare a variable for the output
            Rhino.Geometry.Point3d outPoint = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d outVector = Rhino.Geometry.Vector3d.Unset;

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetDataList(2, geometry)) { return; }
            if (!DA.GetData(3, ref slowdown)) { return; }

            if ((geometry.Count == 0) || (geometry == null)) { return; }
            if (point == null) { return; }

            //temporary outputs

            Rhino.Geometry.Point3d tempPoint = point+vector;
            Rhino.Geometry.Vector3d tempVector = vector;

            Rhino.Geometry.Line tempLine = new Rhino.Geometry.Line(point, tempPoint);
            Rhino.Geometry.Curve tempCurve = null;
            Grasshopper.Kernel.GH_Convert.ToCurve(tempLine, ref tempCurve, GH_Conversion.Both);



            Rhino.Geometry.Vector3d projectedVector = Rhino.Geometry.Vector3d.Unset;
            Rhino.Geometry.Vector3d refVector = Rhino.Geometry.Vector3d.Unset;

            foreach (Grasshopper.Kernel.Types.GH_GeometricGooWrapper geo in geometry)
            {
                Rhino.Geometry.Point3d testPoint = Rhino.Geometry.Point3d.Unset;
                Rhino.Geometry.Plane testPlane = Rhino.Geometry.Plane.Unset;

                geo.CastTo<Rhino.Geometry.Plane>(ref testPlane);
                if (testPlane.IsValid)
                {
                    Rhino.Geometry.Intersect.CurveIntersections intersection = Rhino.Geometry.Intersect.Intersection.CurvePlane(tempCurve, testPlane, 0.1);
                    if (intersection.Count > 0)
                    {
                        testPoint = intersection.First().PointA; ;
                        Rhino.Geometry.Transform project = Rhino.Geometry.Transform.PlanarProjection(testPlane);
                        refVector = vector;
                        refVector.Transform(project);
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
                        testPlane = new Rhino.Geometry.Plane(testPoint, testNormal);
                        refVector = vector;
                    }
                }

                if ((testPlane != null) && (testPlane.IsValid))
                {
                    Rhino.Geometry.Vector3d testVector = new Rhino.Geometry.Vector3d(testPoint - point);
                    double squaredDistance = testVector.SquareLength;
                    if ((squaredDistance > minSquaredDistance) && ((squaredDistance < maxSquaredDistance) || (maxSquaredDistance <= 0)))
                    {
                        minDistance = squaredDistance;
                        Rhino.Geometry.Transform project = Rhino.Geometry.Transform.PlanarProjection(testPlane);
                        projectedVector = refVector;
                        projectedVector.Transform(project);
                    }
                }
            }

            // output
            DA.SetData(0, projectedVector);
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
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920038}"); }
        }
    }
}
