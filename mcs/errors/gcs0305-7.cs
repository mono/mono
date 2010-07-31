// CS0305: Using the generic method `X.G<T>()' requires `1' type argument(s)
// Line: 17

using System;

delegate int E ();

class X
{
	public static T G<T> ()
	{
		throw new ArgumentException ();
	}

	static void Main ()
	{
		E e2 = new E (G);
	}
}
