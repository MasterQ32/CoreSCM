using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    public sealed class ComponentInstance
    {
        public ComponentInstance(Schematic schematic, string name, Component component)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Name = name;
            this.Schematic = schematic ?? throw new ArgumentNullException(nameof(schematic));
            this.Component = component ?? throw new ArgumentNullException(nameof(component));
            this.Pins = this.Component.Pins.Select(p => p.MakeInstance(this)).ToArray();
        }

        public string Name { get; }

        public Schematic Schematic { get; }

        public Component Component { get; }

        public IReadOnlyList<Pin> Pins { get; }

        public override string ToString() => $"{Name} : {Component}";
    }
}