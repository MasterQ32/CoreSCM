using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSchematic
{
    /// <summary>
    /// A package that defines a set of pins
    /// </summary>
    public sealed class Package
    {
        public Package(string name, string[] pins)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Pins = pins.Select(p => new Pin(this, p)).ToArray();
        }

        public Package(string name, int pincount)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Pins = Enumerable.Range(1, pincount).Select(p => new Pin(this, p.ToString())).ToArray();
        }

        public Pin GetPin(string name) => this.Pins.FirstOrDefault(p => p.Name == name);

        public string Name { get; }

        public IReadOnlyList<Pin> Pins { get; }
    }
}
