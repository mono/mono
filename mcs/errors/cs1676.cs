// CS1676: Signature mismatch in parameter modifier for parameter #1
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
