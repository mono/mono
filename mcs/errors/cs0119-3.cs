// CS0119: Expression denotes a `variable', where a `method group' was expected
// Line: 11

using System;

class X
{
	static void Main ()
	{
		Delegate d = null;
		Console.WriteLine (d (null, null));
	}
}
