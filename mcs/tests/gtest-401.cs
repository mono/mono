using System;

class ParserTest
{
	public static void Main ()
	{
		int [] ivals = { 2, 5 };
		foreach (int x in ivals)
			foreach (int y in ivals)
				Console.WriteLine ("{0} {1} {2} {3} {4} {5}", x, y, x + y, x - y, x < y, x >= y);
	}
}

