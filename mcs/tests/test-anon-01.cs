delegate void S ();

class X {

	//
	// DO NOT ADD ANYTHING ELSE TO THIS TEST
	//
	static int Main ()
	{
		int a;

		S b = delegate {
			a = 2;
		};

		return 0;
	}
}
