// CS0067: The event `Foo.OnFoo' is never used
// Line: 12
// Compiler options: -warnaserror -warn:4

using System;

class ErrorCS0067 {
	public delegate void FooHandler ();
}

class Foo {
	private event ErrorCS0067.FooHandler OnFoo;
	
	public static void Main () {
	}
}

