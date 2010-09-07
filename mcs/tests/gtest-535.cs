class G<T> where T : class
{
}

class A
{
	static int Foo<T> (T a, G<T> y) where T : class
	{
		return 1;
	}

	static int Foo<T> (T a, object y)
	{
		return 2;
	}

	public static int Main ()
	{
		if (A.Foo<int> (99, null) != 2)
			return 1;

		if (A.Foo (66, null) != 2)
			return 2;

		return 0;
	}
}