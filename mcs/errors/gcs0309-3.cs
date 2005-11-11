// CS0309: The type `B' must be convertible to `A' in order to use it as parameter `T' in the generic type or method `Foo<T>'
// Line: 35
using System;

public class Foo<T>
	where T : A
{
	public void Test (T t)
	{
		Console.WriteLine (t);
		Console.WriteLine (t.GetType ());
		t.Hello ();
	}
}

public class A
{
	public void Hello ()
	{
		Console.WriteLine ("Hello World");
	}
}

public class B
{
	public static implicit operator A (B b)
	{
		return new A ();
	}
}

class X
{
	Foo<B> b;

	static void Main ()
	{
	}
}
