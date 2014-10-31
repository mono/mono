// CS0815: An implicitly typed local variable declaration cannot be initialized with `void'
// Line: 8

class X
{
	public static void Main ()
	{
		Foo (out var x = Main ());
	}

	static void Foo (out int i)
	{
		i = 0;
	}
}