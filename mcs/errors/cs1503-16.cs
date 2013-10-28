// CS1503: Argument `#2' cannot convert `method group' expression to type `System.Collections.Generic.Comparer<int>'
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
