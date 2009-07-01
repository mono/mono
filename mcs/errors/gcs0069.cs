// CS0069: Event in interface cannot have add or remove accessors
// Line: 11

using System;

public delegate void FooHandler ();

interface IBar<T>
{
	event FooHandler OnFoo {
		remove { }
	}
}

