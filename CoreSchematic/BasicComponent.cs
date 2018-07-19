using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSchematic
{
    public sealed class BasicComponent : Component
    {
        internal BasicComponent(string name, bool bipolar) : base(name)
        {
            if (bipolar)
            {
                this.AddFunction("+");
                this.AddFunction("-");
            }
            else
            {
                this.AddFunction("A");
                this.AddFunction("B");
            }
        }
    }
}
