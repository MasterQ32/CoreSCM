using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    public sealed class Schematic : Component
    {
        private readonly List<ComponentInstance> instances = new List<ComponentInstance>();
        private readonly List<Signal> signals = new List<Signal>();

        public Schematic(string name) : base(name)
        {

        }

        public ComponentInstance AddInstance(string name, Component component)
        {
            if (GetInstance(name) != null)
                throw new ArgumentException("Another instance with this name already exists!", nameof(name));

            var ci = new ComponentInstance(this, name, component);
            this.instances.Add(ci);
            return ci;
        }

        public Signal AddAnonymousSignal()
        {
            var sig = new Signal(this);
            this.signals.Add(sig);
            return sig;
        }

        public Signal AddSignal(string name)
        {
            var sig = new Signal(this, name);
            this.signals.Add(sig);
            return sig;
        }

        public Signal GetSignal(string name) => this.signals.SingleOrDefault(s => s.Name == name);

        public ComponentInstance GetInstance(string name) => this.instances.SingleOrDefault(i => i.Name == name);

        public IReadOnlyList<Signal> Signals => this.signals;

        public IReadOnlyList<ComponentInstance> Components => this.instances;
    }
}