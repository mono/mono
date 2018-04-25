// Compiler options: -r:dlls/test-939-1/test-939-ref.dll -r:dlls/test-939-2/test-939-lib.dll -r:dlls/test-939-common.dll

class X
{
	public static void Main ()
	{
	}

	static void RealTest ()
	{
		A.Foo ();
		new B ();
	}
}