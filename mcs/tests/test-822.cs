// Compiler options: -r:test-822-lib.dll

using System;

class Test
{
	public static int Main ()
	{
		B b = new B ();
		b.Prop = 1;
		int a = b.Prop;
		return 0;
	}
}
