//
// Tests assignment of a captured variable to another
// Do not add anything else to this test.
//
delegate void S ();

class X {

	//
	// DO NOT ADD ANYTHING ELSE TO THIS TEST
	//
	public static int Main ()
	{
		int a = 2;
		int b = 1;
		S d = delegate {
			a = b;
		};

		return 0;
	}
}
