// cs0052.cs: Accessibility levels inconsistent. Field type is less accessible than field.
// Line: 

using System;

class ErrorCS0052 {
}

public class Foo {
	public ErrorCS0052 Method () {
		Console.WriteLine ("The compile should advice the return type of this method is less accessible than the method.");
	}
	public static void Main () {}
}

