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

public class B : A
{
}

class X
{
	public static void Main ()
	{
		Foo<B> foo = new Foo<B> ();
		foo.Test (new B ());
	}
}
