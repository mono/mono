// cs0052.cs: Accessibility levels inconsistent. Method type is less accessible than method.
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

