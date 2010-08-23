// CS1744: Named argument `a' cannot be used for a parameter which has positional argument specified
// Line: 12

static class C
{
	public static void Test (int a, int b)
	{
	}
	
	public static void Main ()
	{
		Test (1, a : 2);
	}
}
