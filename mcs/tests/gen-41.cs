class Stack<T>
{
	public void Hello (int a, params T[] args)
	{ }
}

class X
{
	static void Main ()
	{
		Stack<string> stack = new Stack<string> ();
		stack.Hello (1, "Hello", "World");
	}
}
