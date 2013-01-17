//
// Tests capturing of double nested variables
//
using System;
delegate void S ();

class X {
	public static int Main ()
	{
		int i;
		S b = null;
		
		for (i = 0; i < 10; i++){
			int j = 0;
			b = delegate {
				Console.WriteLine ("i={0} j={1}", i, j);
				i = i + 1;
				j = j + 1;
			};
		}
		Console.WriteLine ("i = {0}", i);
		b ();
		Console.WriteLine ("i = {0}", i);
		if (!t (i, 11))
			return 1;
		b ();
		if (!t (i, 12))
			return 2;
		Console.WriteLine ("i = {0}", i);
		Console.WriteLine ("Test is OK");
		return 0;
	}

	static bool t (int a, int b)
	{
		return a == b;
	}
}
