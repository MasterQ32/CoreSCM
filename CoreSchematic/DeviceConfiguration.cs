using System;
using System.Collections.Generic;

namespace CoreSchematic
{
    public sealed class DeviceConfiguration
    {
        private readonly List<Tuple<Pin,int>> pins = new List<Tuple<Pin,int>>();

        public string Package { get; set; }

        public IList<Tuple<Pin, int>> Pins => this.pins;
    }
}