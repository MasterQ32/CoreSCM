using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSchematic
{
    class Program
    {
        static void Main(string[] args)
        {
            var atmega32 = Device.Load("Devices/atmega32.xml");

            var schem = new Schematic("main");

            var cpu = schem.AddInstance("cpu", atmega32);
            
        }
    }
}
