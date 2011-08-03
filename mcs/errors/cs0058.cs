// CS0058: Inconsistent accessibility: return type `ErrorCS0058' is less accessible than delegate `Foo.Delegate'
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

