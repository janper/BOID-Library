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
            pManager.AddBooleanParameter("Unit", "U", "Make unit vectors.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Random seed", "S", "Random seed for the newly created vecotrs.", GH_ParamAccess.item, 2);
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

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            // Use the DA object to retrieve the data inside the first input parameter.
            // If the retieval fails (for example if there is no data) we need to abort.
            if (!DA.GetData(0, ref count)) { return; }
            if (!DA.GetData(1, ref unit)) { return; }
            if (!DA.GetData(2, ref seed)) { return; }

            Random rnd = new Random(seed);
            seed = rnd.Next(int.MaxValue);
            rnd = new Random(seed);

            for (int i = 0; i < count; i++)
            {
                Rhino.Geometry.Vector3d vector = new Rhino.Geometry.Vector3d(2*(rnd.NextDouble() - 0.5), 2* (rnd.NextDouble() - 0.5), 2*(rnd.NextDouble() - 0.5));
                if (unit) { vector.Unitize(); }
                newVectors.Add(vector);
            }

            // output
            DA.SetDataList(0, newVectors);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resource1.random_vector;
                //return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920031}"); }
        }
    }
}
