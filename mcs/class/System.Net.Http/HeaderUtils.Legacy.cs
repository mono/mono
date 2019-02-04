using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http
{
	static class HeaderUtils
	{
		internal static bool IsContentHeader (string name)
		{
			return HttpHeaders.GetKnownHeaderKind (name) == Headers.HttpHeaderKind.Content;
		}

		internal static string GetSingleHeaderString (string name, IEnumerable<string> values)
		{
			return HttpRequestHeaders.GetSingleHeaderString (name, values);
		}
	}
}
