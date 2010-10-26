using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreClr.Tools
{
	public static class Strings
	{
		public static IEnumerable<string> NonEmptyLines(this string signatures)
		{
			return signatures.SplitLines()
				.Where(s => s.Length > 0);
		}

		public static IEnumerable<string> SplitLines(this string signatures)
		{
			return signatures
				.Split('\n')
				.Select(s => s.Trim());
		}

		public static string JoinLines(this IEnumerable<string> self, string linePrefix)
		{
			var sb = new StringBuilder();
			foreach (var line in self)
			{
				sb.Append(linePrefix);
				sb.AppendLine(line);
			}
			return sb.ToString();
		}
	}
}
