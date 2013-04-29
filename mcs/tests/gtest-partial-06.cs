partial class Test
{
	static partial void Foo<T> ();

	static partial void Baz<T> ();

	static partial void Baz<U> ()
	{
	}

	static partial void Bar<T> (T t) where T : class;

	static partial void Bar<U> (U u) where U : class
	{
	}

	public static void Main ()
	{
		Foo<long> ();
		Baz<string> ();
		Bar<Test> (null);
	}
}