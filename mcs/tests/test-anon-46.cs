using System;

public delegate void Foo ();

public class World
{
	public void Hello (long a)
	{ }

	public void Test (int t)
	{
		Hello (t);
		int u = 2 * t;
		Foo foo = delegate {
			Hello (u);
		};
		foo ();
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
