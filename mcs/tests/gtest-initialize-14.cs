using System;

struct S
{
	public int X, Y;
}

class X
{
	public static int Main ()
	{
		var rect = new S {
			X = 1,
			Y = 2,
		};

		if (rect.X != 1)
			return 1;

		if (rect.Y != 2)
			return 2;

		rect = new S {
			X = rect.X,
			Y = rect.Y,
		};

		if (rect.X != 1)
			return 3;

		if (rect.Y != 2)
			return 4;

		return 0;
	}
}