using System;
using System.Collections.Generic;
using System.Linq;

static class Test
{
	public static IEnumerable<T> Select<T> (this Array This, Func<object, T> transform)
	{
		foreach (var item in This) {
			yield return transform (item);
		}
	}

	public static void Main ()
	{
		Type type = typeof (int);
		IEnumerable<string> properties = new[] { "x" };

		var checkIncludeExists = from n in properties
								 let p = type.GetProperty (n)
								 where p == null
								 select n;

		foreach (var item in checkIncludeExists) {
		}
	}
}
