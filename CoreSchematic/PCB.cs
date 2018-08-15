using System.Collections.Generic;

namespace CoreSchematic
{
	/// <summary>
	/// A PCB board description.
	/// </summary>
	public sealed class PCB
	{
		public PCB()
		{

		}
		
		public ICollection<Part> Parts { get; } = new HashSet<Part>();

		public ICollection<Net> NetList { get; } = new HashSet<Net>();
	}
}