// Very simple example of a generic method.

class Stack<S>
{
	public static void Hello<T,U> (S s, T t, U u)
	{
		U v = u;
	}
}

class X
{
	static void Main ()
	{
	}
}
