// CS1501: No overload for method `Foo' takes `3' arguments
// Line: 12

class Test
{
	public static void Foo (string s = null, int value = 2)
	{
	}

	static void Main ()
	{
		Foo ("a", 2, 6);
	}
}
