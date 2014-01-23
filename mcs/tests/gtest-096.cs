using System;

class Foo<T>
{ }

class Test
{
	static void Hello<T> (Foo<T>[] foo, int i)
	{
		Foo<T> element = foo [0];
		Console.WriteLine (element);
		if (i > 0)
			Hello<T> (foo, i - 1);
	}

	public static void Quicksort<U> (Foo<U>[] arr)
	{
		Hello<U> (arr, 1);
	}

	public static void Main ()
	{
		Foo<int>[] foo = new Foo<int> [1];
		foo [0] = new Foo<int> ();
		Quicksort (foo);
	}
}
