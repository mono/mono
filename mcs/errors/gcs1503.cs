// CS1503:  Argument 1: Cannot convert type `bool' to `int[]'
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
