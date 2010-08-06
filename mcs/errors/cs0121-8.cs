// CS0121: The call is ambiguous between the following methods or properties: `C.Foo(byte)' and `C.Foo(int)'
// Line: 18

class C
{
	static int Foo (byte b = 9)
	{
		return 4;
	}
	
	static int Foo (int i = 8)
	{
		return 2;
	}
	
	public static void Main ()
	{
		Foo ();
	}
}
