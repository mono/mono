// Test for bug 76550 -- EmitAssign getting called
// on captured params.

class Z {
	public static void Main ()
	{
		TestPreinc (1);
		TestPostinc (1);
	}
	
	delegate void X ();

	static void TestPreinc (int i)
	{
		Assert (i, 1);
		X x = delegate {
			int z = ++i;
			Assert (z, 2);
			Assert (i, 2);
		};
		x ();
		Assert (i, 2);
	}

	static void TestPostinc (int i)
	{
		Assert (i, 1);
		X x = delegate {
			int z = i++;
			Assert (z, 1);
			Assert (i, 2);
		};
		x ();
		Assert (i, 2);
	}
	
	static void Assert (int a, int b)
	{
		if (a == b)
			return;

		throw new System.Exception ("Incorrect was: " + a + " should have been " + b + ".");
	}
}
