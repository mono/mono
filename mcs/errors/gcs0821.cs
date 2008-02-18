// CS0821: A fixed statement cannot use an implicitly typed local variable
// Line: 9
// Compiler options: -unsafe

public class Point
{
	public int X;
	public int Y;
}

public class Test
{
	unsafe static void Main ()
	{
		Point p = new Point ();
		p.X = 42;
		p.Y = 16;
		
		fixed (var x = &p.X)
		{
		}
	}
}

