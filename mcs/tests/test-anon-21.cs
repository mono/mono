//
// Nested anonymous methods and capturing of variables test
//
using System;

delegate void D ();

class X {

	public static int Main ()
	{
		X x = new X();
		x.M ();
		e ();
                Console.WriteLine ("J should be 101= {0}", j);
		if (j != 101)
			return 3;
		Console.WriteLine ("OK");
		return 0;
	}

	static int j = 0;
	static D e;
	
	void M ()
	{
		int l = 100;

		D d = delegate {
			int b;
			b = 1;
			Console.WriteLine ("Inside d");
			e = delegate {
					Console.WriteLine ("Inside e");
					j = l + b;
					Console.WriteLine ("j={0} l={1} b={2}", j, l, b);
			};
		};
		Console.WriteLine ("Calling d");
		d ();
	}
	
}
