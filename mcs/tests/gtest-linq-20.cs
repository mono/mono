using System;

class Maybe<T>
{
	public readonly static Maybe<T> Nothing = new Maybe<T> ();
	public T Value { get; private set; }
	public bool HasValue { get; private set; }
	Maybe ()
	{
		HasValue = false;
	}
	public Maybe (T value)
	{
		Value = value;
		HasValue = true;
	}

	public override string ToString ()
	{
		if (HasValue)
			return Value.ToString ();
		return string.Empty;
	}

	public Maybe<U> SelectMany<U> (Func<T, Maybe<U>> k)
	{
		if (!HasValue)
			return Maybe<U>.Nothing;
		return k (Value);
	}

	public Maybe<V> SelectMany<U, V> (
		Func<T, Maybe<U>> selector,
		Func<T, U, V> resultSelector)
	{
		if (!HasValue)
			return Maybe<V>.Nothing;
		Maybe<U> n = selector (Value);
		if (!n.HasValue)
			return Maybe<V>.Nothing;
		return resultSelector (Value, n.Value).ToMaybe ();
	}
}

static class MaybeExtensions
{
	public static Maybe<T> ToMaybe<T> (this T value)
	{
		return new Maybe<T> (value);
	}
}

class Test
{
	public static void Main ()
	{
		Console.WriteLine (
			from x in 1.ToMaybe ()
			from y in 2.ToMaybe ()
			from z in 3.ToMaybe ()
			select x + y + z);
	}
}
