// Compiler options: -langversion:default

using System;
using System.Collections;

class X {
	static int start, end;
	static int i;

	static IEnumerator GetRange ()
	{
		yield return 1;
		for (i = start; i < end; i++)
			yield return i;
		yield return 100;
	}

	public static int Main ()
	{
		start = 10;
		end = 30;

		int total = 0;
		
		IEnumerator e = GetRange ();
		while (e.MoveNext ()){
			Console.WriteLine ("Value=" + e.Current);
			total += (int) e.Current;
		}

		if (total != 491)
			return 1;
		return 0;
	}
}
