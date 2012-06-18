// Compiler options: -r:test-846-lib.dll

class Test
{
	public static int Main ()
	{
		new B ().Foo ();
		return 0;
	}
}