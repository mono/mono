using System;

class Foo<T>
{
	public T Test<U> (U u)
		where U : T
	{
		return u;
	}
}

class X
{
	public static void Main ()
	{
		Foo<X> foo = new Foo<X> ();

		Y y = new Y ();
		X x = foo.Test<Y> (y);
	}
}

class Y : X
{
}
