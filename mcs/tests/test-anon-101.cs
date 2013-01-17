using System;

delegate void Foo ();

class X
{
	public void Hello<U> (U u)
	{ }

	public void Test<T> (T t)
	{
		T u = t;
		Hello (u);
		Foo foo = delegate {
			Hello (u);
		};
		foo ();
		Hello (u);
	}

	public static void Main ()
	{
		X x = new X ();
		x.Test (3);
	}
}
