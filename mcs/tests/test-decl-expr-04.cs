public class C
{
	public static void Main ()
	{
		Test2 (Test (out var x1), x1);
	}

	static int Test (out int x)
	{
		x = 1;
		return 2;
	}

	static int Test2 (int x, int y)
	{
		return 2;
	}
}