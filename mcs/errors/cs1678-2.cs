// CS1678: Parameter `1' is declared as type `T' but should be `IStream<T>'
// Line: 14

using System;

interface IStream<T>
{
}

static class X
{
	public static IStream<U> Select<T, U> (IStream<T> stream, Func<IStream<T>, U> selector)
	{
		return Select<T, U> (stream, (T _) => selector(stream));
	}
}
