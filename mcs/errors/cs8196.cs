// CS8196: Reference to an implicitly typed out variable `x1' is not permitted in the same argument list
// Line: 8

public class C
{
	public static void Main ()
	{
		Test (out var x1, out x1);
	}

	static void Test (out int x, out int x2)
	{
		x = 1;
		x2 = 2;
	}
}