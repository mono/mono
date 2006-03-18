// cs1661.cs: Anonymous method could not be converted to delegate `D' since there is a parameter mismatch
// Line: 9

delegate void D (int x);

class X {
	static void Main ()
	{
		D d2 = delegate (ref int x) {};
	}
}
