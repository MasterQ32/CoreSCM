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
            var r1 = schem.AddInstance("R1", Component.Resistor);

            var vcc = schem.AddSignal("VCC");
            var gnd = schem.AddSignal("GND");

            vcc.Attach(cpu.GetFunction("VCC"));
            gnd.Attach(cpu.GetFunction("GND"));

            vcc.Attach(r1.GetFunction("A"));
            var anon = schem.AddAnonymousSignal();
            anon.Attach(r1.GetFunction("B"));
            anon.Attach(cpu.GetFunction("/RESET"));
            
            foreach(var element in Enumerable.Zip(Range.Unwrap("A[0..1].X[3,7]"), Range.Unwrap("B[3..0]"), (a,b) => a + " -- " + b))
            {
                Console.WriteLine(element);
            }
        }
    }
}
