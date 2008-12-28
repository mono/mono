// CS1942: An expression type in `select' clause is incorrect. Type inference failed in the call to `Select'
// Line: 12

using System;
using System.Linq;

class C
{
	public static void Main ()
	{
		var e = from values in "abcd"
			select null;
	}
}
