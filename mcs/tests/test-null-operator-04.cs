using System;

public class D
{
	void Foo ()
	{
	}

	public static void Main()
	{
		D d = null;
		Action a = d?.Foo;
	}
}
