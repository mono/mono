using System;

public class Stack<S>
{
	public Stack (S s)
	{
		Console.WriteLine ("STACK CTOR: {0} {1}", s, s.GetType ());
	}

	public void Push (S s)
	{
		Console.WriteLine ("STACK PUSH: {0} {1}", s, s.GetType ());
	}
}

public class X
{
	static void Main ()
	{
		Stack<int> s1 = new Stack<int> (3);
		Stack<string> s2 = new Stack<string> ("Hello");

		Console.WriteLine (s1);
		s1.Push (4);
		Console.WriteLine (s2);
		s2.Push ("Test");
	}
}
