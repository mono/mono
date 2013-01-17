using System;
using System.Collections.Generic;
using System.Linq;

// Tests caching of closed generic anonymous delegates

static class C
{
	public static decimal Average<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
	{
		return source.Select (selector).Average<decimal, decimal, decimal> ((a, b) => a + b, (a, b) => a / b);
	}

	static TResult Average<TElement, TAggregate, TResult> (this IEnumerable<TElement> source,
		Func<TAggregate, TElement, TAggregate> func, Func<TAggregate, TElement, TResult> result)
		where TElement : struct
		where TAggregate : struct
		where TResult : struct
	{
		throw new InvalidOperationException ();
	}

	public static void Main ()
	{
	}
}

