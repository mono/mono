using System;
using System.Collections.Generic;

static class Enumerable
{

	public static int Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
	{
		return Sum<TSource, int> (source, (a, b) => a + selector (b));
	}

	static TR Sum<TA, TR> (this IEnumerable<TA> source, Func<TR, TA, TR> selector)
	{
		if (source == null)
			throw new ArgumentNullException ("source");
		if (selector == null)
			throw new ArgumentNullException ("selector");

		TR total = default (TR);
		int counter = 0;
		foreach (var element in source) {
			total = selector (total, element);
			++counter;
		}

		if (counter == 0)
			throw new InvalidOperationException ();

		return total;
	}

}

class Repro
{

	public static int Main ()
	{
		var sum = new [] { "1", "2", "3", "4", "5", "6", "7" }.Sum ((s) => int.Parse (s));
		if (sum != 28)
			return 4;

		Console.WriteLine (sum);
		return 0;
	}
}
