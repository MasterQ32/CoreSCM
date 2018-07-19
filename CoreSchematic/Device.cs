using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CoreSchematic
{
    public sealed class Device : Component
    {
        private readonly Dictionary<string, DeviceConfiguration> packages = new Dictionary<string, DeviceConfiguration>();

        public Device(string name) : base(name)
        {
            
        }

        public static Device Load(string fileName)
        {
            using (var xmlReader = XmlReader.Create(fileName))
                return Load(xmlReader);
        }

        public static Device Load(XmlReader xmlReader)
        {
            var ser = new XmlSerializer(typeof(XML));
            var obj = (XML)ser.Deserialize(xmlReader);

            var dev = new Device(obj.Name);

            foreach(var protopack in obj.Packages ?? throw new InvalidDataException("Device requires at least one package!"))
            {
                if (dev.packages.TryGetValue(protopack.ID, out DeviceConfiguration cfg))
                    throw new InvalidDataException($"Package {protopack.ID} was declared twice!");

                var package = PackageFactory.CreatePackage(protopack.ID);

                cfg = new DeviceConfiguration(dev, package);

                foreach (var protopin in protopack.Bindings ?? throw new InvalidDataException("Package requires at least one pin definition!"))
                {
                    var fun = dev.GetFunction(protopin.Name);
                    if (fun == null)
                        fun = dev.AddFunction(protopin.Name);

                    var exclusive = (protopin.BindMode == BindMode.All);

                    foreach (var loc in protopin.Location.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var pin = package.GetPin(loc);
                        cfg.Bindings[pin].Bind(fun, exclusive);
                    }
                }
            

                dev.packages.Add(package.Name, cfg);
            }


            return dev;
        }


        [XmlRoot("device", Namespace = null)]
        public class XML
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlElement("package")]
            public XMLPackage[] Packages { get; set; }
        }

        public class XMLPackage
        {
            [XmlAttribute("id")]
            public string ID { get; set; }

            [XmlElement("pin")]
            public XMLPin[] Bindings { get; set; }
        }

        public class XMLPin
        {
            [XmlAttribute("function")]
            public string Name { get; set; }

            [XmlAttribute("loc")]
            public string Location { get; set; }

            [XmlAttribute("bind")]
            public BindMode BindMode { get; set; } = BindMode.Any;
        }

        public enum BindMode
        {
            [XmlEnum("any")]
            Any = 0,

            [XmlEnum("all")]
            All = 1,
        }
    }
}