// cs0069.cs: Event cannot have add or remove accessors in an interface.
// Line: 13

using System;

public delegate void FooHandler ();

interface IBar {
	event FooHandler OnFoo {
		add { }
		remove { }
	}
}

