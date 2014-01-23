using System;

public class MyLinkedList<T> {
	protected Node first;

	protected class Node
	{
		public T item;

		public Node (T item)
		{
			this.item = item; 
		}
	}
}

class SortedList<U> : MyLinkedList<U>
{
	public void Insert (U x) { 
		Node node = first;
	}
}

class X {
	public static void Main ()
	{ }
}
