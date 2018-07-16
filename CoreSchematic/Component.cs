using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    public abstract class Component
    {
        private readonly List<Pin> pins = new List<Pin>();

        protected Component(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            this.Name = name;
        }

        protected Pin AddPin(string name)
        {
            var p = new Pin(this, name);
            this.pins.Add(p);
            return p;
        }

        public Pin GetPin(string name) => this.pins.SingleOrDefault(p => p.Name == name);

        public IReadOnlyList<Pin> Pins => this.pins;

        public string Name { get; }

        public override string ToString() => this.Name;
    }
}