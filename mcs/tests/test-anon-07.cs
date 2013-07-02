//
// Tests havign more than one anonymous method that captures the same variable
//
using System;

delegate void D ();

class X {
	public static int Main ()
	{
		int a = 0;
		D d1 = delegate {
			Console.WriteLine ("First");
			a = 1;
		};
		
		D d2 = delegate {
			Console.WriteLine ("Second");
			a = 2;
		};
		if (!t (a, 0))
			return 1;
		d1 ();
		if (!t (a, 1))
			return 2;
		d2 ();
		if (!t (a, 2))
			return 3;
		Console.WriteLine ("Test passes OK");
		return 0;
	}

	static bool t (int a, int b)
	{
		return a == b;
	}
}
