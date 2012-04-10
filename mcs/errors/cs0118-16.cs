// CS0118: `X.Foo' is a `property' but a `method group' was expected
// Line: 12

class X
{
	static int Foo {
		get { return 1; }
	}

	static void Main ()
	{
		Foo (1);
	}
}
