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
	T t;

	public void Test ()
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
	static void Main()
	{
	}
}
