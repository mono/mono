// CS1678: Parameter `1' is declared as type `long' but should be `int'
// Line: 9

delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate (long x) {};
	}
}
