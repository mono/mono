// CS8047: Declaration expression cannot be used in this context
// Line: 8

public class C
{
	public static void Main ()
	{
		dynamic target = 3;
		var x = new Test (target, out var y);
	}
}

class Test
{
	public Test (int x, out int y)
	{
		y = 0;
	}
}