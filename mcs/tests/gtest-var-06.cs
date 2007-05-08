// Compiler options: -langversion:linq -unsafe
// Tests variable type inference with the var keyword when using the "fixed" statement
using System;

public class Point
{
	public int X;
	public int Y;
}

public class Test
{
	unsafe static int Main ()
	{
		Point p = new Point ();
		p.X = 42;
		p.Y = 16;
		
		fixed (var x = &p.X)
			if (*x != 42)
				return 1;
		
		return 0;
	}
}
