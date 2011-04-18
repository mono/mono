// CS1677: Parameter `1' should not be declared with the `ref' keyword
// Line: 9

delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate (ref int x) {};
	}
}
