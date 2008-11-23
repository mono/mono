using System;
using System.Collections.Generic;

class Test
{
	static IEnumerable<T> Create<T> (T [,] self)
	{
		for (int i = 0; i < self.Length; ++i)
			yield return self [0, i];
	}

	public static int Main ()
	{
		int [,] s = new int [,] { { 1, 2, 3 } };
		foreach (int i in Create (s))
			Console.WriteLine (i);

		return 0;
	}
}
