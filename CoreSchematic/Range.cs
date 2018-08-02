using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreSchematic
{
	public static class Range
	{
		public static string[] Unwrap(string cfg)
		{
			var ranges = new List<string>();

			int start = 0;
			do
			{
				// Find start of descriptor
				int idx = cfg.IndexOf('[', start);
				if (idx < 0)
				{
					if (ranges.Count == 0)
						return new[] { cfg };
					return ranges.ToArray();
				}
				// Find end of descriptor
				var end = cfg.IndexOf(']', idx);
				if (end < 0) // last [ is not a descriptor, concat end to all sequences
					return ranges.Select(r => r + cfg.Substring(start)).ToArray();

				var prefix = cfg.Substring(start, idx - start);

				var desc = cfg.Substring(idx + 1, end - idx - 1);

				var source = ranges;
				ranges = new List<string>();

				var append = new Action<int>((i) =>
				{
					if (source.Count > 0)
					{
						foreach (var str in source)
							ranges.Add(str + prefix + "[" + i + "]");
					}
					else
					{
						ranges.Add(prefix + "[" + i + "]");
					}
				});

				foreach (var elem in desc.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					var split = elem.IndexOf("..");
					if (split >= 0)
					{
						var rStart = elem.Substring(0, split);
						var rEnd = elem.Substring(split + 2);
						var low = int.Parse(rStart);
						var high = int.Parse(rEnd);
						if (high >= low)
						{
							for (int i = low; i <= high; i++)
								append(i);
						}
						else
						{
							for (int i = low; i >= high; i--)
								append(i);
						}
					}
					else
					{
						append(int.Parse(elem));
					}
				}

				start = end + 1;

			} while (start < cfg.Length);
			return ranges.ToArray();
		}
	}
}