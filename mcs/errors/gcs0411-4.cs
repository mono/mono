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
