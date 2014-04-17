using System;
using System.Collections.Generic;

class X
{
	public X (int i = 1, params string[] preprocessorSymbols)
	{
	}

	public X (int i = 1, List<string> s = null)
	{
	}

	public static void Main ()
	{
		new X ();
	}
}
