// CS1501: No overload for method `TestCall' takes `1' arguments
// Line: 13

class C
{
	static void TestCall (byte b, int a)
	{
	}

	public static void Main ()
	{
		dynamic d = 0;
		TestCall (d);
	}
}
