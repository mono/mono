// Compiler options: -warnaserror -warn:4

// No CS0649 warnings

using System;

struct S
{
}

class C2
{
	S s;
	
	void Foo ()
	{
		Func<string> f = s.ToString;
		Console.WriteLine (f);
	}
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
