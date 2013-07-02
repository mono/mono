using System;

interface IFoo
{
	MyList<U> Map<U> ();
}

class MyList<T>
{
	public void Hello (T t)
	{
		Console.WriteLine (t);
	}
}

class Foo : IFoo
{
	public MyList<T> Map<T> ()
	{
		return new MyList<T> ();
	}
}

class X
{
	public static void Main ()
	{
		Foo foo = new Foo ();
		MyList<int> list = foo.Map<int> ();
		list.Hello (9);
	}
}
