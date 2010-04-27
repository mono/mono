// CS1661: Cannot convert `anonymous method' to delegate type `D' since there is a parameter mismatch
// Line: 9

delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate (ref int x) {};
	}
}
