// CS1676: Parameter `1' must be declared with the `ref' keyword
// Line: 11
//
// The delegate has an explicit signature, so it can not be assigned.
//
delegate void D (ref int x);

class X {
	static void Main ()
	{
		D d2 = delegate (int x) {};
	}
}
