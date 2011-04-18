// CS0407: A method or delegate `int Program.Foo(object)' return type does not match delegate `void System.Action<dynamic>(dynamic)' return type
// Line: 10

using System;

class Program
{
	static void Main()
	{
		Action<dynamic> d = Foo;
	}

	static int Foo (object o)
	{
		return 0;
	}
}
