using System;
using System.Collections.Generic;

namespace CoreSchematic
{
	public sealed class Attribute
	{
		public Attribute(string name)
		{
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public string Name { get; }
		
		public IList<string> Values { get; } = new List<string>();

		public override string ToString() => $"{Name} = {string.Join(", ", Values)}";
	}
}
