// Note how the order of type parameters is different
// in the base class.

class Foo<T>
{
	public Foo ()
	{ }

	public void Hello (T t)
	{ }
}

class Bar<T,U> : Foo<U>
{
	public Bar ()
	{ }

	public void Test (T t, U u)
	{ }
}

class X
{
	static void Test (Bar<int,string> bar)
	{
		bar.Hello ("Test");
		bar.Test (7, "Hello");
	}

	public static void Main ()
	{
		Bar<int,string> bar = new Bar<int,string> ();
		Test (bar);
	}
}
