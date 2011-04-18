// CS0059: Inconsistent accessibility: parameter type `ErrorCS0059' is less accessible than delegate `Foo.ErrorCS0059Delegate'
// Line: 10

using System;

class ErrorCS0059 {
}

public class Foo {
	public delegate void ErrorCS0059Delegate (ErrorCS0059 e);

	public static void Main () {
	}
}

