// CS0128: A local variable named `x' is already defined in this scope
// Line: 9

class X
{
	public static void Main ()
	{
		Foo (out int x);
		Foo (out int x);
	}

	static void Foo (out int arg)
	{
		arg = 2;
	}
}