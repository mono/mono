using System;

delegate void Simple ();

delegate Simple Foo ();

class X
{
	public static void Hello (long k)
	{ }

	public static void Test (int i)
	{
		Hello (3);
		Foo foo = delegate {
			Hello (4);
			return delegate {
				Hello (5);
			};
		};
		Simple simple = foo ();
		simple ();
	}

	public static void Main ()
	{
		X x = new X ();
		X.Test (3);
	}
}
