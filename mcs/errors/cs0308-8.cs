// CS0308: The non-generic method `C.TestCall(int)' cannot be used with the type arguments
// Line: 13

class C
{
	static void TestCall (int i)
	{
	}
	
	public static void Main ()
	{
		dynamic d = 0;
		TestCall<int> (d);
	}
}
