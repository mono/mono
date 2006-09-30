using System;
using System.Collections.Generic;

public class A {
}

public class B : A {
}

public class Tests
{
	public static void Main ()
	{
		IList<A> a = new B [0];

		Console.WriteLine (typeof (IList<A>).IsAssignableFrom (typeof (B[])));
	}
}

