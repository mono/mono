using System;
using System.Collections.Generic;

class Test
{
	static IEnumerable<int> FromTo (int from, int to)
	{
		while (from <= to) yield return from++;
	}

	public static int Main ()
	{
		IEnumerable<int> e = FromTo (1, 10);

		int i = 0;
		foreach (int x in e) {
			foreach (int y in e) {
				i += y;
				Console.Write ("{0,3} ", x * y);
			}
			Console.WriteLine ();
		}
		
		Console.WriteLine (i);
		if (i != 550)
			return i;
		
		return 0;
	}
}
