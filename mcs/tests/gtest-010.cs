// Type parameters with constraints: check whether we can invoke
// things on the constrained type.

using System;

interface I
{
	void Hello ();
}

class J
{
	public void Foo ()
	{
		Console.WriteLine ("Foo!");
	}
}

class Stack<T>
	where T : J, I
{
	public void Test (T t)
	{
		t.Hello ();
		t.Foo ();
	}
}

class Test
{
}

class X
{
	public static void Main()
	{
	}
}
