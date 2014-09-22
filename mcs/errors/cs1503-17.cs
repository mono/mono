// CS1501: Argument `#1' cannot convert `ref string' expression to type `ref int'
// Line: 8

class C
{
	public static void Main ()
	{
		Foo (ref var x = "");
	}

	static void Foo (ref int i)
	{
	}
}