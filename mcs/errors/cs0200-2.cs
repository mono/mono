// CS0200: The read only property or indexer `X.this[int]' cannot be assigned to
// Line: 12

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
