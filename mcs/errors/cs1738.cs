// CS1738: Named arguments must appear after the positional arguments when using language version older than 7.2
// Line: 12

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
