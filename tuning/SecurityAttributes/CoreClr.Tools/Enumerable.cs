using System;
using System.Collections.Generic;

namespace CoreClr.Tools
{
	public static class Enumerable
	{
		// http://blogs.msdn.com/ericlippert/archive/2009/05/07/zip-me-up.aspx
		public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>
			(this IEnumerable<TFirst> first,
			 IEnumerable<TSecond> second,
			 Func<TFirst, TSecond, TResult> resultSelector)
		{
			if (first == null) throw new ArgumentNullException("first");
			if (second == null) throw new ArgumentNullException("second");
			if (resultSelector == null) throw new ArgumentNullException("resultSelector");
			return ZipIterator(first, second, resultSelector);
		}

		private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>
			(IEnumerable<TFirst> first,
			 IEnumerable<TSecond> second,
			 Func<TFirst, TSecond, TResult> resultSelector)
		{
			using (IEnumerator<TFirst> e1 = first.GetEnumerator())
			using (IEnumerator<TSecond> e2 = second.GetEnumerator())
				while (e1.MoveNext() && e2.MoveNext())
					yield return resultSelector(e1.Current, e2.Current);
		}
	}
}