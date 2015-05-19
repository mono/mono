// CS1929: Type `int' does not contain a member `Check' and the best extension method overload `C.Check(this string)' requires an instance of type `string'
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
