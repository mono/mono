// cs0067.cs: The event is never used.
// Line: 12
// Compiler options: -warnaserror -warn:4

using System;

class ErrorCS0067 {
	public delegate void FooHandler ();
}

class Foo {
	public event ErrorCS0067.FooHandler OnFoo;
	
	public static void Main () {
	}
}

