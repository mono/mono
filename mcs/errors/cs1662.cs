// CS1662: Cannot convert `anonymous method' to delegate type `D' because some of the return types in the block are not implicitly convertible to the delegate return type
// Line: 11

delegate void D (int x);

class X {
	static void Main ()
	{
		D d6 = delegate (int x) { return x; }; // Return type mismatch.
	}
}
