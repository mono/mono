// Compiler options: -langversion:default

//
// Use

using System;
using System.Collections;

class X {
	static IEnumerable GetIt (int [] args)
	{
		foreach (int a in args)
			yield return a;
	}

	static IEnumerable GetMulti (int [,] args)
	{
		foreach (int a in args)
			yield return a;
	}
	
	public static int Main ()
	{
		int total = 0;
		foreach (int i in GetIt (new int [] { 1, 2, 3})){
			Console.WriteLine ("Got: " + i);
			total += i;
		}

		if (total != 6)
			return 1;

		total = 0;
		foreach (int i in GetMulti (new int [,] { { 10, 20 }, { 30, 40}})){
			Console.WriteLine ("Got: " + i);
			total += i;
		}
		if (total != 100)
			return 2;
		
		return 0;
	}
}
