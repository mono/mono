// CS0311: The type `B' cannot be used as type parameter `T' in the generic type or method `Foo<T>'. There is no implicit reference conversion from `B' to `A'
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
