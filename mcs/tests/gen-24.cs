class Foo<T>
{
	public Foo ()
	{ }

	public void Hello (T t)
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
	static void Test (Foo<int> foo)
	{
		foo.Hello (4);
	}

	static void Main ()
	{
		Foo<int> foo = new Foo<int> ();
		Test (foo);
	}
}
