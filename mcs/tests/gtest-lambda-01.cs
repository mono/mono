
//
// Lambda expression test, basics.
//
using System;

delegate int IntFunc (int x);
delegate void VoidFunc (int x);

class X {

	static IntFunc func, increment;
	static VoidFunc nothing;
	
	public static int Main ()
	{
		int y = 0;
		int r;
		
		//
		// The following tests body-style lambda
		//
		increment = (int x) => { return x + 1; };
		r = increment (4);
		Console.WriteLine ("Should be 5={0}", r);
		if (r != 5)
			return 1;

		//
		// This tests the body of a lambda being an expression
		//
		func = (int x) => x + 1;
		r = func (10);
		Console.WriteLine ("Should be 11={0}", r);
		if (r != 11)
			return 2;
		
		//
		// The following tests that the body is a statement
		//
		nothing = (int x) => { y = x; };
		nothing (10);
		Console.WriteLine ("Should be 10={0}", y);
		if (y != 10)
			return 3;

		nothing = (int x) => { new X (x); };
		nothing (314);
		if (instantiated_value != 314)
			return 4;
		
		Console.WriteLine ("All tests pass");
		return 0;
	}

	static int instantiated_value;

	X (int v)
	{
		instantiated_value = v;
	}
}
