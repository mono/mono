// CS0246: The type or namespace name `wrong' could not be found. Are you missing an assembly reference?
// Line: 15

using System;

class X
{
	static void Foo<T> () where T : class
	{
	}

	public static void Main ()
	{
		Action a = () => {
			Foo<wrong> ();
		};
	}
}