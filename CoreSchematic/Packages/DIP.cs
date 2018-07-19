using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CoreSchematic.Packages
{
    [PackageFactory]
    public sealed class DIP : PackageFactory
    {
        public DIP() : base(new Regex(@"^DI[PL]\-?(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {
        }

        public override Package Create(string id)
        {
            var m = pattern.Match(id);
            if (!m.Success)
                throw new ArgumentException("id does not match!", "id");
            int count = int.Parse(m.Groups[1].Value);
            return new Package($"DIP-{count}", count);
        }
    }
}
