// CS1502: The best overloaded method match for `C.Foo(dynamic)' has some invalid arguments
// Line: 13

static class C
{
	public static void Foo (dynamic d)
	{
	}

	static void Main()
	{
		dynamic d = null;
		Foo (ref d);
	}
}
