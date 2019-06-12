using System;

class Program
{
	public static int Main ()
	{
		int x = 1;
		int y = 2;

		(x, y) = (y, x);

		if (x != 2)
			return 1;

		if (y != 1)
			return 2;

		return 0;
	}
}
