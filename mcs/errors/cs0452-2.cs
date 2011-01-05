// CS0452: The type `int' must be a reference type in order to use it as type parameter `T' in the generic type or method `Foo.Test<T>(ref T)'
// Line: 14
public class Foo
{
	public static void Test<T> (ref T t)
		where T : class
	{ }
}

public class C {
	public static void Main ()
	{
		int i = 0;
		Foo.Test<int> (ref i);
	}
}
