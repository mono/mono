// Compiler options: -langversion:latest

using System;

ref struct ValueStringBuilder
{
	public override string ToString ()
	{
		return "aaa";
	}
}


class X
{
	public static int Main ()
	{
		var s = new ValueStringBuilder ();
		if (s.ToString () != "aaa")
			return 1;

		return 0;
	}
}