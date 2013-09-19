using System;

public delegate Tuple<D1, DR1> Parser<D1, DR1> (D1 stream);

static class Combinator
{
	public static Parser<L1, LR1> Lazy<L1, LR1> (Func<Parser<L1, LR1>> func)
	{
		return null;
	}

	public static Parser<C1, CR1> Choice<C1, CR1> (Parser<C1, CR1> parsers)
	{
		Parser<C1, CR1> tail = null;

		Lazy (() => Choice (tail));

		return null;
	}

	public static void Main ()
	{
		Choice ((int l) => Tuple.Create (1, 2));
	}
}