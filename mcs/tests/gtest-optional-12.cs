using System;

public class C
{
	static int M (string s = "s", int k = 0, params int[] args)
	{
		return args[1];
	}

	public static int Main ()
	{
		if (M (args: new int[] { 10, 20, 30 }) != 20)
			return 1;

		return 0;
	}
}
