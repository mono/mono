struct Stack<S>
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

	static void Main ()
	{ }
}
