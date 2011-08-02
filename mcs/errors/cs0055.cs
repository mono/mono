// CS0055: Inconsistent accessibility: parameter type `ErrorCS0055' is less accessible than indexer `Foo.this[ErrorCS0055]'
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

