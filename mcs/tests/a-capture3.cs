//
// Simple variable capturing
//
delegate void S ();
using System;

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
