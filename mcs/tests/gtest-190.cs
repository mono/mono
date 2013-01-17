using System;
using System.Collections.Generic;

public class Foo<T>
{
	public abstract class Node
	{ }

	public class ConcatNode : Node
	{ }

	public Node GetRoot ()
	{
		return new ConcatNode ();
	}

	public void Test (Node root)
	{
		ConcatNode concat = root as ConcatNode;
		Console.WriteLine (concat);
	}
}

class X
{
	public static void Main ()
	{
		Foo<int> foo = new Foo<int> ();
		Foo<int>.Node root = foo.GetRoot ();
		foo.Test (root);
	}
}
