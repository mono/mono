// cs0056.cs: Incompatible accessibility. Operator return type is less accessible than operator.
// Line: 11

using System;

class ErrorCS0056 {
	public ErrorCS0056 () {}
}

public class Foo {
	public static implicit operator ErrorCS0056(Foo foo) {
		return new ErrorCS0056 ();
	}
	public static void Main () {
	}
}
