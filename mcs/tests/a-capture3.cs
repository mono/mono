//
// Simple variable capturing
//
using System;

delegate void S ();

class X {
	static void Main ()
	{
		int a = 1;
		S b = delegate {
			a = 2;
		};
		b ();
		Console.WriteLine ("Back, got " + a);
	}
}
