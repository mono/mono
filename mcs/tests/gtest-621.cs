using System;

class X
{
	static int Main ()
	{
		int? intArg = 1;
		long? longArg = 2;

		var g = intArg ?? longArg;
		Console.WriteLine (g);
		if (g != 1)
			return 1;

		intArg = null;
		g = intArg ?? longArg;
		Console.WriteLine (g);
		if (g != 2)
			return 2;

		longArg = null;
		g = intArg ?? longArg;
		Console.WriteLine (g);
		if (g != null)
			return 3;
			
		return 0;
	}

	const Action cf = null;
	void Foo (Action f)
	{
		var x = f ?? cf;
	}
}