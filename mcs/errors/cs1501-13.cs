// CS1501: No overload for method `Foo' takes `2' arguments
// Line: 13

static class C
{
	public static void Foo (this string s, int d, bool b)
	{
	}

	static void Main()
	{
		dynamic d = null;
		"x".Foo (d);
	}
}
