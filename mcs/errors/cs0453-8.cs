// CS0453: The type `dynamic' must be a non-nullable value type in order to use it as type parameter `T' in the generic type or method `Tester.Foo<T>(T)'
// Line: 10

class Tester
{
	static void Foo<T> (T t) where T : struct
	{
	}
	
	public static int Main ()
	{
		dynamic d = 1;
		Foo<dynamic>(d);
		return 0;
	}
}
