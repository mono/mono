// CS0452: The type `int' must be a reference type in order to use it as type parameter `T' in the generic type or method `A.Foo<T>(T, string)'
// Line: 18

class A
{
	static int Foo<T> (T a, string s) where T : class
	{
		return 1;
	}

	static int Foo<T> (T a, object y)
	{
		return 2;
	}

	public static void Main ()
	{
		A.Foo<int> (99, null);
	}
}
