// cs0053.cs: Inconsistent accessibility. Property type is less accessible than property.
// Line: 10

using System;

class ErrorCS0053 {
}

class Foo {
	public ErrorCS0053 Property {
		get { return new ErrorCS0053 (); } 
	}

	public static void Main () {
	}

	ErrorCS0053 error;
}

