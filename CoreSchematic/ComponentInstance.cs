using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
    public sealed class ComponentInstance
    {
    	private readonly Dictionary<string, Attribute> attributes = new Dictionary<string, Attribute>();
    
        public ComponentInstance(Schematic schematic, string name, Component component)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            this.Name = name;
            this.Schematic = schematic ?? throw new ArgumentNullException(nameof(schematic));
            this.Component = component ?? throw new ArgumentNullException(nameof(component));
            this.Functions = this.Component.Functions.Select(p => p.MakeInstance(this)).ToArray();
        }

		public void AddAttribute(Attribute value)
		{
			this.attributes.Add(value.Name, value);
		}

        public Function GetFunction(string name) => this.Functions.SingleOrDefault(f => f.Name == name);

        public string Name { get; }

        public Schematic Schematic { get; }

        public Component Component { get; }

        public IReadOnlyList<Function> Functions { get; }
        
        public IReadOnlyDictionary<string, Attribute> Attributes => this.attributes;

        public override string ToString() => $"{Name} : {Component}";
	}
}