// CS0057: Inconsistent accessibility: parameter type `ErrorCS0057' is less accessible than operator `Foo.implicit operator Foo(ErrorCS0057)'
// Line: 11

using System;

class ErrorCS0057 {
	public ErrorCS0057 () {}
}

public class Foo {
	public static implicit operator Foo(ErrorCS0057 bar) {
		return new Foo ();
	}

	public static void Main () {
	}
}

