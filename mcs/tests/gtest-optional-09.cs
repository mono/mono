using System;

public class Program
{
	static int Test_1 (int i, sbyte s = 1)
	{
		return 1;
	}

	static int Test_1<T> (T s)
	{
		return 0;
	}

	static int Test_1 (int i, long s = 1)
	{
		return 2;
	}

	static int Test_2 (short s)
	{
		return 1;
	}

	static int Test_2 (int i, sbyte s = 1)
	{
		return 0;
	}

	static int Test_3 (string s)
	{
		return 0;
	}

	static int Test_3 (string s, sbyte s2 = 1)
	{
		return 1;
	}

	static int Test_4 (object o = null)
	{
		return 1;
	}

	static int Test_4 (params object[] a)
	{
		return 0;
	}

	static int Test_5 ()
	{
		return 0;
	}

	static int Test_5 (int i = 1, params object[] a)
	{
		return 1;
	}

	static int Test_6 (params object[] o)
	{
		return 1;
	}

	static int Test_6 (int i = 1, params object[] a)
	{
		return 0;
	}

	static int Test_7 (bool b, params object[] o)
	{
		return 1;
	}

	static int Test_7 (bool b, int i = 1, params object[] a)
	{
		return 0;
	}

	static int Test_8 (Type t, bool b = false, int x = 0)
	{
		return 0;
	}

	static int Test_8 (Type t, params int[] x)
	{
		return 1;
	}

	public static int Main ()
	{
		if (Test_1 (5) != 0)
			return 1;

		if (Test_2 (6) != 0)
			return 2;

		if (Test_3 ("") != 0)
			return 3;

		if (Test_4 (null) != 0)
			return 4;

		if (Test_5 () != 0)
			return 5;

		if (Test_6 () != 1)
			return 6;

		if (Test_7 (false) != 1)
			return 7;

		if (Test_8 (typeof (bool)) != 0)
			return 8;

		Console.WriteLine ("ok");
		return 0;
	}
}
