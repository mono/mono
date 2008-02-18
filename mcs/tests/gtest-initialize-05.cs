

struct Point
{
	public int X, Y;
}

class C
{
	static Point p;
	
	public static int Main ()
	{
		new Point {
			X = 0,
			Y = 0
		};
		
		var markerPosition = new Point {
			X = 2 * 3,
			Y = 9
		};
		
		if (markerPosition.X != 6)
			return 1;
		
		if (markerPosition.Y != 9)
			return 2;
		
		Point[] pa = new Point[] { new Point { X = 9 }, new Point { X = 8 } };
		
		if (pa [0].X != 9)
			return 3;
		
		if (pa [1].X != 8)
			return 3;

		p = new Point { Y = -1 };
		if (p.Y != -1)
			return 4;
		
		return 0;
	}
}
