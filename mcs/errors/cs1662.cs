// cs1662.cs: Cannot convert anonymous method block to delegate type `D' because some of the return types in the block are not implicitly convertible to the delegate return type
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
