class Stack<T>
{
	T[] t;

	public Stack (int n)
	{
		t = new T [n];
	}

	public object Test ()
	{
		// Boxing the type parameter to an object; note that we're
		// an array !
		return t;
	}
}

class X
{
	public static void Main ()
	{
		Stack<int> stack = new Stack<int> (5);
		System.Console.WriteLine (stack.Test ());
	}
}
