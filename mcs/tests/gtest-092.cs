using System;

public class Element<T>
{
	public readonly T Item;

	public Element (T item)
	{
		this.Item = item;
	}

	public void GetItem (out T retval)
	{
		retval = Item;
	}

	public T GetItem (int a, ref T data)
	{
		return Item;
	}

	public void SetItem (T data)
	{ }
}

public class Foo<T>
{
	Element<Node> element;

	public Node Test ()
	{
		Node node = element.Item;
		element.GetItem (out node);
		element.SetItem (node);
		return element.GetItem (3, ref node);
	}

	public class Node
	{
	}
}

class X
{
	public static void Main ()
	{ }
}
