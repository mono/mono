public class Stack<S>
{
	public Stack (S s)
	{ }

	public void Push (S s)
	{ }
}

public class X
{
	public static void Main ()
	{
		Stack<int> s1 = new Stack<int> (3);
		s1.Push (4);

		Stack<string> s2 = new Stack<string> ("Hello");
		s2.Push ("Test");
	}
}
