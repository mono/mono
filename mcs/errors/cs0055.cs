// cs0055.cs: Inconsistent accessibility. Parameter type is less accessible than indexer.
// Line:  11

using System;

class ErrorCS0055 {
	public ErrorCS0055 () {}
}

public class Foo {
	public int this[ErrorCS0055 e] {
		get { return 5; }
	}

	public static void Main () {
	}
}

