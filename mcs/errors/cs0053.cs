// CS0053: Inconsistent accessibility: property type `ErrorCS0053' is less accessible than property `Foo.Property'
// Line: 11

using System;

class ErrorCS0053 {
	public ErrorCS0053 () {}
}

public class Foo {
	public ErrorCS0053 Property {
		get { return new ErrorCS0053 (); } 
	}

	public static void Main () {
	}

	ErrorCS0053 error;
}

