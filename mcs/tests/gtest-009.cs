using System;

interface I
{
	void Hello ();
}

class Stack<T>
	where T : ICloneable
{
	public object Test (T t)
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
