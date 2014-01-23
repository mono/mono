// Generic interfaces

interface Foo<S>
{
	void Hello (S s);
}

interface Bar<T,U> : Foo<U>
{
	void Test (T t, U u);
}

class X
{
	static void Test (Bar<int,string> bar)
	{
		bar.Hello ("Test");
		bar.Test (7, "Hello");
	}

	public static void Main ()
	{ }
}
