// CS1503: Argument 1: Cannot convert type `bool' to `int'
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
