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

            foreach(var package in obj.Packages ?? throw new InvalidDataException("Device requires at least one package!"))
            {
                if (dev.packages.TryGetValue(package.ID, out DeviceConfiguration cfg))
                    throw new InvalidDataException($"Package {package.ID} was declared twice!");
                cfg = new DeviceConfiguration();
                cfg.Package = package.ID;

                foreach (var pin in package.Pins ?? throw new InvalidDataException("Package requires at least one pin definition!"))
                {
                    var p = dev.GetPin(pin.Name);
                    if (p == null)
                        p = dev.AddPin(pin.Name);

                    foreach (var loc in pin.Location.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        cfg.Pins.Add(Tuple.Create(p, int.Parse(loc)));
                    }
                }
            

                dev.packages.Add(package.ID, cfg);
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
            public XMLPin[] Pins { get; set; }
        }

        public class XMLPin
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("loc")]
            public string Location { get; set; }
        }
    }
}