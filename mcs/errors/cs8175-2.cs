// CS8175: Cannot use by-reference variable `s' inside an anonymous method, lambda expression, or query expression
// Line: 17
// Compiler options: -langversion:latest

using System;

public ref struct S
{
}

class Test
{
	public static void Main ()
	{
		var s = new S ();

		Action a = () => Console.WriteLine (s);
	}
}