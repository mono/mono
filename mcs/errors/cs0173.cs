// CS0173: Type of conditional expression cannot be determined because there is no implicit conversion between `null' and `null'
// Line: 10

using System;

class X
{
	static int Main (string[] args)
	{
		int[] t = args.Length > 0 ? null : null;
		return t == null ? 0 : 1;
	}
}
