// Test access to class fields outside the generic type declaration.

class Foo<T>
{
	public T Hello;

	public Foo ()
	{ }
}

class X
{
	public static void Main ()
	{
		Foo<int> foo = new Foo<int> ();
		foo.Hello = 9;
	}
}
