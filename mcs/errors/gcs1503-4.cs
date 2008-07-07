// CS1503: Argument `#1' cannot convert `bool' expression to type `int'
// Line: 12

public class C
{
	static void Foo<T>(T t)
	{
	}
	
	public static void Main ()
	{
		Foo<int> (true);
	}
}
