// CS1928: Type `string' does not contain a member `Check' and the best extension method overload `C.Check(this string, int)' has some invalid arguments
// Line: 12

static class C
{
	public static void Check (this string s, int i)
	{
	}

	static void Main ()
	{
		"alo".Check ("o");
	}
}
