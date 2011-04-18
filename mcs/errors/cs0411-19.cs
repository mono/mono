// CS0411: The type arguments for method `C.TestCall<T>(int)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 13

class C
{
	static void TestCall<T> (int i)
	{
	}
	
	public static void Main ()
	{
		dynamic d = 0;
		TestCall (d);
	}
}
