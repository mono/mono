// CS0067: The event `Foo.OnFoo' is never used
// Line: 12
// Compiler options: -warnaserror -warn:3

using System;

class Foo
{
	public event FooHandler OnFoo;
	public delegate void FooHandler ();

	public static void Main ()
	{
	}
}

