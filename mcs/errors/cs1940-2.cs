// CS1940: Ambiguous implementation of the query pattern `Select' for source type `string[]'
// Line: 11

using System;
using System.Collections.Generic;

class Multiple
{
	public static void Main ()
	{
		var q = from x in new [] { "a", "b", "c" } select x;
	}
}

static class Y
{
	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		return null;
	}
}

static class X
{
	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		return null;
	}
}