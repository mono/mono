// CS1503: Argument `#1' cannot convert `bool' expression to type `int[]'
// Line: 13

public class X
{
	public static void Test (params int[] a)
	{
	}

	public static void Main()
	{
		int i;
		Test (true);
	}
}
