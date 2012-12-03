//
// fixed
//
class X {

	static void A (ref int a)
	{
		a++;
	}

	// Int32&
	static void B (ref int a)
	{
		// Int32&&
		A (ref a);
	}

	public static int Main ()
	{
		int a = 10;

		B (ref a);

		if (a == 11)
			return 0;
		else
			return 1;
	}
}
