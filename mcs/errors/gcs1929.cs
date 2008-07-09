// CS1929: Extension method instance type `int' cannot be converted to `string'
// Line: 12

static class C
{
	public static void Check (this string s)
	{
	}

	static void Main ()
	{
		1.Check ();
	}
}
