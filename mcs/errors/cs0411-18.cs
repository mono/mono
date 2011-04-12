// CS0411: The type arguments for method `C.Foo<T>(T)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 10

using System;

class C
{
	static void Main ()
	{
		Foo (new TypedReference ());
	}

	static void Foo<T> (T arg)
	{
	}
}
