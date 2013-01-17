using System;

public delegate void Foo ();

public class World
{
	public void Hello (long a)
	{ }

	public void Test (int t)
	{
		for (long l = 0; l < t; l++) {
			for (long m = 0; m < l; m++) {
				for (int u = 0; u < t; u++) {
					Foo foo = delegate {
						Hello (u);
						Hello (l);
						Hello (m);
					};
					foo ();
				}
			}
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
