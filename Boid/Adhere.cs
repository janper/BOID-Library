using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Adhere : GH_Component
    {

        public Adhere()
            : base("Ahere to the flock center", "Adhere",
                "Adhere to the flock geometrical center",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Agent location point", "P", "Current location points of the agents", GH_ParamAccess.list);
            pManager.AddPointParameter("Location points of the flock", "F", "Current location points of all the reference the agents in the flock", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Search distance", "D", "Search distance domain. x to 0 = infinity", GH_ParamAccess.list, new Rhino.Geometry.Interval(0, 0));
            pManager.AddIntegerParameter("Reference agents count", "C", "Number of closest agents to calculate the flock center. -1 = all", GH_ParamAccess.list, -1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector to the center of the flock", "V", "Motion vector towards the center of the flock", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables for the input
            List<Rhino.Geometry.Point3d> points = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Point3d> flock = new List<Rhino.Geometry.Point3d>();
            List<Rhino.Geometry.Interval> intervals = new List<Rhino.Geometry.Interval>();
            List<int> counts = new List<int>();

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            //check inputs
            if (!DA.GetDataList(0, points)) { return; }
            if (!DA.GetDataList(1, flock)) { return; }
            if (!DA.GetDataList(2, intervals)) { return; }
            if (!DA.GetDataList(3, counts)) { return; }

            if ((points.Count == 0) || (points == null)) { return; }
            if ((flock.Count == 0) || (flock == null)) { return; }

            //for each point in the list of points
            for (int i = 0; i < points.Count; i++)
            {
                int count = (i > counts.Count -1) ? counts[counts.Count - 1] : counts[i];
                if ((count > flock.Count - 1) || (count < 0)) { count = flock.Count; }

                Rhino.Geometry.Vector3d vector = new Rhino.Geometry.Vector3d(0,0,0);

                //if it should really adhere
                if (count > 0)
                {
                    List<Rhino.Geometry.Vector3d> tempVectors = new List<Rhino.Geometry.Vector3d>();

                    double minSquaredDistance = (i > intervals.Count - 1) ? intervals[intervals.Count - 1].T0 : intervals[i].T0;
                    minSquaredDistance *= minSquaredDistance;
                    double maxSquaredDistance = (i > intervals.Count - 1) ? intervals[intervals.Count - 1].T1 : intervals[i].T1;
                    maxSquaredDistance *= maxSquaredDistance;
                     

                    for (int j = 0; j < flock.Count; j++)
                    {
                        Rhino.Geometry.Vector3d tempVector = new Rhino.Geometry.Vector3d(flock[j] - points[i]);
                        if ((tempVector.SquareLength >= minSquaredDistance) && ((tempVector.SquareLength <= maxSquaredDistance) || (maxSquaredDistance <= 0))) { tempVectors.Add(tempVector); }
                    }

                    //if there are any relevant points nearby
                    if (tempVectors.Count > 0)
                    {
                        IEnumerable<Rhino.Geometry.Vector3d> orderedVectorsEnumerable = tempVectors.OrderBy(x => x.SquareLength);
                        List<Rhino.Geometry.Vector3d> orderedVectors = orderedVectorsEnumerable.ToList<Rhino.Geometry.Vector3d>();

                        if (orderedVectors[0].SquareLength == 0) { orderedVectors.RemoveAt(0); }
                        if (orderedVectors.Count > count) { orderedVectors.RemoveRange(count, orderedVectors.Count - count); }

                        foreach (Rhino.Geometry.Vector3d tempVector in orderedVectors) { vector += tempVector; }
                        vector /= count;
                    }

                }
                

                newVectors.Add(vector);
            }

            // output
            DA.SetDataList(0, newVectors);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.adhere;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920029}"); }
        }
    }
}
