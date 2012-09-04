// CS0121: The call is ambiguous between the following methods or properties: `A.Foo<int>(int, G<int>)' and `A.Foo<int>(int, object)'
// Line:

class A
{
	static int Foo<T> (T a, G<T> y = null)
	{
		return 1;
	}

	static int Foo<T> (T a, object y = null)
	{
		return 2;
	}

	public static int Main ()
	{
		if (A.Foo<int> (99) != 2)
			return 1;

		return 0;
	}
}

class G<U>
{
}
