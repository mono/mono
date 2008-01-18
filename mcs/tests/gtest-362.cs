class C
{
	static void Assert<T> (T a)
	{
	}

	static void Assert<T> (T a, T b)
	{
	}
	
	public static int Main ()
	{
		Assert (new object [,] { { 1, 2 }, { "x", "z" } });
		Assert (new object (), "a");
		
		return 0;
	}
}
