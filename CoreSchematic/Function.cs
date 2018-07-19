using System;
using System.Collections.Generic;

namespace CoreSchematic
{
    /// <summary>
    /// A function is a named feature of a pin that may be shared one-to-many (one function, multiple pins)
    /// or many-to-one (multiple functions, single pin).
    /// </summary>
    public sealed class Function
    {
        public Function(Component component, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Name = name;
            this.Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        private Function(ComponentInstance instance, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Name = name;
            this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.Component = instance.Component;
            this.AttachedSignals = new HashSet<Signal>();
        }

        /// <summary>
        /// Creates an instance of this function for a given component instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Function MakeInstance(ComponentInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (instance.Component != this.Component)
                throw new ArgumentException("The given instance does not match this pins componetn!");
            return new Function(instance, this.Name);
        }

        public string Name { get; }

        /// <summary>
        /// Gets the component this function is contained in
        /// </summary>
        public Component Component { get; }

        /// <summary>
        /// Gets if the function is an instance function.
        /// </summary>
        public bool IsInstanced => (this.Instance != null);

        /// <summary>
        /// Gets the component instance for this function.
        /// </summary>
        public ComponentInstance Instance { get; }

        /// <summary>
        /// Gets the schematic this function instance is contained in.
        /// </summary>
        public Schematic Schematic => this.Instance?.Schematic;

        /// <summary>
        /// Gets a collection of all attached signals
        /// </summary>
        public ICollection<Signal> AttachedSignals { get; }

        public override string ToString() => IsInstanced ? $"{Instance}.{Name}" : $"{Component}.{Name}";
    }
}