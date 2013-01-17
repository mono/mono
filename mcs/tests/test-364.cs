//
// Test for bug: 69614
//
// Basically, this tests that we can capture parameters and use them outside the delegate
//
using System;

class X {

	delegate int Foo ();
	
	public static int Main ()
	{
		int x = t1 (1);
		if (x != 1)
			return 1;
		x = t2 (2);
		if (x != 3)
			return 2;
		return 0;
	}

	static int t1 (int p)
	{
		Foo f = delegate {
			return p;
		};
		return f ();
	}

	static int t2 (int p)
	{
		p++;
		Foo f = delegate {
			return p;
		};
		return f ();
	}

	//
	// This is just here to check that it compiles, but the logic is the
	// same as the ones before
	
	public static void Main2 (string[] argv)
	{
		Console.WriteLine ("Test");

		Delegable db = new Delegable ();
		if (argv.Length > 1) {
			db.MyDelegate += delegate (object o, EventArgs args) {
				Console.WriteLine ("{0}", argv);
				Console.WriteLine ("{0}", db);
			};
		}
	}	
}

class Delegable {
	public event EventHandler MyDelegate;
}


