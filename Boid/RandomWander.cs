using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class RandomWander : GH_Component
    {
        public RandomWander()
            : base("Random Wander", "Wander",
                "Random vector within the angle domain of the current motion vector.",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Agent motion vectors", "V", "Current motion vectors of the agents. Zero length vector causes a random vector.", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Angle spread domain", "D", "Random motion angle deviation range.", GH_ParamAccess.list, new Rhino.Geometry.Interval(0, Math.PI));
            pManager.AddIntegerParameter("Random seed", "S", "Random seed for the newly created motion vectors.", GH_ParamAccess.list, 2);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Random motion vector", "V", "Random unit motion vector for within the defined range for each agent. Vector maintains current motion vector length.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare a variable for the input
            List<Rhino.Geometry.Vector3d> vectors = new List<Rhino.Geometry.Vector3d>();
            List<Rhino.Geometry.Interval> intervals = new List<Rhino.Geometry.Interval>();
            List<int> seeds = new List<int>();

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            // Use the DA object to retrieve the data inside the first input parameter.
            // If the retieval fails (for example if there is no data) we need to abort.
            if (!DA.GetDataList(0, vectors)) { return; }
            if (!DA.GetDataList(1, intervals)) { return; }
            DA.GetDataList(2, seeds);

            int max = Math.Max(vectors.Count, Math.Max(intervals.Count, seeds.Count));

            //make proper random seeds
            List<int> newSeeds = new List<int>();

            if (seeds.Count == 1)
            {
                Random rnd = new Random(seeds[0]);
                for (int i = 0; i < max; i++)
                {
                    newSeeds.Add(Math.Abs(rnd.Next(int.MaxValue)));
                }
            }
            else
            {
                for (int i = 0; i < max; i++)
                {
                    int seedIndex = (i > seeds.Count - 1) ? seeds.Count - 1 : i;
                    Random rnd = new Random(seeds[seedIndex]);
                    newSeeds.Add(rnd.Next(int.MaxValue));
                }
            }


            for (int i = 0; i < max; i++)
            {
                int vectorIndex = (i > vectors.Count - 1) ? vectors.Count - 1 : i;
                int intervalIndex = (i > intervals.Count - 1) ? intervals.Count - 1 : i;

                Rhino.Geometry.Vector3d vector = vectors[vectorIndex];
                Rhino.Geometry.Interval interval = intervals[intervalIndex];
                int seed = newSeeds[i];

                System.Random rnd = new System.Random(seed);

                if ((interval == null) || (vector.IsZero) || (vector == null))
                {
                    interval.T0 = 0;
                    interval.T1 = Math.PI;
                }

                if ((vector.SquareLength == 0) || (vector == null))
                {
                    vector = new Rhino.Geometry.Vector3d(rnd.NextDouble() - 0.5, rnd.NextDouble() - 0.5, rnd.NextDouble() - 0.5);
                    vector.Unitize();
                }

                Rhino.Geometry.Vector3d vector2Unit = new Rhino.Geometry.Vector3d(rnd.NextDouble() - 0.5, rnd.NextDouble() - 0.5, rnd.NextDouble() - 0.5);
                vector2Unit.Unitize();
                Rhino.Geometry.Vector3d vector1Unit = vector;
                vector1Unit.Unitize();

                while (vector2Unit.Equals(vector1Unit))
                {
                    vector2Unit = new Rhino.Geometry.Vector3d(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble());
                    vector2Unit.Unitize();
                }

                Rhino.Geometry.Vector3d rotationAxis = Vector3d.CrossProduct(vector1Unit, vector2Unit);
                double rotationAngle = interval.T0 + rnd.NextDouble() * (interval.T1 - interval.T0);
                vector.Rotate(rotationAngle, rotationAxis);

                newVectors.Add(vector);
            }

            // output
            DA.SetDataList(0, newVectors);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Resource1.wander;

            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920028}"); }
        }
    }
}
