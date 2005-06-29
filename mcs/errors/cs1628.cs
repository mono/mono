// cs1628.cs: Cannot use ref or out parameter `a' inside an anonymous method block
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
