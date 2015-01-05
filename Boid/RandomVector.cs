using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class RandomVector : GH_Component
    {
        public RandomVector()
            : base("Random Vector", "RndVec",
                "Generate random vector. (Double randomization)",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number", "N", "Number of random vectors.", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Unit", "U", "Make unit vectors - all exactly 1 unit long (before multiplication).", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Random seed", "S", "Random seed for the newly created vecotrs.", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("Absolute multiplier", "*", "Output vector length multiplier. By default the vectors maintain their original length between 0..1 or exactly 1 if unit. Less than 0 = reversed random vector, 0 = zero length vector, 1 = original random vector, above 1 = larger than original vector", GH_ParamAccess.list, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Random vector(s)", "V", "Random vector(s)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare a variable for the input
            int count = 0;
            Boolean unit = false;
            int seed = 2;
            List<double> multipliers = new List<double>();

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            // Use the DA object to retrieve the data inside the first input parameter.
            // If the retieval fails (for example if there is no data) we need to abort.
            if (!DA.GetData(0, ref count)) { return; }
            if (!DA.GetData(1, ref unit)) { return; }
            if (!DA.GetData(2, ref seed)) { return; }
            if (!DA.GetDataList(3, multipliers)) { return; }

            Random rnd = new Random(seed);
            seed = rnd.Next(int.MaxValue);
            rnd = new Random(seed);

            for (int i = 0; i < count; i++)
            {
                Rhino.Geometry.Vector3d vector = new Rhino.Geometry.Vector3d(2*(rnd.NextDouble() - 0.5), 2* (rnd.NextDouble() - 0.5), 2*(rnd.NextDouble() - 0.5));
                if (unit) { vector.Unitize(); }
                newVectors.Add(vector * multipliers[(i >= multipliers.Count) ? multipliers.Count - 1 : i]);
            }

            // output
            DA.SetDataList(0, newVectors);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.random_vector;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920031}"); }
        }
    }
}
