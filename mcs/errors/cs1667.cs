// cs1677: out parameters are not permitted on anonymous delegate declarations.
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
