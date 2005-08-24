// cs0200.cs: Property or indexer `X.P' cannot be assigned to (it is read only)
// Line: 12
class X {
	static int P {
		get {
			return 1;
		}
	}

	static int Main ()
	{
		P = 10;
		return 1;
	}
}
