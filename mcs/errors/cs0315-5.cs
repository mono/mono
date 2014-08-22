// CS0315: The type `int' cannot be used as type parameter `T' in the generic type or method `H<T>'. There is no boxing conversion from `int' to `I'
// Line: 27

using System;

interface I
{
}

class H<T> where T : I, new()
{
}

public class A
{
	static void Test (Action a)
	{
	}

	static void Foo<T>()
	{
	}

	static void Main ()
	{
		Test (() => {
			Foo<H<int>> ();
		});
	}
}