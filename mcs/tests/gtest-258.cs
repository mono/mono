using System;

public class A
{
	public A ()
	{ }
}

public class B
{ }

class Foo<T>
	where T : new ()
{ }

class X
{
	static void Main ()
	{
		Foo<A> foo = new Foo<A> ();
	}
}
