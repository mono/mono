//
// Use

using System;
using System.Collections;

class X {
	static IEnumerable GetIt (int [] args)
	{
		foreach (int a in args)
			yield a;
	}
	
	static int Main ()
	{
		int total = 0;
		foreach (int i in GetIt (new int [] { 1, 2, 3})){
			Console.WriteLine ("Got: " + i);
			total += i;
		}
		if (total != 6)
			return 1;
		return 0;
	}
}
