// cs0058.cs: Incompatible accessibility. Parameter type is less accessible than delegate.
// Line: 10

using System;

class ErrorCS0058 {
	public ErrorCS0058 () {}
}

public class Foo {
	public delegate ErrorCS0058 Delegate ();

	public static void Main () {
	}
}

