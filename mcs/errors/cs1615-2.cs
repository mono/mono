// CS1615: Argument `#2' does not require `out' modifier. Consider removing `out' modifier
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
