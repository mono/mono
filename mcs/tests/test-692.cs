using System;

class Test
{
	static DateTime a, b, c;
	DateTime ia, ib, ic;

	static void test1 (out DateTime x)
	{
		x = new DateTime (85);
	}

	static void test2 (ref DateTime x)
	{
		x = new DateTime (999);
	}

	public static int Main ()
	{
		c = b = a = new DateTime (633596616517216000);
		if (c != a)
			return 1;

		if (b != c)
			return 2;

		DateTime ia, ib, ic;
		ic = ib = ia = new DateTime (633596616517216000);

		if (ic != ia)
			return 10;

		if (ib != ic)
			return 12;

		test1 (out ia);
		test2 (ref ia);

		return 0;
	}
}
