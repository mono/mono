using System;

class X
{
	static int Main (string[] args)
	{
		int[] t = args.Length > 0 ? null : null;
		return t == null ? 0 : 1;
	}
}
