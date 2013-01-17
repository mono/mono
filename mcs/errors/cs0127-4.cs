// CS0127: `D': A return keyword must not be followed by any expression when delegate returns void
// Line: 9

delegate void D (int x);

class X {
	static void Main ()
	{
		D d6 = delegate (int x) { return x; }; // Return type mismatch.
	}
}
