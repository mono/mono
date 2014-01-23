using System;

class X
{
	static int Test (int? a)
	{
		switch (a) {
		case 0:
			return 0;
		case 1:
			return 1;

		default:
			return -1;
		}
	}

	public static int Main ()
	{
		if (Test (null) != -1)
			return 1;
		if (Test (0) != 0)
			return 2;

		return 0;
	}
}
