// cs0055.cs: Inconsistent accessibility. Parameter type is less accessible than indexer.
// Line:  11

using System;

class ErrorCS0055 {
	public ErrorCS0055 () {}
}

public class Foo {
	public ErrorCS0055 this[ErrorCS0055 e] {
		get { return new ErrorCS0055 (); }
	}

	public static void Main () {
	}
}

