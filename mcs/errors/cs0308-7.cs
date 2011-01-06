// CS0308: The non-generic type `Foo' cannot be used with the type arguments
// Line: 16

public class Foo
{
	public string Test<T> ()
	{
		return null;
	}
}

public static class Driver
{
	static object UseBrokenType ()
	{
		return Foo<int> ().Test ();
	}
}
