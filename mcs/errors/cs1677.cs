// cs1677.cs: Parameter `1' should not be declared with the `out' keyword
// Line: 11
//
// The delegate has an explicit signature, so it can not be assigned.
//
delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate (out int x) {};
	}
}
