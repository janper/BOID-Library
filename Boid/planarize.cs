using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Planarize : GH_Component
    {

        public Planarize()
            : base("Converge to average plane", "Planarize",
                "Converge to the average plane caluclated from the entire flock",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.list);
            pManager.AddPointParameter("Location points of the flock", "F", "Current location points of all the reference the agents in the flock", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "*", "Output vector length multiplier. Optimal values around 0.10. Less than 0 = negative effect, 0 = no motion, 1 = immediate effect, above 1 = overdone effect", GH_ParamAccess.list, 0.1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector to average plane", "V", "Motion vector towards the average flock plane", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables for the input
            List<Rhino.Geometry.Point3d> points = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Point3d> flock = new List<Rhino.Geometry.Point3d>();
            List<double> multipliers = new List<double>();

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            //check inputs
            if (!DA.GetDataList(0, points)) { return; }
            if (!DA.GetDataList(1, flock)) { return; }
            if (!DA.GetDataList(2, multipliers)) { return; }

            if ((points.Count == 0) || (points == null)) { return; }
            if ((flock.Count == 0) || (flock == null)) { return; }

            //for each point in the list of points

            Plane avgPlane;
            Plane.FitPlaneToPoints(flock, out avgPlane);

            for (int i = 0; i < points.Count; i++)
            {
                Point3d point = points[i];
                Point3d testPoint = avgPlane.ClosestPoint(point);
                Vector3d vector = new Vector3d(testPoint - point);
                newVectors.Add(vector * multipliers[(i >= multipliers.Count) ? multipliers.Count - 1 : i]);
            }

            // output
            DA.SetDataList(0, newVectors);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.planarize;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920039}"); }
        }
    }
}
