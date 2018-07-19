using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    public abstract class Component
    {
        public static readonly Component Resistor = new BasicComponent("R", false);
        public static readonly Component UnipolarCapacitor = new BasicComponent("C", false);
        public static readonly Component BipolarCapacitor = new BasicComponent("C", false);
        public static readonly Component Inductivity = new BasicComponent("L", false);

        private readonly List<Function> functions = new List<Function>();

        protected Component(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            this.Name = name;
        }

        protected Function AddFunction(string name)
        {
            var p = new Function(this, name);
            this.functions.Add(p);
            return p;
        }

        public Function GetFunction(string name) => this.functions.SingleOrDefault(p => p.Name == name);

        public IReadOnlyList<Function> Functions => this.functions;

        public string Name { get; }

        public override string ToString() => this.Name;
    }
}