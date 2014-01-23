using System;

class FooAttribute : Attribute
{

	public FooAttribute (string f)
	{
	}
}

[Foo (Bar)]
class Test
{

	const string Bar = "Bar";
}

class Program
{

	public static void Main ()
	{
	}
}
