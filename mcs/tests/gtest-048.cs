// This fixed a problem in the JIT.

public class Stack<T>
{
	T[] data;

	public Stack ()
	{
		data = new T [10];
	}

	public void Add (T t)
	{
		data [0] = t;
	}
}

struct Foo
{
	int a;
}

class X
{
	public static void Main ()
	{
		Foo foo = new Foo ();
		Stack<Foo> stack = new Stack<Foo> ();
		stack.Add (foo);
	}
}
