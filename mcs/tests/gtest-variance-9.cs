interface IIn<in T>
{
}

class Test
{

	static void Foo (IIn<string> f)
	{
	}

	public static int Main ()
	{
		IIn<object> test = null;
		Foo (test);

		return 0;
	}
}
