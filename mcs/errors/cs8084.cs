// CS8084: An argument to nameof operator cannot be method group with type arguments
// Line: 12

static class C
{
	static void Foo<T> ()
	{
	}

	static void Main ()
	{
		string s = nameof (Foo<int>);
	}
}