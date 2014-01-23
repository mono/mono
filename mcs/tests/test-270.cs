using System;

class X
{
	static int Test (string format, params object[] args)
	{
		return 1;
	}

	static int Test (string format, __arglist)
	{
		return 2;
	}

	public static int Main ()
	{
		if (Test ("Hello", 1, 2, "World") != 1)
			return 1;
		if (Test ("Hello", __arglist ("Boston")) != 2)
			return 2;
		return 0;
	}
}
