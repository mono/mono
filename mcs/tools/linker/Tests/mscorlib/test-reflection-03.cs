using System;
using System.Reflection;

class X
{
	public static int Main ()
	{
		var body = typeof (X).GetMethod ("Main").GetMethodBody ();
		Console.WriteLine (body.LocalVariables.Count);
		if (body.LocalVariables.Count == 0)
			return 1;
		return 0;
	}
}