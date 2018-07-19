using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreSchematic
{
    /// <summary>
    /// A factory that can create packages by a given identifier.
    /// </summary>
    public abstract class PackageFactory
    {
        protected readonly Regex pattern;

        protected PackageFactory()
        {

        }

        protected PackageFactory(Regex pattern)
        {
            this.pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        /// <summary>
        /// Checks if this factory can create the given package id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool CanCreate(string id)
        {
            if (pattern != null)
                return pattern.IsMatch(id);
            else
                throw new NotSupportedException("No pattern provided!");
        }

        /// <summary>
        /// Creates a new package based on the package id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Package Create(string id);


        private static readonly List<PackageFactory> registeredFactories = new List<PackageFactory>();

        public static Package CreatePackage(string id)
        {
            if (registeredFactories.Count == 0)
                LoadPackageFactories();

            var factory = registeredFactories.FirstOrDefault(f => f.CanCreate(id));
            if (factory == null)
                throw new ArgumentOutOfRangeException(nameof(id), "A package with this id does not exist!");

            return factory.Create(id);
        }

        private static void LoadPackageFactories()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    LoadPackageFactories(asm);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                }
            }

        }

        private static void LoadPackageFactories(Assembly asm)
        {
            foreach (var type in asm.ExportedTypes)
            {
                if (type.IsSubclassOf(typeof(PackageFactory)) == false)
                    continue;
                if (type.GetCustomAttribute<PackageFactoryAttribute>() == null)
                    continue;
                if (type.IsAbstract || type.IsInterface)
                    continue;
                var pf = (PackageFactory)Activator.CreateInstance(type);
                registeredFactories.Add(pf);
            }
        }
    }
}
