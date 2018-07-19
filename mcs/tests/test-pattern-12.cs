using System;
using System.Collections.Generic;

class X
{
	public static int Main ()
	{
		foreach (var x in Test1 ("2"))
		{
			Console.WriteLine (x);
			return 1;
		}

		foreach (var x in Test2 (2))
		{
			Console.WriteLine (x);
			return 2;
		}

		return 0;
	}

	public static IEnumerable<object> Test1 (object expr)
	{
		if (expr is short list)
		{
			yield return "list.Length";
		}
	}

	public static IEnumerable<object> Test2 (object expr)
	{
		if (expr is string list)
		{
			yield return "list.Length";
		}
	}
}