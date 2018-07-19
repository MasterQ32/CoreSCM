using System;

namespace CoreSchematic
{
    /// <summary>
    /// A pin that is attached to a certain package
    /// </summary>
    public class Pin
    {
        internal Pin(Package package, string name)
        {
            this.Package = package ?? throw new ArgumentNullException(nameof(package));
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Package Package { get; }

        public string Name { get; }

        public override string ToString() => $"{Package.Name}.{Name}";
    }
}