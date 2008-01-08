// CS0411: The type arguments for method `X.Test<T>(Foo<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 17
// Compiler options: -langversion:ISO-2

using System;

public delegate void Foo<T> (T t);

class X
{
	public void Test<T> (Foo<T> foo)
	{ }

	static void Main ()
	{
		X x = new X ();
		x.Test (delegate (string str) { });
	}
}
