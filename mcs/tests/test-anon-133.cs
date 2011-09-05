using System;
using System.Collections.Generic;
using System.Text;

public static class IEnumerableRocks
{

	public static string Implode<TSource, TResult> (this IEnumerable<TSource> self, string separator, Func<TSource, TResult> selector)
	{
		return Implode (self, separator, (b, e) => { b.Append (selector (e).ToString ()); });
	}

	public static string Implode<TSource> (this IEnumerable<TSource> self, string separator, Action<StringBuilder, TSource> appender)
	{
		var coll = self as ICollection<TSource>;
		if (coll != null && coll.Count == 0)
			return string.Empty;

		bool needSep = false;
		var s = new StringBuilder ();

		foreach (var element in self) {
			if (needSep && separator != null)
				s.Append (separator);

			appender (s, element);
			needSep = true;
		}

		return s.ToString ();
	}
}

class Test
{
	public static void Main ()
	{
		Console.WriteLine (new [] { "foo", "bar" }.Implode (", ", e => "'" + e + "'"));
	}
}
