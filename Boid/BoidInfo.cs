using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Boid
{
    public class BoidInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Boid";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resource1.repulse;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("77d433bc-8eb4-444e-ac0a-ab9fba679874");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Jan Pernecky | rese arch";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "www.rese-arch.org";
            }
        }
    }
}
