// CS1931: A range variable `i' conflicts with a previous declaration of `i'
// Line: 14


using System;
using System.Linq;

class C
{
	public static void Main ()
	{
		int i = 9;
		var e = from v in "a"
			let i = 2
			select v;
	}
}
