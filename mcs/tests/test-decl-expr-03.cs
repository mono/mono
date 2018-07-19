using System;
using System.Linq;

public class C
{
	public static void Main ()
	{
		var a = "abcdef";

		var t1 = from x in Foo (a, out var q1) select x;
		var t2 = from x in a join y in Foo (a, out var q2) on x equals y select x;
	}

	public static T Foo<T> (T x, out T z) => z = x;
}