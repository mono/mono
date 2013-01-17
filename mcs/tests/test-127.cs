//
// It is possible to invoke Enum methods on an enum type.
//
using System;

enum Test {
	A,
	B,
	C
}

class X {

	public static int Main ()
	{
		Test test = Test.A;

		if (!Test.IsDefined (typeof (Test), test))
			return 1;

		return 0;
	}
}
