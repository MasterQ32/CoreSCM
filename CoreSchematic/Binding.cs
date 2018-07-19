using System;
using System.Collections.Generic;

namespace CoreSchematic
{
    /// <summary>
    /// Binds one or more functions to a certain pin on a package
    /// </summary>
    public sealed class Binding
    {
        private readonly List<Function> functions = new List<Function>();

        public Binding(DeviceConfiguration deviceConfiguration, Pin pin)
        {
            this.Configuration = deviceConfiguration ?? throw new ArgumentNullException(nameof(deviceConfiguration));
            this.Pin = pin ?? throw new ArgumentNullException(nameof(pin));
        }

        public void Bind(Function fun, bool exlusive)
        {
            if (fun == null)
                throw new ArgumentNullException(nameof(fun));
            if (IsExclusive || (exlusive && (functions.Count > 0)))
                throw new InvalidOperationException($"Cannot bind {fun} to {Pin} exlusively: Pin has already another binding!");
            this.IsExclusive = exlusive;
            this.functions.Add(fun);
        }

        public DeviceConfiguration Configuration { get; }

        public Device Device => this.Configuration.Device;

        public Pin Pin { get; }

        public bool IsExclusive { get; private set; }

        public IReadOnlyCollection<Function> Functions { get; }

        public override string ToString() => $"{Pin} => ({string.Join(",", Functions)})";
    }
}