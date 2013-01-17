using System;

delegate void Simple ();

delegate Simple Foo ();

class X
{
	public void Hello (long k)
	{ }

	public void Test (int i)
	{
		Hello (3);
		Foo foo = delegate {
			int a = i;
			Hello (4);
			return delegate {
				int b = a;
				Hello (5);
			};
		};
		Foo bar = delegate {
			int c = i;
			Hello (6);
			return delegate {
				int d = i;
				Hello (7);
			};
		};
		Simple simple = foo ();
		simple ();
	}

	public static void Main ()
	{
		X x = new X ();
		x.Test (3);
	}
}
