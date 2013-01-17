using System;

class C
{
	public static int Main ()
	{
		return Bar (null) ? 1 : 0;
	}

	static bool Bar (object t)
	{
		return Bar is object;
	}
}

