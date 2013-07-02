// CS1501: No overload for method `Foo' takes `1' arguments
// Line: 15

using System;

class MainClass
{
	public static void Main ()
	{
		int val = 2;
		Run (() => {
			if (val > 3)
				return;

			Foo (5);
		});
	}

	static void Foo ()
	{
	}

	static void Run<T> (Func<T> func)
	{
	}

	static void Run<T> (Action act)
	{
	}
}
