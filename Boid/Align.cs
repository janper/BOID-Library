using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Boid
{
    public class Align : GH_Component
    {

        public Align()
            : base("Align to the flock members", "Align",
                "Align agent's motion direction to correspond the average motion direction of the flock.",
                "Boid", "Legacy")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Agent motion vector", "V", "Current motion vectors of the agents", GH_ParamAccess.item);
            pManager.AddVectorParameter("Flock motion vectors", "FV", "Current motion vectors of the flock", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplier", "*", "Output vector length multiplier. Optimal values around 0.10. Less than 0 = negative effect, 0 = no motion, 1 = immediate effect, above 1 = overdone effect", GH_ParamAccess.item, 0.1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Aligned motion vector", "V", "Motion vector aligned to the average motion vector of the flock", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare variables for the input
            Rhino.Geometry.Vector3d vector = new Rhino.Geometry.Vector3d();
            List<Rhino.Geometry.Vector3d> flock = new List<Rhino.Geometry.Vector3d>();
            double multiplier = 1;

            // Daclare a variable for the output
            List<Rhino.Geometry.Vector3d> newVectors = new List<Rhino.Geometry.Vector3d>();

            //check inputs
            if (!DA.GetData(0, ref vector)) { return; }
            if (!DA.GetDataList(1, flock)) { return; }
            if (!DA.GetData(2, ref multiplier)) { return; }

            if ((flock.Count == 0) || (flock == null)) { return; }

            //for each point in the list of points

            Rhino.Geometry.Vector3d avgVector = new Rhino.Geometry.Vector3d(0,0,0);

            foreach (Rhino.Geometry.Vector3d currentVector in flock)
            {
                avgVector += currentVector;
            }
            avgVector /= flock.Count;

            DA.SetData(0, avgVector*multiplier);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resource1.align;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{b5d980b2-b340-45ca-8530-afeccd920032}"); }
        }
    }
}
