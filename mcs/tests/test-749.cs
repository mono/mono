// Compiler options: -r:test-749-lib.dll

using System;

class M
{
	public static void Main ()
	{
		B b = new B ();
		b.Prop = 2;
	}
}
