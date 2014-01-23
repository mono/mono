using System;

public delegate void Foo ();
public delegate void Bar (int x);

class X
{
	public X (Foo foo)
	{ }

	public X (Bar bar)
	{ }

	static void Test ()
	{ }

	public static void Main ()
	{
		X x = new X (Test);
	}
}
