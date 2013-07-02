//
// This tests the use of an array indexer on value exprclasses
// and not only variables
//
class X {
	static int [] g ()
	{
		int [] x = new int [5];
		x [1] = 10;
		return x;
	}

	public static int Main ()
	{
		if (g () [1] == 10)
				return 0;
		return 1;
	}
}
