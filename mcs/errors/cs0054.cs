// cs0054.cs: Inconsistent accessibility. Indexer return type is less accessible than indexer.
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

