using System;

public class HashedLinkedList<T>
{
	public int? Offset;

	public static HashedLinkedList<T> GetList ()
	{
		return new HashedLinkedList<T> ();
	}

	public static void Test (int added)
	{
		GetList ().Offset += added;
	}

	public void Test (HashedLinkedList<T> view)
	{
		view.Offset--;
	}
}

class X
{
	public static void Main ()
	{
		HashedLinkedList<int>.Test (5);
		HashedLinkedList<long> list = new HashedLinkedList<long> ();
		list.Test (list);
	}
}
