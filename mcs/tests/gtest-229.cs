using System;
using System.Collections;
using System.Collections.Generic;

public class B : IComparable<B> {
	public int CompareTo (B b)
	{
		return 0;
	}
}

public class Tester
{
	public static int Main ()
	{
		B b = new B ();

		// This should be false
		if (b is IComparable<object>)
			return 1;
		return 0;
	}
}
