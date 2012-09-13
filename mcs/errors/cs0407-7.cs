// CS0407: A method or delegate `dynamic Program.Foo()' return type does not match delegate `string System.Func<string>()' return type
// Line: 10

using System;

class Program
{
	static void Main()
	{
		Func<string> d = Foo;
	}

	static dynamic Foo ()
	{
		return 1;
	}
}
