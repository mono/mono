public class Stack<T>
{
	public Stack ()
	{ }

	public Node GetNode (T t)
	{
		return new Node (t);
	}

	public Foo<T> GetFoo (T t)
	{
		return new Foo<T> (t);
	}

	public Bar<T> GetBar (T t)
	{
		return new Bar<T> (t);
	}

	public class Node
	{
		public readonly T Data;

		public Node (T t)
		{
			this.Data = t;
		}
	}

	public class Foo<T>
	{
		public readonly T Data;

		public Foo (T t)
		{
			this.Data = t;
		}
	}

	public class Bar<U>
	{
		public readonly U Data;

		public Bar (U u)
		{
			this.Data = u;
		}
	}
}

class X
{
	static void Main ()
	{
		Stack<int> stack = new Stack<int> ();
		Stack<int>.Node node = stack.GetNode (9);
		Stack<int>.Foo<int> foo = stack.GetFoo (7);
		Stack<int>.Bar<int> bar = stack.GetBar (8);
	}
}
