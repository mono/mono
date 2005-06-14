interface I
{
	void Hello ();
}

class Stack<T>
	where T : I, new ()
{
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
