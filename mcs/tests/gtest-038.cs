//
// Another important test: nested generic types.
//

using System;

class Queue<T>
{
	public Queue (T first, T second)
	{
		head = new Node<T> (null, second);
		head = new Node<T> (head, first);
	}

	protected Node<T> head;

	protected Node<T> GetFoo ()
	{
		return head;
	}

	protected Node<T> Foo {
		get {
			return GetFoo ();
		}
	}

	protected void Test (T t)
	{
		Console.WriteLine (t);
	}

	public void Test ()
	{
		Test (head.Item);
		Test (head.Next.Item);
		Test (GetFoo ().Item);
		Test (Foo.Item);
	}

	protected class Node<U>
	{
		public readonly U Item;
		public readonly Node<U> Next;

		public Node (Node<U> next, U item)
		{
			this.Next = next;
			this.Item = item;
		}
	}
}

class X
{
	public static void Main ()
	{
		Queue<int> queue = new Queue<int> (5, 9);
		queue.Test ();
	}
}
