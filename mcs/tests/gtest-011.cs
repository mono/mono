class Stack<S>
{
	public void Hello (S s)
	{ }
}

class X
{
	Stack<int> stack;

	void Test ()
	{
		stack.Hello (3);
	}

	public static void Main ()
	{ }
}
