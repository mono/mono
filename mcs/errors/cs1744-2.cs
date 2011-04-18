// CS1744: Named argument `a' cannot be used for a parameter which has positional argument specified
// Line: 13

static class C
{
	public static int Test (this int a, int b)
	{
		return a * 3 + b;
	}
	
	public static void Main ()
	{
		1.Test (a : 2);
	}
}
