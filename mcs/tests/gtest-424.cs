using System;
using System.Collections.Generic;

public class A { }

public class B : A { }

public class Test
{
	public static void Block (params A[] expressions)
	{
		throw new ApplicationException ();
	}

	public static void Block (IEnumerable<B> variables, params A[] expressions)
	{
	}

	public static int Main ()
	{
		A e = new A ();
		Block (new B[] { }, e);
		return 0;
	}
}
