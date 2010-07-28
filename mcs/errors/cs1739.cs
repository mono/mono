// CS1739: The best overloaded method match for `C.Foo(int, int)' does not contain a parameter named `b'
// Line: 12

class C
{
	static void Foo (int x, int y)
	{
	}
	
	public static void Main ()
	{
		Foo (1, b : 2);
	}
}
