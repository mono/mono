//
// Capturing test.
//
using System;

delegate void S ();

class X {
	public static void Main ()
	{
		int a = 1;
		S b = delegate {
			float f = 1;
			Console.WriteLine (a);
			if (f == 2)
				return;
		};
		b ();
		Console.WriteLine ("Back, got " + a);
	}
}
