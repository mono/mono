// cs0057.cs: Inconsisten accessibility. Parameter type is less accessible than operator.
// Line: 10

using System;

class ErrorCS0057 {
}

class Foo {
	public static implicit operator ErrorCS0057(Foo bar) {
		return new ErrorCS0057 ();
	}

	public static void Main () {
	}
}

