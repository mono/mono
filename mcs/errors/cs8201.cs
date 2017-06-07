// CS8201: Out variable and pattern variable declarations are not allowed within a query clause
// Line: 11

using System.Linq;

class Program
{
	public static void Main ()
	{
		var a = "abcdef";
		var res = from x in a from y in M (a, out var z) select x;
	}

	public static T M<T>(T x, out T z) => z = x;
}