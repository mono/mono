// Type inference when creating delegates

using System;

delegate int D (string s, int i);

delegate int E ();

class X
{
	public static T F<T> (string s, T t)
	{
		return t;
	}

	public static T G<T> ()
	{
		throw new ArgumentException ();
	}

	public static void Main ()
	{
		D d1 = new D (F<int>);
		D d2 = new D (F);

		E e1 = new E (G<int>);
	}
}
