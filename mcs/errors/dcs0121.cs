// CS0121: The call is ambiguous between the following methods or properties: `X.a(int, dynamic)' and `X.a(double, object)'
// Line: 16

class X
{
	static void a (int i, dynamic d)
	{
	}

	static void a (double d, object i)
	{
	}

	public static void Main ()
	{
		a (0, 0);
	}
}	
