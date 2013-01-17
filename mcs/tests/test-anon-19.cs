//
// Tests capturing of double nested variables
//
using System;

delegate void S ();

class X {
	public static int Main ()
	{
		int i;
		int a = 0;
		S b = null;
		
		for (i = 0; i < 10; i++){
			int j = 0;
			b = delegate {
				Console.WriteLine ("i={0} j={1}", i, j);
				i = i + 1;
				j = j + 1;
				a = j;
			};
		}
		b ();
		Console.WriteLine ("i = {0}", i);
		if (!t (i, 11))
			return 1;
		b ();
		if (!t (i, 12))
			return 2;
		Console.WriteLine ("i = {0}", i);
		Console.WriteLine ("a = {0}", a);
		if (!t (a, 2))
			return 3;
		
		return 0;
	}

	static bool t (int a, int b)
	{
		return a == b;
	}
}
