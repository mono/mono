using System;
using System.Collections;

class X {
	static int start, end;
	static int i;

	static IEnumerator GetRange ()
	{
		yield 1;
		for (i = start; i < end; i++)
			yield i;
		yield 100;
	}

	static void Main ()
	{
		start = 10;
		end = 30;

		IEnumerator e = GetRange ();
		while (e.MoveNext ()){
			Console.WriteLine ("Value=" + e.Current);
		}
	}
}
