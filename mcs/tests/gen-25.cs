class Foo
{
	public Foo ()
	{ }

	public void Hello<T> (T t)
	{
		Whatever (t);
	}

	public void Whatever (object o)
	{
		System.Console.WriteLine (o.GetType ());
	}
}

class X
{
	static void Test (Foo foo)
	{
		foo.Hello<int> (531);
	}

	static void Main ()
	{
		Foo foo = new Foo ();
		Test (foo);
	}
}
