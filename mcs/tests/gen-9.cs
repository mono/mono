using System;

interface I
{
	void Hello ();
}

class Stack<T>
	where T : ICloneable
{
	T t;

	public object Test ()
	{
		return t.Clone ();
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
