// cs1628: Can not access `ref' or `out' parameters in an anonymous method
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
