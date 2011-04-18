// CS0122: The call is ambiguous between the following methods or properties: `Test.Foo(IIn<string>)' and `Test.Foo(IIn<Test>)'
// Line: 22

interface IIn<in T>
{
}

class Test
{

	static void Foo (IIn<string> f)
	{
	}

	static void Foo (IIn<Test> f)
	{
	}

	public static int Main ()
	{
		IIn<object> test = null;
		Foo (test);

		return 0;
	}
}
