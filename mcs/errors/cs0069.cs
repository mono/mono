// cs0069.cs: Event cannot have add or remove accessors in an interface.
// Line: 13

using System;

class ErrorCS0069 {
	public delegate void FooHandler ();
	public static void Main () {
	}
}

interface IBar {
	event OnFoo {
		add { }
		remove { }
	}
}

