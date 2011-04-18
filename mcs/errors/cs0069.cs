// CS0069: Event in interface cannot have add or remove accessors
// Line: 13

using System;

public delegate void FooHandler ();

interface IBar {
	event FooHandler OnFoo {
		add { }
		remove { }
	}
}

