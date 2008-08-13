using System;
using System.Collections.Generic;

namespace Mono.Rocks
{
	public static class KeyValuePair
	{
		public static KeyValuePair<TKey, TValue>? Just<TKey, TValue> (TKey key, TValue value)
		{
			return new KeyValuePair<TKey, TValue> (key, value);
		}
	}

	public static class Sequence
	{
		public static IEnumerable<TResult> Unfoldr<TSource, TResult> (TSource value, Func<TSource, KeyValuePair<TResult, TSource>?> func)
		{
			return CreateUnfoldrIterator (value, func);
		}

		private static IEnumerable<TResult> CreateUnfoldrIterator<TSource, TResult> (TSource value, Func<TSource, KeyValuePair<TResult, TSource>?> func)
		{
			KeyValuePair<TResult, TSource>? r;
			while ((r = func (value)).HasValue) {
				KeyValuePair<TResult, TSource> v = r ?? new KeyValuePair<TResult, TSource> ();
				yield return v.Key;
				value = v.Value;
			}
		}
	}

	class Test
	{
		public static int Main ()
		{
			IEnumerable<int> x = Sequence.Unfoldr (10, b => b == 0
				? null
				: KeyValuePair.Just (b, b - 1));

			int i = 10;
			foreach (int e in x) {
				Console.WriteLine (e);
				if (i-- != e)
					return 1;
			}
			
			return 0;
		}
	}
}
