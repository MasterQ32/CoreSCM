using System;

namespace CoreSchematic
{
    public sealed class Pin
    {
        public Pin(Component component, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Name = name;
            this.Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        private Pin(ComponentInstance instance, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Name = name;
            this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.Component = instance.Component;
        }

        public Pin MakeInstance(ComponentInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (instance.Component != this.Component)
                throw new ArgumentException("The given instance does not match this pins componetn!");
            return new Pin(instance, this.Name);
        }

        public string Name { get; }

        public Component Component { get; }

        public bool IsInstanced => (this.Instance != null);

        public ComponentInstance Instance { get; }

        public Schematic Schematic => this.Instance?.Schematic;

        public override string ToString() => IsInstanced ? $"{Instance}.{Name}" : $"{Component}.{Name}";
    }
}