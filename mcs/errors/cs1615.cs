// CS1615: Argument '1' should not be passed with the 'ref' keyword
// Line: 11

class C
{
	public static void test (int i) {}

	public static void Main()
	{
		int i = 1;
		test (ref i);
	}
}
