using System;

class X
{
	public static int Main ()
	{
		int x = 7;
		int y = 2;

		x = (y += 3) + 10;
		if (y != 5)
			return 1;
		if (x != 15)
			return 2;

		x += 9;
		if (x != 24)
			return 3;

		byte c = 3;
		byte d = 5;
		x = d ^= c;
		Console.WriteLine (x);

		return 0;
	}
}
