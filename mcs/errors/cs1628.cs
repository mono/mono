// CS1628: Parameter `a' cannot be used inside `anonymous method' when using `ref' or `out' modifier
// Line: 15
using System;

delegate void D ();

class X {
	static void Main ()
	{
	}

	static void Host (ref int a)
	{
		D b = delegate {
			a = 1;
		};
	}
}
