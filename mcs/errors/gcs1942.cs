// CS1942: Type inference failed to infer type argument for `select' clause. Try specifying the type argument explicitly
// Line: 13


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
