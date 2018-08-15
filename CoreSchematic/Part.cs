using System;

namespace CoreSchematic
{
	/// <summary>
	/// A part on a PCB.
	/// </summary>
	public sealed class Part
	{
		public Part(ComponentInstance component, Package package, string name)
		{
			this.Component = component ?? throw new ArgumentNullException(nameof(component));
			this.Package = package ?? throw new ArgumentNullException(nameof(package));
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public ComponentInstance Component { get; }
		public Package Package { get; }
		public string Name { get; }
	}
}