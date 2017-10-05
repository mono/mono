class X
{
	int field;

	public static int Main ()
	{
		var x = new X ();

		x.field = 5;
		if (!x.Test1 ())
			return 1;

		x.Test2 ();

		if (x.Test3 ()++ != 6)
			return 2;

		if (x.field != 7)
			return 3;

		return 0;
	}

	bool Test1 ()
	{
		ref var x = ref field;
		int v = x;
		++x;

		return x == 6;
	}

	void Test2 ()
	{
		ref int x = ref field;
		x.ToString ();
	}

	ref int Test3 ()
	{
		ref int l = ref field;
		ref int v = ref l;
		return ref l;
	}
}