// CS1502: The best overloaded method match for `C.TestCall(int, string)' has some invalid arguments
// Line: 13

class C
{
	static void TestCall (int i, string s)
	{
	}
	
	public static void Main ()
	{
		dynamic d = 0;
		TestCall (d, 1);
	}
}
