// CS1943: An expression type is incorrect in a subsequent `from' clause in a query expression with source type `string'
// Line: 11

using System.Linq;

class Test
{
	static void Main ()
	{
		var e = from a in "abcd"
				from b in new Test ()
				select b;
	}
}
