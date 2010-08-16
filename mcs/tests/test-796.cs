// Compiler options: -warnaserror -warn:4

// No CS0649 warnings

using System;

[Foo (Product = "Mono")]
class Program
{
	static void Main ()
	{
	}
}

class FooAttribute : Attribute
{
	public string Product;

	public FooAttribute ()
	{
	}
}
