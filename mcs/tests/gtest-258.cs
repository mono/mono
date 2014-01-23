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
	public static void Main ()
	{
		Foo<A> foo = new Foo<A> ();
	}
}
