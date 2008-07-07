// CS1615: Argument `#1' does not require `ref' modifier. Consider removing `ref' modifier
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
