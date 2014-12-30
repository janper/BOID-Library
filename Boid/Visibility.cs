using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Visibility : GH_Component
    {

        public Visibility()
            : base("Find visible agents", "Visible",
                "Find agents from flock visible to the current agent",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.item);
            pManager.AddVectorParameter("Agent motion vector", "V", "Current motion vectors of the agents", GH_ParamAccess.item);
            pManager.AddPointParameter("Location points of the flock", "F", "Current location points of all the reference the agents in the flock", GH_ParamAccess.list);
            pManager.AddVectorParameter("Flock motion vectors", "FV", "Current motion vectors of all the reference the agents inthe flock", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Search distance", "D", "Search distance domain. x to 0 =  infinity", GH_ParamAccess.item, new Rhino.Geometry.Interval(0, 0));
            pManager.AddIntervalParameter("Search angle", "A", "Search angle domain. x to 0 = infinity", GH_ParamAccess.item, new Rhino.Geometry.Interval(0, 0));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "Location points of the visible flock agents", GH_ParamAccess.list);
            pManager.AddVectorParameter("List of vectors", "V", "Motion vectors of the visible flock agents", GH_ParamAccess.list);
            pManager.AddIntegerParameter("List of indices", "I", "List indices of the visible flock agents", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables for the input
            Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Vector3d vector = Rhino.Geometry.Vector3d.Unset;
            List<Rhino.Geometry.Point3d> flock = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Vector3d> flockVectors = new List<Rhino.Geometry.Vector3d>();
            Rhino.Geometry.Interval distances = Rhino.Geometry.Interval.Unset;
            Rhino.Geometry.Interval angles = Rhino.Geometry.Interval.Unset;

            // Daclare a variable for the output
            List<Rhino.Geometry.Point3d> newPoints = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();
            List<int> newIndices = new List<int>();

            //check inputs
            if (!DA.GetData(0, ref point)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetDataList(2, flock)) { return; }
            if (!DA.GetDataList(3, flockVectors)) { return; }
            if (!DA.GetData(4, ref distances)) { return; }
            if (!DA.GetData(5, ref angles)) { return; }

            if ((flock.Count == 0) || (flock == null)) { return; }

            double minSquaredDistance = distances.T0 * distances.T0;
            double maxSquaredDistance = distances.T1 * distances.T1;

            int max = Math.Max(flockVectors.Count, flock.Count);

            for (int i = 0; i < max; i++)
            {
                int flockVectorIndex = (i > flockVectors.Count - 1) ? flockVectors.Count - 1 : i;
                int flockIndex = (i > flock.Count - 1) ? flock.Count - 1 : i;

                Rhino.Geometry.Vector3d direction = new Rhino.Geometry.Vector3d(flock[flockIndex] - point);

                if (((direction.SquareLength >= minSquaredDistance) && (direction.SquareLength <= maxSquaredDistance)) || (maxSquaredDistance == 0))
                {
                    double angle = Rhino.Geometry.Vector3d.VectorAngle(vector, direction);
                    if (((angle > angles.T0) && (angle < angles.T1))||(angles.T1 <=0))
                    {
                        newPoints.Add(flock[flockIndex]);
                        newVectors.Add(flockVectors[flockVectorIndex]);
                        newIndices.Add(flockIndex);
                    }
                }
            }

            // output
            DA.SetDataList(0, newPoints);
            DA.SetDataList(1, newVectors);
            DA.SetDataList(2, newIndices);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.visibility;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920034}"); }
        }
    }
}
