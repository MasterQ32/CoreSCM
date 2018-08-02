using System.Collections.Generic;

namespace CoreSchematic
{
	public static class Libraries
	{
		public static List<Component> Generic { get; }
		
		static Libraries()
		{
			var generic = new List<Component>();
			generic.Add(new Device("BAT")
			{
				"Plus", "Minus"
			});
		}
	}
}