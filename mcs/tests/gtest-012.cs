// A generic type definition may have another generic type
// definition as its parent.

class Stack<S>
{
	public void Hello (S s)
	{ }		
}

class Test<T> : Stack<T>
{
	public void Foo (T t)
	{ }
}

class X
{
	Test<int> test;

	void Test ()
	{
		test.Foo (4);
		test.Hello (3);
	}

	public static void Main ()
	{ }
}
