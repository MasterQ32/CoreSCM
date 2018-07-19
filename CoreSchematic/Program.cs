using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreSchematic
{
    class Program
    {
        static void Main(string[] args)
        {
            var atmega32 = Device.Load("Devices/atmega32.xml");
            var as6c1616 = Device.Load("Devices/as6c1616.xml");
            var lpc18xx = Device.Load("Devices/lpc18xx.xml");

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

            foreach (var element in Enumerable.Zip(Range.Unwrap("A[0..1].X[3,7]"), Range.Unwrap("B[3..0]"), (a, b) => a + " -- " + b))
            {
                Console.WriteLine(element);
            }
        }



        /*
        class Pin
        {
            public string Name;

            public List<string> Functions = new List<string>();

            public List<string> LBGA256 = new List<string>();
            public List<string> TFBGA100 = new List<string>();
            public List<string> LQFP144 = new List<string>();
            public List<string> LQFP208 = new List<string>();
        }

        static void Main()
        {
            var rgx = new Regex("^(?<pin>(?:\"\"|\"?\\w+\"?\\/?)*)?\\s*,\\s*(?<lbga256>\\-|\"\\w{2,4},\"|\\w{2,4})?\\s*,\\s*(?<tfbga100>\\-|\"\\w{2,4},\"|\\w{2,4})?\\s*,\\s*(?<lqfp144>\\-|\"\\w{1,3},\"|\\w{1,3})?\\s*,\\s*(?<lqfp208>\\-|\"\\w{1,3},\"|\\w{1,3})?\\s*,(?<A>[^,]*),(?<B>[^,]*),(?<C>[^,]*),(?<desc>.*)$", RegexOptions.Compiled);

            var patcher = new Regex(@"([AD])(\d+)$");

            var lines = File.ReadAllLines(@"C:\Users\Felix\Desktop\Projekte\CoreSCM\Data\pinconfig-lpc18xx.csv");

            var pins = new List<Pin>();

            var functions = new Dictionary<string, List<Pin>>();

            for (int i = 0; i < lines.Length; i++)
            {
                var m = rgx.Match(lines[i]);
                if (!m.Success) throw new InvalidOperationException();
                var pin = m.Groups["pin"].Value.Replace("\"", "");
                var lbga256 = m.Groups["lbga256"].Value.Replace("\"", "");
                var tfbga100 = m.Groups["tfbga100"].Value.Replace("\"", "");
                var lqfp144 = m.Groups["lqfp144"].Value.Replace("\"", "");
                var lqfp208 = m.Groups["lqfp208"].Value.Replace("\"", "");
                var desc = m.Groups["desc"].Value.Replace("\"", "");

                Pin p;
                if (!string.IsNullOrWhiteSpace(pin))
                {
                    p = new Pin { Name = pin };
                    pins.Add(p);
                }
                else
                {
                    p = pins[pins.Count - 1];
                }

                if (!string.IsNullOrWhiteSpace(lbga256) && lbga256 != "-")
                    p.LBGA256.Add(lbga256);
                if (!string.IsNullOrWhiteSpace(tfbga100) && tfbga100 != "-")
                    p.TFBGA100.Add(tfbga100);
                if (!string.IsNullOrWhiteSpace(lqfp144) && lqfp144 != "-")
                    p.LQFP144.Add(lqfp144);
                if (!string.IsNullOrWhiteSpace(lqfp208) && lqfp208 != "-")
                    p.LQFP208.Add(lqfp208);

                if (p.LBGA256.Count == 0 && p.TFBGA100.Count == 0 && p.LQFP144.Count == 0 && p.LQFP208.Count == 0)
                    throw new InvalidOperationException();

                var idx = desc.IndexOf("—");
                if (idx > 0)
                {
                    var fun = desc.Substring(0, idx).Trim();
                    if (fun != "R")
                    {
                        fun = patcher.Replace(fun, (e) => $"{e.Groups[1].Value}[{e.Groups[2].Value}]");

                        p.Functions.Add(fun);

                        if (functions.TryGetValue(fun, out var list))
                        {
                            list.Add(p);
                        }
                        else
                        {
                            functions.Add(fun, new List<Pin>(new[] { p }));
                        }
                    }
                }
            }

            using (var file = new StreamWriter(@"C:\Users\Felix\Desktop\Projekte\CoreSCM\CoreSchematic\Devices\lpc18xx.xml", false, Encoding.UTF8))
            {
                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                file.WriteLine("<device name=\"lpc18xx\">");

                var writePackage = new Action<string, Func<Pin, IEnumerable<string>>>((a, b) =>
                {
                    file.WriteLine("\t<package id=\"{0}\">", a);
                    foreach (var fun in functions.OrderBy(f => f.Key))
                    {
                        var items = fun.Value.SelectMany(b).ToArray();
                        if (items.Length > 0)
                        {
                            file.WriteLine(
                                "\t\t<pin function=\"{0}\" loc=\"{1}\" />",
                                fun.Key,
                                string.Join(",", items));
                        }
                        else
                        {
                            Console.WriteLine("{0}, {1}", a, fun.Key);
                        }
                    }
                    file.WriteLine("\t</package>");
                });

                writePackage("LBGA-256", p => p.LBGA256);
                writePackage("TFBGA-100", p => p.TFBGA100);
                writePackage("LQFP-144", p => p.LQFP144);
                writePackage("LQFP-208", p => p.LQFP208);

                file.WriteLine("</device>");
            }

            System.Diagnostics.Debugger.Break();
        }
        */

    }
}
