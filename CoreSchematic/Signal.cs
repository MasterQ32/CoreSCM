using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    /// <summary>
    /// A collection of connected pins.
    /// </summary>
    public sealed class Signal
    {
        private readonly List<Pin> attachments = new List<Pin>();

        public Signal(Schematic schematic)
        {
            this.Schematic = schematic ?? throw new ArgumentNullException(nameof(schematic));
            this.Name = null;
        }

        public Signal(Schematic schematic, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Schematic = schematic ?? throw new ArgumentNullException(nameof(schematic));
            this.Name = name;
        }

        public void Attach(Pin pin)
        {
            if (pin == null)
                throw new ArgumentNullException(nameof(pin));
            if (pin.IsInstanced == false)
                throw new ArgumentException("Pin is abstract and cannot be attached!");
            if (pin.Schematic != this.Schematic)
                throw new ArgumentException("Pins component instance is not located in this schematic!");

            if (this.attachments.Contains(pin) == false)
                this.attachments.Add(pin);
        }

        public void Detach(Pin pin)
        {
            if (pin == null)
                throw new ArgumentNullException(nameof(pin));
            if (pin.IsInstanced == false)
                throw new ArgumentException("Pin is abstract and cannot be attached!");
            if (pin.Schematic != this.Schematic)
                throw new ArgumentException("Pins component instance is not located in this schematic!");

            this.attachments.Remove(pin);
        }

        public Schematic Schematic { get; }

        public string Name { get; }

        public IReadOnlyList<Pin> Attachments => this.attachments;

        public override string ToString() => this.Name ?? "<Anonymous Signal>";
    }
}
