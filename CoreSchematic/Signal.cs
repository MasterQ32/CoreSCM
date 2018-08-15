using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
	/// <summary>
	/// A collection of connected pins.
	/// </summary>
	public sealed class Signal
	{
		private readonly List<Function> attachments = new List<Function>();

		public Signal(Schematic schematic)
		{
			this.Schematic = schematic ?? throw new ArgumentNullException(nameof(schematic));
			this.Name = null;
		}

		public Signal(Schematic schematic, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentOutOfRangeException(nameof(name));
			this.Schematic = schematic ?? throw new ArgumentNullException(nameof(schematic));
			this.Name = name;
		}

		public void Attach(Function fun)
		{
			if (fun == null)
				throw new ArgumentNullException(nameof(fun));
			if (fun.IsInstanced == false)
				throw new ArgumentException("Pin is abstract and cannot be attached!");
			if (fun.Schematic != this.Schematic)
				throw new ArgumentException("Pins component instance is not located in this schematic!");

			if (this.attachments.Contains(fun) == false)
				this.attachments.Add(fun);
			if (fun.AttachedSignals.Contains(this) == false)
				fun.AttachedSignals.Add(this);
		}

		public void Detach(Function fun)
		{
			if (fun == null)
				throw new ArgumentNullException(nameof(fun));
			if (fun.IsInstanced == false)
				throw new ArgumentException("Pin is abstract and cannot be attached!");
			if (fun.Schematic != this.Schematic)
				throw new ArgumentException("Pins component instance is not located in this schematic!");

			this.attachments.Remove(fun);
			fun.AttachedSignals.Remove(this);
		}

		public Schematic Schematic { get; }

		public string Name { get; }

		public IReadOnlyList<Function> Attachments => this.attachments;

		public override string ToString() => this.Name ?? "<Anonymous Signal>";
	}
}
