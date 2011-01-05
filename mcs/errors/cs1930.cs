// CS1930: A range variable `v' has already been declared in this scope
// Line: 13


using System;
using System.Linq;

class C
{
	public static void Main ()
	{
		var e = from v in "a"
			let v = 1
			select v;
	}
}
