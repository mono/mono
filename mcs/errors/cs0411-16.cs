// CS0411: The type arguments for method `M.Foo<T>(System.Func<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 14

using System;

class M
{
	static void Foo<T> (Func<T> t)
	{
	}
	
	public static void Main ()
	{
		Foo (delegate { throw new Exception("foo"); });
	}
}
