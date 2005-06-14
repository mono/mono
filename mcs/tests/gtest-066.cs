using System;

public class Foo<T>
{
}

class X
{
	static void Main ()
	{
		Console.WriteLine (typeof (Foo<>));
	}
}
