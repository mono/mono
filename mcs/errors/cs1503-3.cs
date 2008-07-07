// CS1503: Argument `#1' cannot convert `int[]' expression to type `int'
// Line: 12

class C
{
	static void Foo (params int[] i)
	{
	}
	
	public static void Main ()
	{
		Foo (new int[1], 1);
	}
}
