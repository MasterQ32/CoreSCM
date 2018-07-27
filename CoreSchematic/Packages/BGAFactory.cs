using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreSchematic.Packages
{
    [PackageFactory]
    public sealed class BGAFactory : PackageFactory
    {
        const string xcoords = "ABCDEFHKGJLMNPRT";

        public BGAFactory() : base(new Regex(@"^.*?BGA\-?(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {

        }

        public override Package Create(string id)
        {
            var m = pattern.Match(id);
            if (!m.Success)
                throw new ArgumentException("id does not match!", "id");
            int count = int.Parse(m.Groups[1].Value);

            int size = (int)Math.Sqrt(count);
            if (size * size != count)
                throw new InvalidOperationException($"{id} is not a square package!");

            var pins = new List<string>();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    pins.Add(string.Format("{0}{1}", xcoords[x], (y + 1)));
                }
            }

            return new Package(id.ToUpper(), pins.ToArray());
        }
    }
}
