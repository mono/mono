using System;

public class Foo<T>
{
	public readonly T Item;

	public Foo (T item)
	{
		this.Item = item;
	}

	static void maketreer (out Node rest)
	{
		rest = new Node ();
	}

	class Node
	{ }

	public void Hello<U> ()
	{
		Foo<U>.Node node;
		Foo<U>.maketreer (out node);
	}
}

class X
{
	public static void Main ()
	{ }
}
