// CS1620: Argument `#1' is missing `out' modifier
// Line: 13

class C
{
	public static void test (out int i)
	{
		i = 5;
	}

	public static void Main() {
		int i = 1;
		test (ref i);
	}
}
