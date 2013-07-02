//
// Nested anonymous methods tests and capturing of different variables.
//
using System;

delegate void D ();

class X {
	static D GlobalStoreDelegate;
	
	public static void Main ()
	{
		D d = MainHost ();

		d ();
		GlobalStoreDelegate ();
		GlobalStoreDelegate ();
	}

	static D MainHost ()
	{
		int toplevel_local = 0;
		
		D d = delegate () {
			int anonymous_local = 1;
			
			GlobalStoreDelegate = delegate {
				Console.WriteLine ("var1: {0} var2: {1}", toplevel_local, anonymous_local);
				anonymous_local = anonymous_local + 1;
			};

			toplevel_local = toplevel_local + 1;
		};

		return d;
	}
}
