class Stack<T>
{
	T t;

	public Stack (T t)
	{
		this.t = t;
	}

	public object Test ()
	{
		// Boxing the type parameter `T' to an object.
		return t;
	}
}

class X
{
	public static object Test (Stack<int> stack)
	{
		return stack.Test ();
	}

	public static void Main ()
	{
		Stack<int> stack = new Stack<int> (9);
		System.Console.WriteLine (Test (stack));
	}
}
