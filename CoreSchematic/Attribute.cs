using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
	public sealed class Attribute
	{
		public Attribute(string name)
		{
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public Attribute(string name, string value) : this(name)
		{
			this.Values.Add(value);
		}

		public string Value => this.Values.SingleOrDefault() ?? throw new InvalidOperationException("The attribute does not have a single value!");

		public string Name { get; }

		public IList<string> Values { get; } = new List<string>();

		public override string ToString() => $"{Name} = {string.Join(", ", Values)}";
	}
}
