// CS0403: Cannot convert null to the type parameter `T' because it could be a value type. Consider using `default (T)' instead
// Line: 8

struct S
{
	public void Foo<T> () where T : struct
	{
		T t = null;
	}

	static void Main ()
	{ }
}
