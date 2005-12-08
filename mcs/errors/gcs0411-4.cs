// gcs0411.cs: The type arguments for method `T X.G<T>()' cannot be infered from the usage. Try specifying the type arguments explicitly
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
