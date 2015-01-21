// CS0165: Use of unassigned local variable `x'
// Line: 21

using System;

class X
{
	static bool Foo (out int x)
	{
		x = 5;
		return false;
	}

	public static int Main ()
	{
		int x;
		try {
			throw new ApplicationException ();
		} catch when (Foo (out x)) {
			return 1;
		} catch when (x > 0) {
			return 0;
		}
	}
}