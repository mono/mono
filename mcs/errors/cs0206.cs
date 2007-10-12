// CS0206:  A property or indexer `X.P' may not be passed as `ref' or `out' parameter
// Line: 15

class X {
	static int P { get { return 1; } set { } }

	static int m (out int v)
	{
		v = 1;
		return 1;
	}
	
	static void Main ()
	{
		m (out P);
	}
}
