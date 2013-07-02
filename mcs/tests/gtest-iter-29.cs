using System;
using System.Linq;
using System.Collections.Generic;

public class Test
{
	public static int Main ()
	{
		var x = Test2 ();
		if (x.Count () != 0)
			return 1;

		Console.WriteLine ("ok");
		return 0;
	}

	public static IEnumerable<int> Test2 ()
	{
		while (false)
			yield return 5;
	}
}