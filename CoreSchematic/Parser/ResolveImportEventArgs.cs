using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic.Parser
{
	public sealed class ResolveImportEventArgs : EventArgs
	{
		public ResolveImportEventArgs(string[] path)
		{
			this.Path = path.ToArray();
		}

		public IReadOnlyCollection<Component> Library { get; set; }

		public IReadOnlyList<string> Path { get; }
	}
}