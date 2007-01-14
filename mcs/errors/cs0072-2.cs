// CS0072: `Child.OnFoo': cannot override because `ErrorCS0072.OnFoo()' is not an event
// Line: 16

using System;

class ErrorCS0072 {
	public delegate void FooHandler ();
	protected void OnFoo () {}
}

class Child : ErrorCS0072 {
	// We are trying to override a method with an event.
	protected override event FooHandler OnFoo;

	public static void Main () {
	}
}

