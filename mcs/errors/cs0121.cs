// CS0121: The call is ambiguous between the following methods or properties: `X.a(int, double)' and `X.a(double, int)'
// Line: 15

class X {
	static void a (int i, double d)
	{
	}

	static void a (double d, int i)
	{
	}

	public static void Main ()
	{
		a (0, 0);
	}
}	
