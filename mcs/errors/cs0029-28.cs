// CS0029: Cannot implicitly convert type `T[]' to `U[]'
// Line: 8

class Test
{
	static void Main ()
	{
		Foo<int, object> (new int[] { 1 });
	}

	static U[] Foo<T, U> (T[] arg) where T : U where U : class
	{
		return arg;
	}
}
