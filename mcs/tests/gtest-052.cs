// We create an instance of a type parameter which has the new() constraint.
using System;

public class Foo<T>
	where T : new ()
{
	public T Create ()
	{
		return new T ();
	}
}

class X
{
	public X ()
	{ }

	void Hello ()
	{
		Console.WriteLine ("Hello World");
	}

	public static void Main ()
	{
		Foo<X> foo = new Foo<X> ();
		foo.Create ().Hello ();
	}
}
