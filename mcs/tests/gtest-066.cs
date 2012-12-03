using System;
using A = Test;

public class Foo<T>
{
	public class Bar <U>
	{
	}
}

namespace Test
{
	class FooEx<V, W> {}
}

class X
{
	public static void Main ()
	{
		Console.WriteLine (typeof (Foo<>));
		Console.WriteLine (typeof (Foo<>.Bar<>));
		Console.WriteLine (typeof (Test.FooEx<,>));		
		Console.WriteLine (typeof (A::FooEx<,>));
	}
}