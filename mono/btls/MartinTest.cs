using System;
using System.Runtime.InteropServices;

class X
{
	[DllImport ("mono-btls")]
	extern static void mono_btls_martin_test ();

	static void Main ()
	{
		// mono_btls_martin_test ();
		MartinTest.Hello ();
	}
}
