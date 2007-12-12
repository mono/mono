//
// Queryable.cs
//
// Authors:
//  Marek Safar (marek.safar@gmail.com)
//  Alejandro Serrano "Serras" (trupill@yahoo.es)
//  Jb Evain (jbevain@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq {

	public static class Queryable {

		static MethodInfo MakeGeneric (MethodBase method, params Type [] parameters)
		{
			return ((MethodInfo) method).MakeGenericMethod (parameters);
		}

		static Expression StaticCall (MethodInfo method, params Expression [] expressions)
		{
			return Expression.Call (null, method, expressions);
		}

		static TRet Execute<TRet, TSource> (this IQueryable<TSource> source, MethodBase current)
		{
			return source.Provider.Execute<TRet> (
				StaticCall (
					MakeGeneric (current, typeof (TSource)),
					source.Expression));
		}

		public static int Count<TSource> (this IQueryable<TSource> source)
		{
			Check.Source (source);

			return source.Execute<int, TSource> (MethodBase.GetCurrentMethod ());
		}

		public static int Count<TSource> (this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.Provider.Execute<int> (
				StaticCall (
					MakeGeneric (MethodBase.GetCurrentMethod (), typeof (TSource)),
					source.Expression,
					Expression.Quote (predicate)));
		}

		public static long LongCount<TSource> (this IQueryable<TSource> source)
		{
			Check.Source (source);

			return source.Execute<long, TSource> (MethodBase.GetCurrentMethod ());
		}

		public static long LongCount<TSource> (this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.Provider.Execute<long> (
				StaticCall (
					MakeGeneric (MethodBase.GetCurrentMethod (), typeof (TSource)),
					source.Expression,
					Expression.Quote (predicate)));
		}

		public static int Sum (this IQueryable<int> source)
		{
			Check.Source (source);

			return source.Provider.Execute<int> (
				StaticCall (
					(MethodInfo) MethodBase.GetCurrentMethod (),
					source.Expression));
		}
		
		public static int Sum<TSource> (this IQueryable<TSource> source, Expression<Func<TSource, int>> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static int? Sum (this IQueryable<int?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static int? Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int?> selector)
		{
			throw new NotImplementedException ();
		}

		public static long Sum (this IQueryable<long> source)
		{
			throw new NotImplementedException ();
		}
		
		public static long Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, long> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static long? Sum (this IQueryable<long?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static long? Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, long?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double Sum (this IQueryable<double> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, double> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Sum (this IQueryable<double?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, double?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Sum (this IQueryable<decimal> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, decimal> selector)
		{
			throw new NotImplementedException ();
		}

		public static decimal? Sum (this IQueryable<decimal?> source)
		{
			throw new NotImplementedException ();
		}

		public static decimal? Sum<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, decimal?> selector)
		{
			throw new NotImplementedException ();
		}

		public static int Min (this IQueryable<int> source)
		{
			throw new NotImplementedException ();
		}
		
		public static int? Min (this IQueryable<int?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static long Min (this IQueryable<long> source)
		{
			throw new NotImplementedException ();
		}
		
		public static long? Min (this IQueryable<long?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double Min (this IQueryable<double> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Min (this IQueryable<double?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Min (this IQueryable<decimal> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal? Min (this IQueryable<decimal?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Min<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static int Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static int? Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static long Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, long> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static long? Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, long?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, double> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, double?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, decimal> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal? Min<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, decimal?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static TResult Min<TSource, TResult> (
			this IQueryable<TSource> source,
			Func<TSource, TResult> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static int Max (this IQueryable<int> source)
		{
			throw new NotImplementedException ();
		}
		
		public static int? Max (this IQueryable<int?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static long Max (this IQueryable<long> source)
		{
			throw new NotImplementedException ();
		}
		
		public static long? Max (this IQueryable<long?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double Max (
			this IQueryable<double> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Max (
			this IQueryable<double?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Max (
			this IQueryable<decimal> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal? Max (
			this IQueryable<decimal?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Max<TSource> (
			this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static int Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static int? Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static long Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, long> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static long? Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, long?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, double> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, double?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, decimal> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal? Max<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, decimal?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static TResult Max<TSource, TResult> (
			this IQueryable<TSource> source,
			Func<TSource, TResult> selector)
		{
			throw new NotImplementedException ();
		}
				
		public static double Average (this IQueryable<int> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Average (this IQueryable<int?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double Average (this IQueryable<long> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Average (this IQueryable<long?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double Average (this IQueryable<double> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Average (this IQueryable<double?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Average (this IQueryable<decimal> source)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal? Average (this IQueryable<decimal?> source)
		{
			throw new NotImplementedException ();
		}
		
		public static double Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, int> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, int?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, long> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, long?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, double> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static double? Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, double?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, decimal> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static decimal? Average<TSource> (this IQueryable<TSource> source,
			Func<TSource, decimal?> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Aggregate<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, TSource, TSource> func)
		{
			throw new NotImplementedException ();
		}
		
		public static U Aggregate<TSource, U> (
			this IQueryable<TSource> source,
			U seed,
			Func<U, TSource, U> func)
		{
			throw new NotImplementedException ();
		}

		public static IEnumerable<TSource> Concat<TSource> (
			this IQueryable<TSource> first,
			IEnumerable<TSource> second)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TResult> OfType<TResult> (this IQueryable source)
		{
			throw new NotImplementedException ();
		}		

		public static IEnumerable<TResult> Cast<TResult> (this IQueryable source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource First<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource First<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource FirstOrDefault<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource FirstOrDefault<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Last<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Last<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource LastOrDefault<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource LastOrDefault<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Single<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource Single<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource SingleOrDefault<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource SingleOrDefault<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource ElementAt<TSource> (
			this IQueryable<TSource> source,
			int index)
		{
			throw new NotImplementedException ();
		}
		
		public static TSource ElementAtOrDefault<TSource> (
			this IQueryable<TSource> source,
			int index)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> DefaultIfEmpty<TSource> (
			this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> DefaultIfEmpty<TSource> (
			this IQueryable<TSource> source,
			TSource defaultValue)
		{
			throw new NotImplementedException ();
		}

		public static IEnumerable<TSource> Repeat<TSource> (this TSource element, int count)
		{
			throw new NotImplementedException ();
		}
		
		private static List<TSource> ContainsGroup<K, TSource>(
			Dictionary<K, List<TSource>> items, K key, IEqualityComparer<K> comparer)
		{
			throw new NotImplementedException ();
		}
		
		public static IQueryable<IGrouping<K, TSource>> GroupBy<TSource, K> (
			this IQueryable<TSource> source,
			Func<TSource, K> keySelector)
		{
			throw new NotImplementedException ();
		}

		public static IQueryable<IGrouping<K, E>> GroupBy<TSource, K, E> (
			this IQueryable<TSource> source,
			Func<TSource, K> keySelector,
			Func<TSource, E> elementSelector)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<IGrouping<K, E>> GroupBy<TSource, K, E> (
			this IQueryable<TSource> source,
			Func<TSource, K> keySelector,
			Func<TSource, E> elementSelector,
			IEqualityComparer<K> comparer)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> OrderBy<TSource, TKey> (
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> OrderBy<TSource, TKey> (
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey> (
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey> (
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}

		public static IOrderedQueryable<TSource> ThenBy<TSource, TKey> (
			this IOrderedQueryable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> ThenBy<TSource, TKey> (
			this IOrderedQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey> (
			this IOrderedQueryable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}
		
		public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey> (
			this IOrderedQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}

		public static IQueryable<TSource> Reverse<TSource> (
			this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> Take<TSource> (
			this IQueryable<TSource> source,
			int count)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> Skip<TSource> (
			this IQueryable<TSource> source,
			int count)
		{
			throw new NotImplementedException ();
		}

		public static IEnumerable<TSource> TakeWhile<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> TakeWhile<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> SkipWhile<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> SkipWhile<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TResult> Select<TSource, TResult> (
			this IQueryable<TSource> source,
			Func<TSource, TResult> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TResult> Select<TSource, TResult> (
			this IQueryable<TSource> source,
			Func<TSource, int, TResult> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TResult> SelectMany<TSource, TResult> (
			this IQueryable<TSource> source,
			Func<TSource, IQueryable<TResult>> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TResult> SelectMany<TSource, TResult> (
			this IQueryable<TSource> source,
			Func<TSource, int, IQueryable<TResult>> selector)
		{
			throw new NotImplementedException ();
		}
		
		public static bool Any<TSource> (this IQueryable<TSource> source)
		{
			throw new NotImplementedException ();
		}
		
		public static bool Any<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static bool All<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static bool Contains<TSource> (this IQueryable<TSource> source, TSource value)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> Where<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<TSource> Where<TSource> (
			this IQueryable<TSource> source,
			Func<TSource, int, bool> predicate)
		{
			throw new NotImplementedException ();
		}
		
		public static IEnumerable<V> Join<TSource, U, K, V> (
			this IQueryable<TSource> outer,
			IQueryable<U> inner,
			Func<TSource, K> outerKeySelector,
			Func<U, K> innerKeySelector,
			Func<TSource, U, V> resultSelector)
		{			
			throw new NotImplementedException ();
		}
	}
}
