// cs0059.cs: Iconsisten accessibility. Parameter type less accessible than delegate.
// Line: 10

using System;

class ErrorCS0059 {
}

public class Foo {
	public delegate void ErrorCS0059Delegate (ErrorCS0059 e);

	public static void Main () {
	}
}

