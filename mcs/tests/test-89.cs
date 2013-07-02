//
// This test is used to make sure that we correctly create value
// types in the presence of arrays.  Check bug 21801 for a history
// of the bug
//
using System;

struct X {
	int value;
	
	X (int a)
	{
		value = a;
	}

	static X F (int a)
	{
		return new X (a);
	}
	
	public static int Main ()
	{
		X [] x = { new X (40), F (10) };

		if (x [0].value != 40)
			return 1;

		if (x [1].value != 10)
			return 2;

		Console.WriteLine ("test ok");
		return 0;
	}
}
