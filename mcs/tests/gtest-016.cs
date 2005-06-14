// A generic method may also use the type parameters
// from its enclosing type.

class Stack<S>
{
	public static void Hello<T> (S s, T t)
	{ }
}

class X
{
	Stack<int> stack;

	static void Main ()
	{
	}
}
