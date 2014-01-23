using System;

public delegate void Foo ();

public class World
{
	public void Hello (long a)
	{ }

	public void Test (int t)
	{
		Hello (t);
		long j = 1 << t;
		for (int u = 0; u < j; u++) {
			long test = t;

			Foo foo = delegate {
				long v = u * t * test;
				Hello (v);
			};
		}
	}
}

class X
{
	public static void Main ()
	{
		World world = new World ();
		world.Test (5);
	}
}
