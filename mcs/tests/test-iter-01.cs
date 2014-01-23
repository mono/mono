// Compiler options: -langversion:default

using System;
using System.Collections;

class X {
	static IEnumerator GetIt ()
	{
		yield return 1;
		yield return 2;
		yield return 3;
	}
	
	static IEnumerable GetIt2 ()
	{
		yield return 1;
		yield return 2;
		yield return 3;
	}

	public static int Main ()
	{
		IEnumerator e = GetIt ();
		int total = 0;
		
		while (e.MoveNext ()){
			Console.WriteLine ("Value=" + e.Current);
			total += (int) e.Current;
		}

		if (total != 6)
			return 1;

		total = 0;
		foreach (int i in GetIt2 ()){
			Console.WriteLine ("Value=" + i);
			total += i;
		}
		if (total != 6)
			return 2;
		
		return 0;
	}
}
