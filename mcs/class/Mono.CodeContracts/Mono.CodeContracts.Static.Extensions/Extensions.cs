using System.Collections.Generic;
using System.Linq;

namespace Mono.CodeContracts.Static.Extensions {
	static class Extensions {
		public static string ToString<T>(this IEnumerable<T> values, string separator)
		{
			return string.Join (separator, values.Select (v => v.ToString ()));
		}
	}
}