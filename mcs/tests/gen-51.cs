//
// Accoring to the spec (26.7.1), this should compile since
// there's an implicit reference conversion from B to A.
//
// However, csc reports a CS0309.
//

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
	static void Main ()
	{
		Foo<B> foo = new Foo<B> ();
		foo.Test (new B ());
	}
}
