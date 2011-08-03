// CS0054: Inconsistent accessibility: indexer return type `ErrorCS0054' is less accessible than indexer `Foo.this[int]'
// Line: 13

using System;

class ErrorCS0054 {
	public ErrorCS0054 () {}
}

public class Foo {
	ErrorCS0054[] errors;

	public ErrorCS0054 this[int i] {
		get { return new ErrorCS0054 (); }
	}

	public static void Main () {
	}
}

