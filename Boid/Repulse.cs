using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Repulse : GH_Component
    {

        public Repulse()
            : base("Repulse from the flock", "Repulse",
                "Repulse agent from the flock agents within the search distance",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.list);
            pManager.AddPointParameter("Location points of the flock", "F", "Current location points of all the reference the agents in the flock", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Search distance", "D", "Search distance domain. Domain maximum is also the desired separation distance of the agents.", GH_ParamAccess.list, new Rhino.Geometry.Interval(0, 1));
            pManager.AddNumberParameter("Multiplier", "*", "Output vector length multiplier. Optimal values around 0.10. Less than 0 = negative effect, 0 = no motion, 1 = immediate effect, above 1 = overdone effect", GH_ParamAccess.list, 0.1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector away from the closest agent", "V", "Motion vector away from the flock agents within the search distance in the flock. Magnitude = search distance domain maximum", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables for the input
            List<Rhino.Geometry.Point3d> points = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Point3d> flock = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Interval> intervals = new List<Rhino.Geometry.Interval>();
            List<double> multipliers = new List<double>();

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            //check inputs
            if (!DA.GetDataList(0, points)) { return; }
            if (!DA.GetDataList(1, flock)) { return; }
            if (!DA.GetDataList(2, intervals)) { return; }
            if (!DA.GetDataList(3, multipliers)) { return; }

            if ((points.Count == 0) || (points == null)) { return; }
            if ((flock.Count == 0) || (flock == null)) { return; }

            //for each point in the list of points
            for (int i = 0; i < points.Count; i++)
            {
                Rhino.Geometry.Vector3d vector = new Rhino.Geometry.Vector3d(0, 0, 0);

                List<Rhino.Geometry.Vector3d> tempVectors = new List<Rhino.Geometry.Vector3d>();

                double minSquaredDistance = (i > intervals.Count - 1) ? intervals[intervals.Count - 1].T0 : intervals[i].T0;
                minSquaredDistance *= minSquaredDistance;
                double maxDistance = (i > intervals.Count - 1) ? intervals[intervals.Count - 1].T1 : intervals[i].T1;
                double maxSquaredDistance = maxDistance * maxDistance;

                for (int j = 0; j < flock.Count; j++)
                {
                    Rhino.Geometry.Vector3d tempVector = new Rhino.Geometry.Vector3d(points[i] - flock[j]);
                    if ((tempVector.SquareLength >= minSquaredDistance) && (tempVector.SquareLength <= maxSquaredDistance)) { tempVectors.Add(tempVector); }
                }

                //if there are any relevant points nearby
                if (tempVectors.Count > 0)
                {
                    IEnumerable<Rhino.Geometry.Vector3d> orderedVectorsEnumerable = tempVectors.OrderBy(x => x.SquareLength);
                    List<Rhino.Geometry.Vector3d> orderedVectors = orderedVectorsEnumerable.ToList<Rhino.Geometry.Vector3d>();


                    if (orderedVectors[0].SquareLength == 0) { orderedVectors.RemoveAt(0); }
                    //if (tempVectors.Count > 0) { vector = tempVectors[0]; }
                    if (orderedVectors.Count > 0)
                    {
                        foreach (Rhino.Geometry.Vector3d tempVector in orderedVectors)
                        {
                            if (tempVector.SquareLength > 0) { vector += tempVector * (maxDistance / tempVector.Length); } else { vector += new Rhino.Geometry.Vector3d(1, 0, 0) * maxDistance; }
                        }
                        vector /= tempVectors.Count;
                    }
                }

                newVectors.Add(vector * multipliers[(i >= multipliers.Count) ? multipliers.Count - 1 : i]);
            }

            // output
            DA.SetDataList(0, newVectors);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.repulse;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920030}"); }
        }
    }
}
