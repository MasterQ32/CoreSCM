using System;
using System.Collections.Generic;

namespace CoreSchematic
{
	/// <summary>
	/// A net containing pin attachments of different PCB parts.
	/// </summary>
	public sealed class Net : List<PinAttachment>
	{
		public Net(string name)
		{
			this.Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public string Name { get; }
	}
}