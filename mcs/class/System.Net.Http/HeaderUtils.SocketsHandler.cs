using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http
{
	static class HeaderUtils
	{
		internal static bool IsContentHeader (string name)
		{
			return HeaderDescriptor.TryGet (name, out var descriptor) && descriptor.HeaderType == HttpHeaderType.Content;
		}

		internal static string GetSingleHeaderString (string name, IEnumerable<string> values)
		{
			string separator = HttpHeaderParser.DefaultSeparator;
			if (HeaderDescriptor.TryGet (name, out var descriptor) &&
			    (descriptor.Parser != null) && (descriptor.Parser.SupportsMultipleValues)) {
				separator = descriptor.Parser.Separator;
			}

			return string.Join (separator, values);
		}
	}
}

