using System;

public delegate void Foo ();

public class World<T>
{
	public void Hello<U> (U u)
	{ }

	public void Test (T t)
	{
		Hello (t);
		Foo foo = delegate {
			Hello (t);
		};
	}
}

class X
{
	static void Main ()
	{
		World<X> world = new World<X> ();
		world.Test (new X ());
	}
}
