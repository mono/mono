// Compiler options: -r:test-859-lib.dll

using System;

class X
{
	void Test_PropertyOptionalParameters (C c)
	{
		// need to just run verifier on the method
		if (c == null)
			return;

		Console.WriteLine (c.Value);
		c.Value2 = 1;
	}

	static int Main ()
	{
		var x = new X ();
		x.Test_PropertyOptionalParameters (null);

		return 0;
	}
}
