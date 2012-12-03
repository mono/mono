class X {

	double d = 0;

	X ()
	{
	}

	public static int Main ()
	{
		X x = new X ();

		if (x.d != 0)
			return 1;

		return 0;
	}
}
