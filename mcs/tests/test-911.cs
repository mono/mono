// Compiler options: -r:test-911-lib.dll

class N
{
	public static void Foo ()
	{
	}
}

class X
{
	public static void Main ()
	{
		N.Foo ();
	}
}