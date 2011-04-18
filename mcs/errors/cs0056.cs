// CS0056: Inconsistent accessibility: return type `ErrorCS0056' is less accessible than operator `Foo.implicit operator ErrorCS0056(Foo)'
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
