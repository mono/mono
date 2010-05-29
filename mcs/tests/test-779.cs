using System;

public static class Test
{
	public static int Main ()
	{
		if (test1 (15, 15))
			return 1;

		return 0;
	}

	//Bug #610126
	static bool test1 (long a, long b)
	{
		if ((a & b) == 0L) return true;
		return false;
	}
}