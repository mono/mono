using System;

public static class Test
{
	public static int Main ()
	{
		string s;
		s = $"T {  "v"    }";
		if (s != "T v")
			return 1;

		s = $"T {  "v" + "2"  }";
		if (s != "T v2")
			return 2;

		return 0;
	}
}