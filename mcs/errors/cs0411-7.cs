// CS0411: The type arguments for method `C.Foo<T>(T, System.Collections.Generic.Comparer<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 20

using System;
using System.Collections.Generic;

public class C
{
	static void Foo<T>(T t, Comparer<T> tc)
	{
	}
	
	static int Compare (int a, int b)
	{
		return -1;
	}
	
	public static void Main ()
	{
		Foo (1, Compare);
	}
}
