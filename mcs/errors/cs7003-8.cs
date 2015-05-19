// CS7003: Unbound generic name is not valid in this context
// Line: 12

static class C
{
	static void Foo<T> ()
	{
	}

 	static void Main ()
	{
		string s = nameof (C.Foo<>);
	}
}