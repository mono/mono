// CS1930: A range variable `i' has already been declared in this scope
// Line: 14


using System;
using System.Linq;

class C
{
	public static void Main ()
	{
		var e = from v in "a"
			let i = 1
			let i = 2
			select v;
	}
}
