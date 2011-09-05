using System;

delegate void Foo<S> (S s);

class X
{
	public void Hello<U> (U u)
	{ }

	public void Test<T> (T t)
	{
		Hello (t);
		Foo<T> foo = delegate (T u) {
			Hello (u);
		};
		foo (t);
	}

	static void Main ()
	{
		X x = new X ();
		x.Test (3);
	}
}
