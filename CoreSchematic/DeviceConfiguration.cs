using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    public sealed class DeviceConfiguration
    {
        public DeviceConfiguration(Device dev, Package package)
        {
            this.Device = dev ?? throw new ArgumentNullException(nameof(dev));
            this.Package = package ?? throw new ArgumentNullException(nameof(package));

            this.Bindings = package.Pins.Select(p => new Binding(this, p)).ToDictionary(b => b.Pin);
        }

        public Device Device { get; }

        public Package Package { get; }
        
        public IReadOnlyDictionary<Pin, Binding> Bindings { get; }
    }
}