using System.Text.RegularExpressions;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public class MethodDefinitionFilter
	{
		private Regex regex;

		public MethodDefinitionFilter(string filter)
		{
			regex = new Regex(filter);
		}

		public bool Match(MethodDefinition m)
		{
			if (regex.Match(m.DeclaringType.FullName).Success)
				return true;
			return false;
		}
	}
}
