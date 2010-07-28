// CS0200: Property or indexer `X.this[int]' cannot be assigned to (it is read-only)
// Line: 14

class X {
	int this[int i] {
		get {
			return 1;
		}
	}

	static int Main ()
	{
		X x = new X ();
		x[0] = 10;
		return 1;
	}
}
