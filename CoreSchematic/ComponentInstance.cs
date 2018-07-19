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
            this.Functions = this.Component.Functions.Select(p => p.MakeInstance(this)).ToArray();
        }

        public Function GetFunction(string name) => this.Functions.SingleOrDefault(f => f.Name == name);

        public string Name { get; }

        public Schematic Schematic { get; }

        public Component Component { get; }

        public IReadOnlyList<Function> Functions { get; }

        public override string ToString() => $"{Name} : {Component}";
    }
}