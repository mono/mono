using System;

static class Maybe
{
	public static Maybe<T> C<T> (T value)
	{
		return null;
	}
}

sealed class Maybe<T>
{
	public Maybe (T value)
	{
	}
}

static class Extensions {
	public static R Match<T,R>(this T self, params Func<T,Maybe<R>>[] matchers)
	{
		return default (R);
	}
}

class Test {
	public static void Main ()
	{
		Extensions.Match ("a", s => Maybe.C(s));
		Extensions.Match ("a", s => Maybe.C(s), s => Maybe.C("a"));
	}
}
