// CS1738: Named arguments must appear after the positional arguments
// Line: 12
// Compiler options: -langversion:future

class C
{
	static void Foo (int a, string s)
	{
	}
	
	public static void Main ()
	{
		Foo (a : 1,  "out");
	}
}
