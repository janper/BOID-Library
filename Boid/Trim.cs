using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Trim : GH_Component
    {

        public Trim()
            : base("Trim vector", "TrmVec",
                "Trim vector(s) not to exceed specific length limits.",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Length domain", "D", "Minimum and maximum length of a vector. Values below the domain minimum will be set to the domain minimum, values above the domain maximum will be set to the domain maximum. Zero length will remain zero. Domain maximum = -1 -> no upper limit.", GH_ParamAccess.list, new Rhino.Geometry.Interval(0, -1));
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Trimmed vector", "V", "Vector trimmed to fit the domain.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables for the input

            List<Rhino.Geometry.Vector3d> vectors = new List<Rhino.Geometry.Vector3d>();
            List<Rhino.Geometry.Interval> intervals = new List<Rhino.Geometry.Interval>();

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            //check inputs
            if (!DA.GetDataList(0, vectors)) { return; }
            if (!DA.GetDataList(1, intervals)) { return; }

            if ((vectors == null) || (intervals == null)) { return; }

            //for each point in the list of points

            int max = Math.Max(vectors.Count, intervals.Count);

            for (int i = 0; i < max; i++)
            {
                int vectorIndex = (i > vectors.Count - 1) ? vectors.Count - 1 : i;
                int intervalIndex = (i > intervals.Count - 1) ? intervals.Count - 1 : i;

                double minSquaredLength = intervals[intervalIndex].T0 * intervals[intervalIndex].T0;
                double maxSquaredLength = intervals[intervalIndex].T1 * intervals[intervalIndex].T1;

                Rhino.Geometry.Vector3d tempVector = vectors[vectorIndex];

                if ((tempVector.SquareLength < minSquaredLength) && !tempVector.IsZero) { tempVector *= intervals[intervalIndex].T0 / tempVector.Length; }
                if ((tempVector.SquareLength > maxSquaredLength)&&(intervals[intervalIndex].T1>=0) && !tempVector.IsZero) { tempVector *= intervals[intervalIndex].T1 / tempVector.Length; }

                newVectors.Add(tempVector);
            }

            DA.SetDataList(0, newVectors);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.trim;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920033}"); }
        }
    }
}
