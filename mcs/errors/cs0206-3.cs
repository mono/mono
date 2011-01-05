// CS0206: A property, indexer or dynamic member access may not be passed as `ref' or `out' parameter
// Line: 14

class C
{
	static void Foo (ref object o)
	{
	}
	
	public static void Main ()
	{
		var v = new { Foo = "Bar" };
		
		Foo (ref v.Foo);
	}
}
