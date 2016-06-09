using System.Text.RegularExpressions;

namespace Mono
{
	class StackTraceMetadata
	{
		static Regex regex = new Regex (@"\[(?<Id>.+)\] (?<Value>.+)");

		public readonly string Id;
		public readonly string Value;
		public readonly string Line;

		private StackTraceMetadata (string line, string id, string val)
		{
			Line = line;
			Id = id;
			Value = val;
		}
	
		public static bool TryParse (string line, out StackTraceMetadata metadata)
		{
			metadata = null;

			var match = regex.Match (line);
			if (!match.Success)
				return false;

			string id = match.Groups ["Id"].Value;
			string val = match.Groups ["Value"].Value;

			metadata = new StackTraceMetadata (line, id, val);

			return true;
		}
	}
}
