// CS0411: The type arguments for method `C.Foo<T>(System.Func<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 14

using System;

class C
{
	static void Foo<T> (Func<T> a)
	{
	}
	
	static void Main ()
	{
		Foo (() => Main);
	}
}
