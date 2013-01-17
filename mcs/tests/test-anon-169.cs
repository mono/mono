using System;
using System.Collections.Generic;

static class Test
{
	public static int Main ()
	{
		var fs = new List<Func<int>> ();

		foreach (int i in new List<int> () { 1, 2, 3 }) {
			fs.Add (() => i);
		}

		int total = 0;
		foreach (var i in fs) {
			total += i ();
			Console.WriteLine (i ());
		}

		if (total != 6)
			return 1;

		var fs2 = new List<Func<char>> ();
		total = 0;
		foreach (var i in fs2) {
			total += i ();
			Console.WriteLine (i ());
		}

		foreach (var i in "abcd") {
			fs2.Add (() => i);
		}

		string concat = "";
		foreach (var i in fs2) {
			concat += i ();
			Console.WriteLine (i ());
		}

		if (concat != "abcd")
			return 2;

		return 0;
	}
}
