using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreSchematic.Packages
{
    [PackageFactory]
    public sealed class MLF : PackageFactory
    {
        public MLF() : base(new Regex(@"^MLF\-?(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {

        }

        public override Package Create(string id)
        {
            var m = pattern.Match(id);
            if (!m.Success)
                throw new ArgumentException("id does not match!", "id");
            int count = int.Parse(m.Groups[1].Value);
            return new Package($"MLF-{count}", count);
        }
    }
}
