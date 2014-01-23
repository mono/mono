using System;

delegate void Simple ();

delegate Simple Foo ();

class X
{
	public void Hello (long i, long j)
	{ }

	public void Test (int i)
	{
		long j = 1 << i;
		Hello (i, j);
		Foo foo = delegate {
			long k = 5 * j;
			Hello (j, k);
			return delegate {
				Hello (j, k);
			};
		};
	}

	public static void Main ()
	{
		X x = new X ();
		x.Test (3);
	}
}
