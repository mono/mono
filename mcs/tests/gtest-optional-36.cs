using System;

public class Program
{
	static int Arg (uint a, long b)
	{
		return 2;
	}

	static int Arg (int a, ulong b, int c = 9)
	{
		return 3;
	}

	static int Arg_2 (uint a, long b, params int[] arg)
	{
		return 2;
	}

	static int Arg_2 (int a, ulong b, int c = 0)
	{
		return 3;
	}

	static int Arg_3 (int a, long b, params int[] arg)
	{
		return 2;
	}

	static int Arg_3 (uint a, ulong b, int c = 0, int d = 1, params int[] oo)
	{
		return 3;
	}	

	public static int Main ()
	{
		if (Arg (0, 0) != 2)
			return 1;

		if (Arg (0, 0, 0) != 3)
			return 2;

		if (Arg_2 (0, 0) != 3)
			return 3;

		if (Arg_2 (0, 0, 0, 0) != 2)
			return 4;

		if (Arg_3 (0, 0) != 2)
			return 5;

		if (Arg_3 (0, 0, 0) != 2)
			return 6;

		if (Arg_3 (0, 0, 0, 0) != 2)
			return 7;

		if (Arg_3 (0, 0, 0, 0, 0) != 2)
			return 8;

		if (Arg_3 (0, 0, 0, 0, 0) != 2)
			return 9;

		return 0;
	}
}
