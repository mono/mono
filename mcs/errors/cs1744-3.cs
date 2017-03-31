// CS1744: Named argument `p1' cannot be used for a parameter which has positional argument specified
// Line: 8

internal class Program
{
	public static void Main ()
	{
		Method (1, 2, p1: 3);
	}
	
	static void Method (int p1, int paramNamed, int p2)
	{
	}
	
	static void Method (int p1, int p2, object paramNamed)
	{
	}
}
