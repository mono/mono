// cs1678: signature missmatch on type parameter.
// Line: 11
//
// Signature mismatch.
//
delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate (long x) {};
	}
}
