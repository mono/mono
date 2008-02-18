

using System;
using System.Collections.Generic;

class C
{
	public static void Foo<TSource> (IEnumerable<TSource> a)
	{
	}
	
	public static void Foo<TCollection, TSource> (IEnumerable<TSource> a,
		Func<TSource, IEnumerable <TCollection>> b)
	{
	}
	
	public static void Foo<TCollection, TSource> (IEnumerable<TSource> a,
		Func<TSource, TCollection[], IEnumerable <TCollection>> b)
	{
	}
	
	public static void Foo<TCollection, TSource> (Func<TCollection[], IEnumerable <TSource>> b)
	{
	}	
	
	public static void Main ()
	{
		int[] a = new int [] { 1 };
		Foo (a);
		Foo (a, (int i) => { return a; });
		Foo (a, (int i, int[] b) => { return a; });
		Foo ((int[] b) => { return a; });
	}
}

