// CS1738: Named argument `a' specified multiple times
// Line: 12

class C
{
	static void Foo (int a)
	{
	}
	
	public static void Main ()
	{
		Foo (a : 1, a : 2);
	}
}
