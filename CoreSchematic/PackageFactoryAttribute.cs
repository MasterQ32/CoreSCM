using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSchematic
{
    /// <summary>
    /// Marks a class as a package factory that may be imported by CoreSchematic
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PackageFactoryAttribute : Attribute
    {
        public PackageFactoryAttribute()
        {
        }
    }
}
