// Compiler options: -warnaserror -warn:4

// No CS0649 warnings

using System;

struct S
{
}

[Foo (Product = "Mono")]
class Program
{
	S s;
	
	void Test ()
	{
		s.ToString ();
	}
	
	public static void Main ()
	{
		new Program ().Test ();
	}
}

class FooAttribute : Attribute
{
	public string Product;

	public FooAttribute ()
	{
	}
}
