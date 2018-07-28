using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic.Parser
{
	public class ResolveComponentEventArgs : EventArgs
	{
		public ResolveComponentEventArgs(string[] path)
		{
			this.Path = path.ToArray();
		}

		public Component Component { get; set; }

		public IReadOnlyList<string> Path { get; }
	}
}