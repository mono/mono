// Compiler options: -r:gtest-470-lib.dll

class C
{
	public static void Main ()
	{
		var x = new B ();
		x.Foo<C> ();
	}
}
