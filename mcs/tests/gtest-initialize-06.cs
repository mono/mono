

struct Point
{
	public int X, Y;
}

class C
{
	public static int Main ()
	{
		Point p;
		Foo (out p);
		
		if (p.X != 3)
			return 1;
		
		if (p.Y != 5)
			return 2;
		
		return 0;
	}
	
	static void Foo (out Point p)
	{
		p = new Point () { X = 3, Y = 5 };
	}
}
