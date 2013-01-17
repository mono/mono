using System;

public delegate void Simple ();

public delegate Simple Foo ();

class X
{
	public void Hello<U> (U u)
	{ }

	public void Test<T> (T t)
	{
		T u = t;
		Hello (u);
		Foo foo = delegate {
			T v = u;
			Hello (u);
			return delegate {
				Hello (u);
				Hello (v);
			};
		};
		Simple simple = foo ();
		simple ();
		Hello (u);
	}

	public static void Main ()
	{
		X x = new X ();
		x.Test (3);
	}
}
