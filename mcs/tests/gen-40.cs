public class Stack<T>
{
	public Stack ()
	{ }

	public Node GetNode (T t)
	{
		return new Node (t);
	}

	public class Node
	{
		public readonly T Data;

		public Node (T t)
		{
			this.Data = t;
		}
	}
}

class X
{
	static void Main ()
	{
		// Stack<int> stack = new Stack<int> ();
		// Stack<int>.Node node = stack.GetNode (9);
	}
}
