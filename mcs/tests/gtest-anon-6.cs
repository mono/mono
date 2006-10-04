using System;

delegate void Foo<R,S> (R r, S s);

class X
{
	public void Hello<U,V> (U u, V v)
	{ }

	public void Test<A,B,C> (A a, B b, C c)
	{
		Hello (a, b);
		C d = c;
		Foo<A,int> foo = delegate (A i, int j) {
			Hello (i, c);
			Hello (i, j);
		};
	}

	static void Main ()
	{
		X x = new X ();
		x.Test (3, Math.PI, 1 << 8);
	}
}
