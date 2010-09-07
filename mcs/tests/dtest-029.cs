class C
{
	static bool Test<T, U>(T t, U u)
	{
		dynamic d1 = 1;
		dynamic d2 = "a";
		return d1 == t && d2 == u;
	}
	
	static bool Test2(int i)
	{
		dynamic d1 = 1;
		return d1 == i;
	}
	
	public static int Main ()
	{
		if (!Test (1, "a"))
			return 1;
		
		if (Test (2, "a"))
			return 2;

		if (Test (1, "----"))
			return 3;

		if (!Test2 (1))
			return 4;

		if (Test2 (2))
			return 5;

		return 0;
	}
}
