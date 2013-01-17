using System;
using System.Collections.Generic;

class C
{
	static IEnumerable<int> Test ()
	{
		List<Func<int>> lambdas = new List<Func<int>> ();
		for (int i = 0; i < 4; ++i) {
			int h = i;
			lambdas.Add (() => h);
			yield return 2;
		}

		for (int i = 0; i < 4; ++i) {
			yield return lambdas[i] ();
		}
	}

	static IEnumerable<int> Test_2 ()
	{
		List<Func<int>> lambdas = new List<Func<int>> ();
		for (int i = 0; i < 4; ++i) {
			int h = i;
			lambdas.Add (() => h);
		}

		for (int i = 0; i < 4; ++i) {
			yield return lambdas[i] ();
		}
	}

	public static int Main ()
	{
		int t = 0;
		foreach (var a in Test ()) {
			t += a;
		}

		Console.WriteLine (t);
		if (t != 14)
			return 1;

		t = 0;
		foreach (var a in Test_2 ()) {
			t += a;
		}

		Console.WriteLine (t);
		if (t != 6)
			return 2;

		return 0;
	}
}