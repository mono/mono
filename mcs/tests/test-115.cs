//
// This is a compile test, submitted by Joe.   We really need
// a more thorough set of tests for the user defined explicit
// conversions
//
class A {
	public static explicit operator X (A foo)
	{
		X myX = new X();

		return myX;
	}
}

class X {
}

class Y : X {
}

class blah {
	public static int Main ()
	{
		A testA = new A();
		
		X testX = (X) testA;

		Y testY = (Y) testA;

		return 0;
	}
}
		
