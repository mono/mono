using System.Linq;
using System.Text;

namespace CoreClr.Tools
{
	public static class Compatibility
	{
		public static string ParseMoonlightAuditFormat(string text)
		{
			var sb = new StringBuilder();

			//lame parsing method. all methods have ::.  some comments also refer to methods. however methodlines
			//should only have a single space.
			foreach (var s in text.NonEmptyLines().Where(s => (s.Contains("::") && s.Split(' ').Length==2 )))
				sb.AppendLine(s);

			return sb.ToString();
		}
	}
}
