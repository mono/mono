// CS0841: A local variable `x' cannot be used before it is declared
// Line: 8
// Compiler options: -langversion:experimental

class X
{
	public static void Main ()
	{
		Foo (x, out var x);
	}

	static void Foo (int arg, out int value)
	{
		value = 3;
	}
}