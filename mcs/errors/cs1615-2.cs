// CS1615: Argument `2' should not be passed with the `out' keyword
// Line: 13

public class X
{
	public static void Test (params int[] a)
	{
	}

	public static void Main()
	{
		int i;
		Test (1, out i);
	}
}
