// CS0050: Inconsistent accessibility: return type `ErrorCS0052' is less accessible than method `Foo.Method()'
// Line: 10

using System;

class ErrorCS0052 {
}

public class Foo {
	public ErrorCS0052 Method () {
		Console.WriteLine ("The compile should advice the return type of this method is less accessible than the method.");
	}
	public static void Main () {}
}

