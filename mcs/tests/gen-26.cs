class Foo<T>
{
	public T Hello;

	public Foo ()
	{ }
}

class X
{
	static void Main ()
	{
		Foo<int> foo = new Foo<int> ();
		foo.Hello = 9;
	}
}
