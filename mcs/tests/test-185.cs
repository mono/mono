using System;

class X
{
	public static int Test (int x)
	{
		for (;;) {
			if (x != 1)
				x--;
			else
				break;
			return 5;
		}
		return 0;
	}

	static int Main ()
	{
		if (Test (1) != 0)
			return 1;

		if (Test (2) != 5)
			return 2;

		return 0;
	}
}
