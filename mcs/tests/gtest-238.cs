// Compiler options: /r:gtest-238-lib.dll
// Dependencies: gtest-238-lib.cs
class X
{
	public static int Main ()
	{
		Foo<long> foo = new Foo<long> ();
		if (foo.Test (3) != 1)
			return 1;
		if (foo.Test (5L) != 2)
			return 2;
		return 0;
	}
}
