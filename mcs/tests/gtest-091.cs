using System;

public class Foo<T>
{
	Node node;

	public Node Test<V> ()
	{
		return node;
	}

	public class Node
	{ }
}

class X
{
	public static void Main ()
	{ }
}
