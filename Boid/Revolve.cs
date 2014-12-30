using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Revolve : GH_Component
    {

        public Revolve()
            : base("Revolve around geometry", "Revolve",
                "Revolve around the closest geometry",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.item);
            pManager.AddVectorParameter("Agent motion vector", "V", "Current motion vectors of the agents", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Reference geometry", "G", "Reference geometry to stick to.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Revolving angle", "A", "Revolving angle per step (negative changes direction).", GH_ParamAccess.item, Math.PI/100);
            pManager.AddIntervalParameter("Search distance", "D", "Search distance domain. x to 0 =  infinity", GH_ParamAccess.item, new Rhino.Geometry.Interval(0, 0));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector around the closest geometry", "V", "Vector around the closest geometry", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d vector = Rhino.Geometry.Vector3d.Unset;
            List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper> geometry = new List<Grasshopper.Kernel.Types.GH_GeometricGooWrapper>();
            double angle = 0 ;
            Rhino.Geometry.Interval distances = Rhino.Geometry.Interval.Unset;


            // Daclare a variable for the output
            Rhino.Geometry.Vector3d minVector = Rhino.Geometry.Vector3d.Unset;

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetDataList(2, geometry)) { return; }
            if (!DA.GetData(3, ref angle)) { return; }
            if (!DA.GetData(4, ref distances)) { return; }

            if ((geometry.Count == 0) || (geometry == null)) { return; }
            if (point == null) { return; }

            //for each point in the list of points

            double minDistance = double.MaxValue;
            Rhino.Geometry.Vector3d rotationVector = Rhino.Geometry.Vector3d.Unset;

            double minSquaredDistance = distances.T0 * distances.T0;
            double maxSquaredDistance = distances.T1 * distances.T1;

            Rhino.Geometry.Plane rotationPlane = Rhino.Geometry.Plane.WorldXY;

            foreach (Grasshopper.Kernel.Types.GH_GeometricGooWrapper geo in geometry)
            {
                Rhino.Geometry.Point3d testPoint = Rhino.Geometry.Point3d.Unset;
                Rhino.Geometry.Plane testPlane = Rhino.Geometry.Plane.Unset;
                geo.CastTo<Rhino.Geometry.Point3d>(ref testPoint);
                if (testPoint.IsValid)
                {
                    rotationPlane = new Rhino.Geometry.Plane(testPoint, new Rhino.Geometry.Vector3d(point - testPoint), vector);   
                }
                
                /*
                geo.CastTo<Rhino.Geometry.Plane>(ref testPlane);
                if (testPlane.IsValid)
                {
                    debug += "is a plane; ";

                    Rhino.Geometry.Transform project = Rhino.Geometry.Transform.PlanarProjection(testPlane);
                    Rhino.Geometry.Vector3d projectedVector = vector;
                    projectedVector.Transform(project);
                    Rhino.Geometry.Vector3d.CrossProduct(projectedVector, vector);
                    testPoint = testPlane.ClosestPoint(point);
                    rotationPlane = new Rhino.Geometry.Plane(testPoint, normal);
                }
                */

                Rhino.Geometry.Curve testCurve = null;
                geo.CastTo<Rhino.Geometry.Curve>(ref testCurve);
                if (testCurve != null)
                {
                    if (testCurve.IsValid)
                    {
                        double t;
                        testCurve.ClosestPoint(point, out t);
                        rotationPlane = new Rhino.Geometry.Plane(testCurve.PointAt(t), testCurve.TangentAt(t));
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
                        rotationPlane = new Rhino.Geometry.Plane(testPoint, new Rhino.Geometry.Vector3d(testPoint - point), testNormal);
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
                        rotationPlane = new Rhino.Geometry.Plane(testPoint, new Rhino.Geometry.Vector3d(testPoint - point), testNormal);
                    }
                }

                if ((rotationPlane != null) && (rotationPlane.IsValid))
                {
                    Rhino.Geometry.Vector3d testVector = new Rhino.Geometry.Vector3d(rotationPlane.Origin - point);
                    double squaredDistance = testVector.SquareLength;
                    if ( (squaredDistance > minSquaredDistance) && ((squaredDistance < maxSquaredDistance) || (maxSquaredDistance <= 0)) )
                    {
                        minDistance = squaredDistance;
                        Rhino.Geometry.Transform rotate = Rhino.Geometry.Transform.Rotation(angle, rotationPlane.Normal, rotationPlane.Origin);
                        Rhino.Geometry.Point3d tempPoint = new Rhino.Geometry.Point3d(point);
                        tempPoint.Transform(rotate);
                        rotationVector = new Rhino.Geometry.Vector3d(tempPoint - point);
                    }
                }
            }

            // output
            DA.SetData(0, rotationVector);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.revolve;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920037}"); }
        }
    }
}
