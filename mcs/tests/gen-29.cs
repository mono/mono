class Stack<T>
{
	T[] t;

	public Stack (int n)
	{
		t = new T [n];
	}

	public object Test ()
	{
		return t;
	}
}

class X
{
	static void Main ()
	{
		Stack<int> stack = new Stack<int> (5);
		System.Console.WriteLine (stack.Test ());
	}
}
