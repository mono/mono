// cs0057.cs: Inconsistent accessibility. Parameter type is less accessible than operator.
// Line: 11

using System;

class ErrorCS0057 {
	public ErrorCS0057 ();
}

public class Foo {
	public static implicit operator Foo(ErrorCS0057 bar) {
		return new Foo ();
	}

	public static void Main () {
	}
}

