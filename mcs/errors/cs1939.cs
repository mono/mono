// CS1930: A range variable `v' may not be passes as `ref' or `out' parameter
// Line: 19


using System;
using System.Linq;

class C
{
	static int Foo (ref int value)
	{
		return 1;
	}
	
	public static void Main ()
	{
		var e = from v in "a"
			let r = 1
			select Foo (ref v);
	}
}
