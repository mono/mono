using System;

delegate void Simple ();

delegate Simple Foo ();

class X
{
	public void Hello (long k)
	{ }

	public void Test (int i)
	{
		long j = 1 << i;
		Hello (j);
		Foo foo = delegate {
			long k = j;
			Hello (j);
			return delegate {
				long l = k;
				Hello (j);
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
