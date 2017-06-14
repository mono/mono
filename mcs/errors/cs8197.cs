// CS8197: Cannot infer the type of implicitly-typed out variable `y'
// Line: 9

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