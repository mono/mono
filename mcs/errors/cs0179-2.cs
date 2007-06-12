// CS0179: `Bar.OnFoo.add' cannot declare a body because it is marked extern
// Line: 9

using System;

public delegate void FooHandler ();

class Bar {
	extern event FooHandler OnFoo {
		add { }
		remove { }
	}
}

