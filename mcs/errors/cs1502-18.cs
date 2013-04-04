// CS1502: The best overloaded method match for `X.Add(params object[])' has some invalid arguments
// Line: 8

class X
{
	public static void Main ()
	{
		Add (Foo (), Foo ());
	}

	public static void Add (params object[] args)
	{
	}

	static void Foo ()
	{
	}
}