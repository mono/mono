// cs0058.cs: Incompatible accessibility. Parameter type is less accessible than delegate.
// Line: 10

using System;

class ErrorCS0058 {
}

class Foo {
	public delegate ErrorCS0058 Delegate ();

	public static void Main () {
	}
}

