// CS1973: Type `string' does not contain a member `Foo' and the best extension method overload `C.Foo(this string, dynamic)' cannot be dynamically dispatched. Consider calling the method without the extension method syntax
// Line: 13

static class C
{
	public static void Foo (this string s, dynamic d)
	{
	}

	static void Main()
	{
		dynamic d = null;
		"x".Foo (d);
	}
}
