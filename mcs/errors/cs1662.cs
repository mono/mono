// cs1662: Can not convert the anonymous method, as the returned value does not match the return type of the delegate
// Line: 11
//
// Return type mismatch.
//
delegate void D (int x);

class X {
	static void Main ()
	{
		D d6 = delegate (int x) { return x; }; // Return type mismatch.
	}
}
