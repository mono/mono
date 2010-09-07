//
// ParallelEnumerable.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Parallel;
using System.Linq.Parallel.QueryNodes;

namespace System.Linq
{
	public static class ParallelEnumerable
	{
		#region Range & Repeat
		public static ParallelQuery<int> Range (int start, int count)
		{
			if (int.MaxValue - start < count)
				throw new ArgumentOutOfRangeException ("count", "start + count - 1 is larger than Int32.MaxValue");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "count is less than 0");

			return (new RangeList (start, count)).AsParallel ();
		}

		public static ParallelQuery<TResult> Repeat<TResult> (TResult obj, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "count is less than 0");

			return (new RepeatList<TResult> (obj, count)).AsParallel ();
		}
		#endregion

		#region Empty
		public static ParallelQuery<TResult> Empty<TResult> ()
		{
			return Repeat<TResult> (default (TResult), 0);
		}
		#endregion

		#region AsParallel
		public static ParallelQuery<TSource> AsParallel<TSource> (this IEnumerable<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new QueryStartNode<TSource> (source));
		}

		public static ParallelQuery<TSource> AsParallel<TSource> (this Partitioner<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new QueryStartNode<TSource> (source));
		}

		public static ParallelQuery AsParallel (this IEnumerable source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<object> (new QueryStartNode<object> (source.Cast<object> ()));
		}

		public static IEnumerable<TSource> AsEnumerable<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.AsSequential ();
		}

		public static IEnumerable<TSource> AsSequential<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Node.GetSequential ();
		}
		#endregion

		#region AsOrdered / AsUnordered
		public static ParallelQuery<TSource> AsOrdered<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new QueryAsOrderedNode<TSource> (source.Node));
		}

		public static ParallelQuery<TSource> AsUnordered<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new QueryAsUnorderedNode<TSource> (source.Node));
		}

		public static ParallelQuery AsOrdered (this ParallelQuery source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.TypedQuery.AsOrdered ();
		}
		#endregion

		#region With*
		public static ParallelQuery<TSource> WithExecutionMode<TSource> (this ParallelQuery<TSource> source,
		                                                                 ParallelExecutionMode executionMode)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new ParallelExecutionModeNode<TSource> (executionMode, source.Node));
		}

		public static ParallelQuery<TSource> WithCancellation<TSource> (this ParallelQuery<TSource> source,
		                                                                CancellationToken cancellationToken)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new CancellationTokenNode<TSource> (cancellationToken, source.Node));
		}

		public static ParallelQuery<TSource> WithMergeOptions<TSource> (this ParallelQuery<TSource> source,
		                                                                ParallelMergeOptions mergeOptions)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new ParallelMergeOptionsNode<TSource> (mergeOptions, source.Node));
		}

		public static ParallelQuery<TSource> WithDegreeOfParallelism<TSource> (this ParallelQuery<TSource> source,
		                                                                       int degreeParallelism)
		{
			if (degreeParallelism < 1 || degreeParallelism > 63)
				throw new ArgumentException ("degreeOfParallelism is less than 1 or greater than 63", "degreeParallelism");
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new DegreeOfParallelismNode<TSource> (degreeParallelism, source.Node));
		}

		internal static ParallelQuery<TSource> WithImplementerToken<TSource> (this ParallelQuery<TSource> source,
		                                                                      CancellationTokenSource token)
		{
			return new ParallelQuery<TSource> (new ImplementerTokenNode<TSource> (token, source.Node));
		}
		#endregion

		#region Select
		public static ParallelQuery<TResult> Select<TSource, TResult> (this ParallelQuery<TSource> source, Func<TSource, TResult> selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (selector == null)
				throw new ArgumentNullException ("selector");

			return new ParallelQuery<TResult> (new QuerySelectNode<TResult, TSource> (source.Node, selector));
		}

		public static ParallelQuery<TResult> Select<TSource, TResult> (this ParallelQuery<TSource> source, Func<TSource, int, TResult> selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (selector == null)
				throw new ArgumentNullException ("selector");

			return new ParallelQuery<TResult> (new QuerySelectNode<TResult, TSource> (source.Node, selector));
		}
		#endregion
		
		#region SelectMany
		public static ParallelQuery<TResult> SelectMany<TSource, TResult> (this ParallelQuery<TSource> source,
		                                                                   Func<TSource, IEnumerable<TResult>> selector)
		{
			return source.SelectMany (selector, (s, e) => e);
		}

		public static ParallelQuery<TResult> SelectMany<TSource, TResult> (this ParallelQuery<TSource> source,
		                                                                   Func<TSource, int, IEnumerable<TResult>> selector)
		{
			return source.SelectMany (selector, (s, e) => e);
		}
		
		public static ParallelQuery<TResult> SelectMany<TSource, TCollection, TResult> (this ParallelQuery<TSource> source,
		                                                                                Func<TSource, IEnumerable<TCollection>> collectionSelector,
		                                                                                Func<TSource, TCollection, TResult> resultSelector)
		{
			return new ParallelQuery<TResult> (new QuerySelectManyNode<TSource, TCollection, TResult> (source.Node,
			                                                                                           collectionSelector,
			                                                                                           resultSelector));
		}
		
		public static ParallelQuery<TResult> SelectMany<TSource, TCollection, TResult> (this ParallelQuery<TSource> source,
		                                                                                Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
		                                                                                Func<TSource, TCollection, TResult> resultSelector)
		{
			return new ParallelQuery<TResult> (new QuerySelectManyNode<TSource, TCollection, TResult> (source.Node,
			                                                                                           collectionSelector,
			                                                                                           resultSelector));
		}
		#endregion

		#region Where
		public static ParallelQuery<TSource> Where<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return new ParallelQuery<TSource> (new QueryWhereNode<TSource> (source.Node, predicate));
		}

		public static ParallelQuery<TSource> Where<TSource> (this ParallelQuery<TSource> source, Func<TSource, int, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return new ParallelQuery<TSource> (new QueryWhereNode<TSource> (source.Node, predicate));
		}
		#endregion

		#region Aggregate
		public static TSource Aggregate<TSource> (this ParallelQuery<TSource> source, Func<TSource, TSource, TSource> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Aggregate<TSource, TSource, TSource> ((Func<TSource>)null,
			                                                    func,
			                                                    func,
			                                                    (e) => e);
		}

		public static TAccumulate Aggregate<TSource, TAccumulate> (this ParallelQuery<TSource> source,
		                                                             TAccumulate seed,
		                                                             Func<TAccumulate, TSource, TAccumulate> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Aggregate (seed, func, (e) => e);
		}

		public static TResult Aggregate<TSource, TAccumulate, TResult> (this ParallelQuery<TSource> source,
		                                                                  TAccumulate seed,
		                                                                  Func<TAccumulate, TSource, TAccumulate> func,
		                                                                  Func<TAccumulate, TResult> resultSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			TAccumulate accumulator = seed;

			foreach (TSource value in source)
				accumulator = func (accumulator, value);

			return resultSelector (accumulator);
		}

		public static TResult Aggregate<TSource, TAccumulate, TResult> (this ParallelQuery<TSource> source,
		                                                                  TAccumulate seed,
		                                                                  Func<TAccumulate, TSource, TAccumulate> updateAccumulatorFunc,
		                                                                  Func<TAccumulate, TAccumulate, TAccumulate> combineAccumulatorsFunc,
		                                                                  Func<TAccumulate, TResult> resultSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (updateAccumulatorFunc == null)
				throw new ArgumentNullException ("updateAccumulatorFunc");
			if (combineAccumulatorsFunc == null)
				throw new ArgumentNullException ("combineAccumulatorsFunc");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			return source.Aggregate (() => seed, updateAccumulatorFunc, combineAccumulatorsFunc, resultSelector);
		}

		public static TResult Aggregate<TSource, TAccumulate, TResult> (this ParallelQuery<TSource> source,
		                                                                  Func<TAccumulate> seedFunc,
		                                                                  Func<TAccumulate, TSource, TAccumulate> updateAccumulatorFunc,
		                                                                  Func<TAccumulate, TAccumulate, TAccumulate> combineAccumulatorsFunc,
		                                                                  Func<TAccumulate, TResult> resultSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (seedFunc == null)
				throw new ArgumentNullException ("seedFunc");
			if (updateAccumulatorFunc == null)
				throw new ArgumentNullException ("updateAccumulatorFunc");
			if (combineAccumulatorsFunc == null)
				throw new ArgumentNullException ("combineAccumulatorsFunc");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			TAccumulate accumulator = default (TAccumulate);

			ParallelExecuter.ProcessAndAggregate<TSource, TAccumulate> (source.Node, seedFunc, updateAccumulatorFunc, (list) => {
				accumulator = list [0];
				for (int i = 1; i < list.Count; i++)
					accumulator = combineAccumulatorsFunc (accumulator, list[i]);
			});

			return resultSelector (accumulator);;
		}
		#endregion

		#region ForAll
		public static void ForAll<TSource> (this ParallelQuery<TSource> source, Action<TSource> action)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (action == null)
				throw new ArgumentNullException ("action");

			ParallelExecuter.ProcessAndBlock (source.Node, (e, c) => action (e));
		}
		#endregion

		#region OrderBy
		public static OrderedParallelQuery<TSource> OrderByDescending<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                              Func<TSource, TKey> keySelector,
		                                                                              IComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = Comparer<TKey>.Default;

			Comparison<TSource> comparison = (e1, e2) => -comparer.Compare (keySelector (e1), keySelector (e2));

			return new OrderedParallelQuery<TSource> (new QueryOrderByNode<TSource> (source.Node, comparison));
		}

		public static OrderedParallelQuery<TSource> OrderByDescending<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                              Func<TSource, TKey> keySelector)
		{
			return OrderByDescending (source, keySelector, Comparer<TKey>.Default);
		}

		public static OrderedParallelQuery<TSource> OrderBy<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                    Func<TSource, TKey> keySelector)
		{
			return OrderBy (source, keySelector, Comparer<TKey>.Default);
		}

		public static OrderedParallelQuery<TSource> OrderBy<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                    Func<TSource, TKey> keySelector,
		                                                                    IComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = Comparer<TKey>.Default;

			Comparison<TSource> comparison = (e1, e2) => comparer.Compare (keySelector (e1), keySelector (e2));

			return new OrderedParallelQuery<TSource> (new QueryOrderByNode<TSource> (source.Node, comparison));
		}
		#endregion

		#region ThenBy
		public static OrderedParallelQuery<TSource> ThenBy<TSource, TKey> (this OrderedParallelQuery<TSource> source,
		                                                                   Func<TSource, TKey> keySelector)
		{
			return ThenBy (source, keySelector, Comparer<TKey>.Default);
		}

		public static OrderedParallelQuery<TSource> ThenBy<TSource, TKey> (this OrderedParallelQuery<TSource> source,
		                                                                   Func<TSource, TKey> keySelector,
		                                                                   IComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = Comparer<TKey>.Default;

			Comparison<TSource> comparison = (e1, e2) => comparer.Compare (keySelector (e1), keySelector (e2));

			return new OrderedParallelQuery<TSource> (new QueryOrderByNode<TSource> (source.Node, comparison));
		}

		public static OrderedParallelQuery<TSource> ThenByDescending<TSource, TKey> (this OrderedParallelQuery<TSource> source,
		                                                                             Func<TSource, TKey> keySelector)
		{
			return ThenByDescending (source, keySelector, Comparer<TKey>.Default);
		}

		public static OrderedParallelQuery<TSource> ThenByDescending<TSource, TKey> (this OrderedParallelQuery<TSource> source,
		                                                                             Func<TSource, TKey> keySelector,
		                                                                             IComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = Comparer<TKey>.Default;

			Comparison<TSource> comparison = (e1, e2) => -comparer.Compare (keySelector (e1), keySelector (e2));

			return new OrderedParallelQuery<TSource> (new QueryOrderByNode<TSource> (source.Node, comparison));
		}
		#endregion

		#region All
		public static bool All<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			CancellationTokenSource src = new CancellationTokenSource ();
			ParallelQuery<TSource> innerQuery = source.WithImplementerToken (src);

			bool result = true;
			try {
				innerQuery.ForAll ((e) => {
						if (!predicate (e)) {
							result = false;
							src.Cancel ();
						}
					});
			} catch (OperationCanceledException e) {
				if (e.CancellationToken != src.Token)
					throw e;
			}

			return result;
		}
		#endregion

		#region Any
		public static bool Any<TSource> (this ParallelQuery<TSource> source)
		{
			return Any<TSource> (source, (_) => true);
		}

		public static bool Any<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return !source.All ((e) => !predicate (e));
		}
		#endregion

		#region Contains
		public static bool Contains<TSource> (this ParallelQuery<TSource> source, TSource value)
		{
			return Contains<TSource> (source, value, EqualityComparer<TSource>.Default);
		}

		public static bool Contains<TSource> (this ParallelQuery<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			return Any<TSource> (source, (e) => comparer.Equals (value));
		}
		#endregion

		#region SequenceEqual
		public static bool SequenceEqual<TSource> (this ParallelQuery<TSource> first,
		                                           ParallelQuery<TSource> second)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");

			return first.SequenceEqual (second, EqualityComparer<TSource>.Default);
		}

		public static bool SequenceEqual<TSource> (this ParallelQuery<TSource> first,
		                                           ParallelQuery<TSource> second,
		                                           IEqualityComparer<TSource> comparer)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			CancellationTokenSource source = new CancellationTokenSource ();
			ParallelQuery<bool> innerQuery
				= first.Zip (second, (e1, e2) => comparer.Equals (e1, e2)).Where ((e) => !e).WithImplementerToken (source);

			bool result = true;

			try {
				innerQuery.ForAll ((value) => {
						result = false;
						source.Cancel ();
					});
			} catch (OperationCanceledException e) {
				if (e.CancellationToken != source.Token)
					throw e;
			}

			return result;
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static bool SequenceEqual<TSource> (this ParallelQuery<TSource> first, IEnumerable<TSource> second)
		{
			throw new NotSupportedException ();
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static bool SequenceEqual<TSource> (this ParallelQuery<TSource> first,
		                                              IEnumerable<TSource> second,
		                                              IEqualityComparer<TSource> comparer)
		{
			throw new NotSupportedException ();
		}

		#endregion
		
		#region GroupBy
		public static ParallelQuery<IGrouping<TKey, TSource>> GroupBy<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                              Func<TSource, TKey> keySelector)
		{
			return source.GroupBy (keySelector, EqualityComparer<TKey>.Default);
		}
		
		public static ParallelQuery<IGrouping<TKey, TSource>> GroupBy<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                              Func<TSource, TKey> keySelector,
		                                                                              IEqualityComparer<TKey> comparer)
		{
			return source.GroupBy (keySelector, (e) => e, comparer);
		}
		
		public static ParallelQuery<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement> (this ParallelQuery<TSource> source,
		                                                                                         Func<TSource, TKey> keySelector,
		                                                                                         Func<TSource, TElement> elementSelector)
		{
			return source.GroupBy (keySelector, elementSelector, EqualityComparer<TKey>.Default);
		}
		
		public static ParallelQuery<TResult> GroupBy<TSource, TKey, TResult> (this ParallelQuery<TSource> source,
		                                                                      Func<TSource, TKey> keySelector,
		                                                                      Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
		{
			return source.GroupBy (keySelector)
				.Select ((g) => resultSelector (g.Key, (IEnumerable<TSource>)g));
		}
		
		public static ParallelQuery<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement> (this ParallelQuery<TSource> source,
		                                                                                         Func<TSource, TKey> keySelector,
		                                                                                         Func<TSource, TElement> elementSelector,
		                                                                                         IEqualityComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (elementSelector == null)
				throw new ArgumentNullException ("elementSelector");
			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			return new ParallelQuery<IGrouping<TKey, TElement>> (new QueryGroupByNode<TSource, TKey, TElement> (source.Node, keySelector, elementSelector, comparer));
		}
		
		public static ParallelQuery<TResult> GroupBy<TSource, TKey, TElement, TResult> (this ParallelQuery<TSource> source,
		                                                                                Func<TSource, TKey> keySelector,
		                                                                                Func<TSource, TElement> elementSelector,
		                                                                                Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
		{
			return source.GroupBy (keySelector, elementSelector)
				.Select ((g) => resultSelector (g.Key, (IEnumerable<TElement>)g));
		}
		
		public static ParallelQuery<TResult> GroupBy<TSource, TKey, TResult> (this ParallelQuery<TSource> source,
		                                                                      Func<TSource, TKey> keySelector,
		                                                                      Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
		                                                                      IEqualityComparer<TKey> comparer)
		{
			return source.GroupBy (keySelector, comparer)
				.Select ((g) => resultSelector (g.Key, (IEnumerable<TSource>)g));
		}
		
		public static ParallelQuery<TResult> GroupBy<TSource, TKey, TElement, TResult> (this ParallelQuery<TSource> source,
		                                                                                Func<TSource, TKey> keySelector,
		                                                                                Func<TSource, TElement> elementSelector,
		                                                                                Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
		                                                                                IEqualityComparer<TKey> comparer)
		{
			return source.GroupBy (keySelector, elementSelector, comparer)
				.Select ((g) => resultSelector (g.Key, (IEnumerable<TElement>)g));
		}
		#endregion

		#region GroupJoin
		public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                               ParallelQuery<TInner> inner,
		                                                                               Func<TOuter, TKey> outerKeySelector,
		                                                                               Func<TInner, TKey> innerKeySelector,
		                                                                               Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
		{
			return outer.GroupJoin (inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
		}
		
		public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                               ParallelQuery<TInner> inner,
		                                                                               Func<TOuter, TKey> outerKeySelector,
		                                                                               Func<TInner, TKey> innerKeySelector,
		                                                                               Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
		                                                                               IEqualityComparer<TKey> comparer)
		{
			return outer.Join (inner.GroupBy (innerKeySelector, (e) => e), outerKeySelector, (e) => e.Key, (e1, e2) => resultSelector (e1, e2), comparer);
		}
		
		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                               IEnumerable<TInner> inner,
		                                                                               Func<TOuter, TKey> outerKeySelector,
		                                                                               Func<TInner, TKey> innerKeySelector,
		                                                                               Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
		{
			throw new NotSupportedException ();
		}
		
		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TResult> GroupJoin<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                               IEnumerable<TInner> inner,
		                                                                               Func<TOuter, TKey> outerKeySelector,
		                                                                               Func<TInner, TKey> innerKeySelector,
		                                                                               Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
		                                                                               IEqualityComparer<TKey> comparer)
		{
			throw new NotSupportedException ();
		}
		#endregion

		#region ElementAt
		public static TSource ElementAt<TSource> (this ParallelQuery<TSource> source, int index)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index == 0) {
				try {
					return source.First ();
				} catch (InvalidOperationException) {
					throw new ArgumentOutOfRangeException ("index");
				}
			}

			TSource result = default (TSource);

			ParallelQuery<TSource> innerQuery = source.Where ((e, i) => i == index);

			try {
				result = innerQuery.First ();
			} catch (InvalidOperationException) {
				throw new ArgumentOutOfRangeException ("index");
			}

			return result;
		}

		public static TSource ElementAtOrDefault<TSource> (this ParallelQuery<TSource> source, int index)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			try {
				return source.ElementAt (index);
			} catch (ArgumentOutOfRangeException) {
				return default (TSource);
			}
		}
		#endregion

		#region Intersect
		public static ParallelQuery<TSource> Intersect<TSource> (this ParallelQuery<TSource> first,
		                                                         ParallelQuery<TSource> second)
		{
			return Intersect<TSource> (first, second, EqualityComparer<TSource>.Default);
		}

		public static ParallelQuery<TSource> Intersect<TSource> (this ParallelQuery<TSource> first,
		                                                         ParallelQuery<TSource> second,
		                                                         IEqualityComparer<TSource> comparer)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			return new ParallelQuery<TSource> (new QuerySetNode<TSource> (SetInclusionDefaults.Intersect, comparer, first.Node, second.Node));
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Intersect<TSource> (this ParallelQuery<TSource> first, IEnumerable<TSource> second)
		{
			throw new NotSupportedException ();
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Intersect<TSource> (this ParallelQuery<TSource> first,
		                                                         IEnumerable<TSource> second,
		                                                         IEqualityComparer<TSource> comparer)
		{
			throw new NotSupportedException ();
		}
		#endregion

		#region Join
		public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                          ParallelQuery<TInner> inner,
		                                                                          Func<TOuter, TKey> outerKeySelector,
		                                                                          Func<TInner, TKey> innerKeySelector,
		                                                                          Func<TOuter, TInner, TResult> resultSelector)
		{
			return outer.Join (inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default);
		}
		
		public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                          ParallelQuery<TInner> inner,
		                                                                          Func<TOuter, TKey> outerKeySelector,
		                                                                          Func<TInner, TKey> innerKeySelector,
		                                                                          Func<TOuter, TInner, TResult> resultSelector,
		                                                                          IEqualityComparer<TKey> comparer)
		{
			return new ParallelQuery<TResult> (new QueryJoinNode<TOuter, TInner, TKey, TResult> (outer.Node, inner.Node, outerKeySelector, innerKeySelector, resultSelector, comparer));
		}
		
		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                          IEnumerable<TInner> inner,
		                                                                          Func<TOuter, TKey> outerKeySelector,
		                                                                          Func<TInner, TKey> innerKeySelector,
		                                                                          Func<TOuter, TInner, TResult> resultSelector)
		{
			throw new NotSupportedException ();
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TResult> Join<TOuter, TInner, TKey, TResult> (this ParallelQuery<TOuter> outer,
		                                                                          IEnumerable<TInner> inner,
		                                                                          Func<TOuter, TKey> outerKeySelector,
		                                                                          Func<TInner, TKey> innerKeySelector,
		                                                                          Func<TOuter, TInner, TResult> resultSelector,
		                                                                          IEqualityComparer<TKey> comparer)
		{
			throw new NotSupportedException ();
		}
		#endregion

		#region Except
		public static ParallelQuery<TSource> Except<TSource> (this ParallelQuery<TSource> first,
		                                                      ParallelQuery<TSource> second)
		{
			return Except<TSource> (first, second, EqualityComparer<TSource>.Default);
		}

		public static ParallelQuery<TSource> Except<TSource> (this ParallelQuery<TSource> first,
		                                                      ParallelQuery<TSource> second,
		                                                      IEqualityComparer<TSource> comparer)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			return new ParallelQuery<TSource> (new QuerySetNode<TSource> (SetInclusionDefaults.Except,
			                                                              comparer, first.Node, second.Node));
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Except<TSource> (this ParallelQuery<TSource> first,
		                                                      IEnumerable<TSource> second)
		{
			throw new NotSupportedException ();
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Except<TSource> (this ParallelQuery<TSource> first,
		                                                      IEnumerable<TSource> second,
		                                                      IEqualityComparer<TSource> comparer)
		{
			throw new NotSupportedException ();
		}
		#endregion

		#region Distinct
		public static ParallelQuery<TSource> Distinct<TSource> (this ParallelQuery<TSource> source)
		{
			return Distinct<TSource> (source, EqualityComparer<TSource>.Default);
		}

		public static ParallelQuery<TSource> Distinct<TSource> (this ParallelQuery<TSource> source, IEqualityComparer<TSource> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			return new ParallelQuery<TSource> (new QuerySetNode<TSource> (SetInclusionDefaults.Distinct, comparer,
			                                                              source.Node, null));
		}
		#endregion

		#region Union
		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Union<TSource> (this ParallelQuery<TSource> first,
		                                                     IEnumerable<TSource> second)
		{
			throw new NotSupportedException ();
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Union<TSource>(this ParallelQuery<TSource> first,
		                                                    IEnumerable<TSource> second,
		                                                    IEqualityComparer<TSource> comparer)
		{
			throw new NotSupportedException ();
		}

		public static ParallelQuery<TSource> Union<TSource> (this ParallelQuery<TSource> first,
		                                                     ParallelQuery<TSource> second)
		{
			return first.Union (second, EqualityComparer<TSource>.Default);
		}

		public static ParallelQuery<TSource> Union<TSource> (this ParallelQuery<TSource> first,
		                                                     ParallelQuery<TSource> second,
		                                                     IEqualityComparer<TSource> comparer)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			return new ParallelQuery<TSource> (new QuerySetNode<TSource> (SetInclusionDefaults.Union, comparer, first.Node, second.Node));
		}
		#endregion

		#region Take
		public static ParallelQuery<TSource> Take<TSource> (this ParallelQuery<TSource> source, int count)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new QueryHeadWorkerNode<TSource> (source.Node, count));
		}

		public static ParallelQuery<TSource> TakeWhile<TSource> (this ParallelQuery<TSource> source,
		                                                         Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return new ParallelQuery<TSource> (new QueryHeadWorkerNode<TSource> (source.Node, (e, _) => predicate (e), false));
		}

		public static ParallelQuery<TSource> TakeWhile<TSource> (this ParallelQuery<TSource> source,
		                                                         Func<TSource, int, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return new ParallelQuery<TSource> (new QueryHeadWorkerNode<TSource> (source.Node, predicate, true));
		}
		#endregion

		#region Skip
		public static ParallelQuery<TSource> Skip<TSource> (this ParallelQuery<TSource> source, int count)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Node.IsOrdered () ?
				source.Where ((e, i) => i >= count) :
				source.Where ((e) => count < 0 || Interlocked.Decrement (ref count) < 0);

		}

		public static ParallelQuery<TSource> SkipWhile<TSource> (this ParallelQuery<TSource> source,
		                                                         Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return source.Node.IsOrdered () ?
				source.SkipWhile ((e, i) => predicate (e)) :
				source.Where ((e) => !predicate (e));
		}

		public static ParallelQuery<TSource> SkipWhile<TSource> (this ParallelQuery<TSource> source,
		                                                         Func<TSource, int, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			int indexCache = int.MaxValue;

			return source.Where ((e, i) => i >= indexCache || (!predicate (e, i) && (indexCache = i) == i));
		}
		#endregion

		#region Single
		static TSource SingleInternal<TSource> (this ParallelQuery<TSource> source, params TSource[] init)
		{
			TSource result = default(TSource);
			bool hasValue = false;

			foreach (TSource element in source) {
				if (hasValue)
					throw new InvalidOperationException ("The input sequence contains more than one element.");

				result = element;
				hasValue = true;
			}

			if (!hasValue && init.Length != 0) {
				result = init[0];
				hasValue = true;
			}

			if (!hasValue)
				throw new InvalidOperationException ("The input sequence is empty.");

			return result;
		}

		public static TSource Single<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return SingleInternal<TSource> (source);
		}

		public static TSource Single<TSource> (this ParallelQuery<TSource> source,
		                                       Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return source.Where (predicate).Single ();
		}

		public static TSource SingleOrDefault<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return SingleInternal<TSource> (source, default (TSource));
		}

		public static TSource SingleOrDefault<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return source.Where (predicate).SingleOrDefault ();
		}
		#endregion

		#region Count
		public static int Count<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate<TSource, int, int> (() => 0,
			                                           (acc, e) => acc + 1,
			                                           (acc1, acc2) => acc1 + acc2,
			                                           (result) => result);
		}

		public static int Count<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return source.Where (predicate).Count ();
		}

		public static long LongCount<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate<TSource, long, long> (() => 0,
			                                              (acc, e) => acc + 1,
			                                              (acc1, acc2) => acc1 + acc2,
			                                              (result) => result);
		}

		public static long LongCount<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");

			return source.Where (predicate).LongCount ();
		}
		#endregion

		#region Average
		public static double Average (this ParallelQuery<int> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (() => new int[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}

		public static double Average (this ParallelQuery<long> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (() => new long[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}

		public static decimal Average (this ParallelQuery<decimal> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (() => new decimal[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / acc[1]);
		}

		public static double Average (this ParallelQuery<double> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (() => new double[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}

		public static float Average (this ParallelQuery<float> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (() => new float[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / acc[1]);
		}
		#endregion

		#region More Average
		public static double? Average (this ParallelQuery<int?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Average ();;
		}

		public static double? Average (this ParallelQuery<long?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Average ();
		}

		public static decimal? Average (this ParallelQuery<decimal?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Average ();
		}

		public static double? Average (this ParallelQuery<double?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Average ();
		}

		public static float? Average (this ParallelQuery<float?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Average ();
		}

		public static double Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, int> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static double Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, long> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static float Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, float> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static double Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, double> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static decimal Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static double? Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, int?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static double? Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, long?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static float? Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, float?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static double? Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, double?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}

		public static decimal? Average<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Average ();
		}
		#endregion

		#region Sum
		public static int Sum (this ParallelQuery<int> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}

		public static long Sum (this ParallelQuery<long> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate ((long)0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}

		public static float Sum (this ParallelQuery<float> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (0.0f, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}

		public static double Sum (this ParallelQuery<double> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate (0.0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}

		public static decimal Sum (this ParallelQuery<decimal> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Aggregate ((decimal)0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}

		public static int? Sum (this ParallelQuery<int?> source)
		{
			return source.Select ((e) => e.HasValue ? e.Value : 0).Sum ();
		}

		public static long? Sum (this ParallelQuery<long?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Sum ();
		}

		public static float? Sum (this ParallelQuery<float?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Sum ();
		}

		public static double? Sum (this ParallelQuery<double?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Sum ();
		}

		public static decimal? Sum (this ParallelQuery<decimal?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : 0).Sum ();
		}

		public static int Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, int> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static long Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, long> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static decimal Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static float Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, float> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static double Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, double> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static int? Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, int?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static long? Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, long?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static decimal? Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static float? Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, float?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}

		public static double? Sum<TSource> (this ParallelQuery<TSource> source, Func<TSource, double?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Sum ();
		}
		#endregion

		#region Min-Max
		static T BestOrder<T> (ParallelQuery<T> source, Func<T, T, bool> bestSelector, T seed)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			T best = seed;

			best = source.Aggregate (() => seed,
			                        (first, second) => (bestSelector(first, second)) ? first : second,
			                        (first, second) => (bestSelector(first, second)) ? first : second,
			                        (e) => e);
			return best;
		}

		public static int Min (this ParallelQuery<int> source)
		{
			return BestOrder (source, (first, second) => first < second, int.MaxValue);
		}

		public static long Min (this ParallelQuery<long> source)
		{
			return BestOrder (source, (first, second) => first < second, long.MaxValue);
		}

		public static float Min (this ParallelQuery<float> source)
		{
			return BestOrder (source, (first, second) => first < second, float.MaxValue);
		}

		public static double Min (this ParallelQuery<double> source)
		{
			return BestOrder (source, (first, second) => first < second, double.MaxValue);
		}

		public static decimal Min (this ParallelQuery<decimal> source)
		{
			return BestOrder (source, (first, second) => first < second, decimal.MaxValue);
		}

		public static TSource Min<TSource> (this ParallelQuery<TSource> source)
		{
			IComparer<TSource> comparer = Comparer<TSource>.Default;

			return BestOrder (source, (first, second) => comparer.Compare (first, second) < 0, default (TSource));
		}

		public static TResult Min<TSource, TResult> (this ParallelQuery<TSource> source, Func<TSource, TResult> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static int? Min (this ParallelQuery<int?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : int.MaxValue).Min ();
		}

		public static long? Min (this ParallelQuery<long?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : long.MaxValue).Min ();
		}

		public static float? Min (this ParallelQuery<float?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : float.MaxValue).Min ();
		}

		public static double? Min (this ParallelQuery<double?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : double.MaxValue).Min ();
		}

		public static decimal? Min (this ParallelQuery<decimal?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : decimal.MaxValue).Min ();
		}

		public static int Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, int> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static long Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, long> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static float Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, float> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static double Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, double> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static decimal Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static int? Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, int?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static long? Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, long?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static float? Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, float?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static double? Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, double?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static decimal? Min<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Min ();
		}

		public static int Max (this ParallelQuery<int> source)
		{
			return BestOrder (source, (first, second) => first > second, int.MinValue);
		}

		public static long Max(this ParallelQuery<long> source)
		{
			return BestOrder(source, (first, second) => first > second, long.MinValue);
		}

		public static float Max (this ParallelQuery<float> source)
		{
			return BestOrder(source, (first, second) => first > second, float.MinValue);
		}

		public static double Max (this ParallelQuery<double> source)
		{
			return BestOrder(source, (first, second) => first > second, double.MinValue);
		}

		public static decimal Max (this ParallelQuery<decimal> source)
		{
			return BestOrder(source, (first, second) => first > second, decimal.MinValue);
		}

		public static TSource Max<TSource> (this ParallelQuery<TSource> source)
		{
			IComparer<TSource> comparer = Comparer<TSource>.Default;

			return BestOrder (source, (first, second) => comparer.Compare (first, second) > 0, default (TSource));
		}

		public static TResult Max<TSource, TResult> (this ParallelQuery<TSource> source, Func<TSource, TResult> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static int? Max (this ParallelQuery<int?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : int.MinValue).Max ();
		}

		public static long? Max (this ParallelQuery<long?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : long.MinValue).Max ();
		}

		public static float? Max (this ParallelQuery<float?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : float.MinValue).Max ();
		}

		public static double? Max (this ParallelQuery<double?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : double.MinValue).Max ();
		}

		public static decimal? Max (this ParallelQuery<decimal?> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.Select ((e) => e.HasValue ? e.Value : decimal.MinValue).Max ();
		}

		public static int Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, int> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static long Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, long> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static float Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, float> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static double Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, double> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static decimal Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static int? Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, int?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static long? Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, long?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static float? Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, float?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static double? Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, double?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}

		public static decimal? Max<TSource> (this ParallelQuery<TSource> source, Func<TSource, decimal?> func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");

			return source.Select (func).Max ();
		}
		#endregion

		#region Cast / OfType
		public static ParallelQuery<TResult> Cast<TResult> (this ParallelQuery source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.TypedQuery.Select ((e) => (TResult)e);
		}

		public static ParallelQuery<TResult> OfType<TResult> (this ParallelQuery source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.TypedQuery.Where ((e) => e is TResult).Cast<TResult> ();
		}
		#endregion

		#region Reverse
		public static ParallelQuery<TSource> Reverse<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ParallelQuery<TSource> (new QueryReverseNode<TSource> (source));
		}
		#endregion

		#region ToArray - ToList - ToDictionary - ToLookup
		public static List<TSource> ToList<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (source.Node.IsOrdered ())
				return ToListOrdered (source);

			List<TSource> temp = source.Aggregate (() => new List<TSource>(50),
			                                       (list, e) => { list.Add (e); return list; },
			                                       (list, list2) => { list.AddRange (list2); return list; },
			                                       (list) => list);
			return temp;
		}

		internal static List<TSource> ToListOrdered<TSource> (this ParallelQuery<TSource> source)
		{
			List<TSource> result = new List<TSource> ();

			foreach (TSource element in source)
				result.Add (element);

			return result;
		}

		public static TSource[] ToArray<TSource> (this ParallelQuery<TSource> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (source.Node.IsOrdered ())
				return ToListOrdered (source).ToArray ();

			TSource[] result = null;

			Func<List<TSource>, TSource, List<TSource>> intermediate = (list, e) => {
				list.Add (e); return list;
			};

			Action<IList<List<TSource>>> final = (list) => {
				int count = 0;

				for (int i = 0; i < list.Count; i++)
				  count += list[i].Count;

				result = new TSource[count];
				int insertIndex = -1;

				for (int i = 0; i < list.Count; i++)
				  for (int j = 0; j < list[i].Count; j++)
				    result [++insertIndex] = list[i][j];
			};

			ParallelExecuter.ProcessAndAggregate<TSource, List<TSource>> (source.Node,
			                                                              () => new List<TSource> (),
			                                                              intermediate,
			                                                              final);

			return result;
		}

		public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                     Func<TSource, TKey> keySelector,
		                                                                     IEqualityComparer<TKey> comparer)
		{
			return ToDictionary<TSource, TKey, TSource> (source, keySelector, (e) => e, comparer);
		}

		public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                                     Func<TSource, TKey> keySelector)
		{
			return ToDictionary<TSource, TKey, TSource> (source, keySelector, (e) => e, EqualityComparer<TKey>.Default);
		}

		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement> (this ParallelQuery<TSource> source,
		                                                                                  Func<TSource, TKey> keySelector,
		                                                                                  Func<TSource, TElement> elementSelector)
		{
			return ToDictionary<TSource, TKey, TElement> (source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
		}

		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement> (this ParallelQuery<TSource> source,
		                                                                                  Func<TSource, TKey> keySelector,
		                                                                                  Func<TSource, TElement> elementSelector,
		                                                                                  IEqualityComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;
			if (elementSelector == null)
				throw new ArgumentNullException ("elementSelector");

			return source.Aggregate (() => new Dictionary<TKey, TElement> (comparer),
			                          (d, e) => { d.Add (keySelector (e), elementSelector (e)); return d; },
			                          (d1, d2) => { foreach (var couple in d2) d1.Add (couple.Key, couple.Value); return d1; },
			                          (d) => d);
		}

		public static ILookup<TKey, TSource> ToLookup<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                              Func<TSource, TKey> keySelector)
		{
			return ToLookup<TSource, TKey, TSource> (source, keySelector, (e) => e, EqualityComparer<TKey>.Default);
		}

		public static ILookup<TKey, TSource> ToLookup<TSource, TKey> (this ParallelQuery<TSource> source,
		                                                              Func<TSource, TKey> keySelector,
		                                                              IEqualityComparer<TKey> comparer)
		{
			return ToLookup<TSource, TKey, TSource> (source, keySelector, (e) => e, comparer);
		}

		public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement> (this ParallelQuery<TSource> source,
		                                                                         Func<TSource, TKey> keySelector,
		                                                                         Func<TSource, TElement> elementSelector)
		{
			return ToLookup<TSource, TKey, TElement> (source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
		}

		public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement> (this ParallelQuery<TSource> source,
		                                                                         Func<TSource, TKey> keySelector,
		                                                                         Func<TSource, TElement> elementSelector,
		                                                                         IEqualityComparer<TKey> comparer)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;
			if (elementSelector == null)
				throw new ArgumentNullException ("elementSelector");

			ConcurrentLookup<TKey, TElement> lookup = new ConcurrentLookup<TKey, TElement> (comparer);
			source.ForAll ((e) => lookup.Add (keySelector (e), elementSelector (e)));

			return lookup;
		}
		#endregion

		#region Concat
		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather than "
		                   + "System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() extension method "
		                   + "to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TSource> Concat<TSource>(this ParallelQuery<TSource> first,
		                                                     IEnumerable<TSource> second)
		{
			throw new NotSupportedException ();
		}

		public static ParallelQuery<TSource> Concat<TSource> (this ParallelQuery<TSource> first, ParallelQuery<TSource> second)
		{
			return new ParallelQuery<TSource> (new QueryConcatNode<TSource> (first.Node, second.Node));
		}
		#endregion
		
		#region DefaultIfEmpty
		public static ParallelQuery<TSource> DefaultIfEmpty<TSource> (this ParallelQuery<TSource> source)
		{
			return source.DefaultIfEmpty (default (TSource));
		}
		
		public static ParallelQuery<TSource> DefaultIfEmpty<TSource> (this ParallelQuery<TSource> source, TSource defaultValue)
		{
			return new ParallelQuery<TSource> (new QueryDefaultEmptyNode<TSource> (source.Node, defaultValue));
		}
		#endregion
		
		#region First
		public static TSource First<TSource> (this ParallelQuery<TSource> source)
		{
			CancellationTokenSource src = new CancellationTokenSource ();
			IEnumerator<TSource> enumerator = source.WithImplementerToken (src).GetEnumerator ();
			
			if (enumerator == null || !enumerator.MoveNext ())
				throw new InvalidOperationException ("source contains no element");
			
			TSource result = enumerator.Current;
			src.Cancel ();
			enumerator.Dispose ();
			
			return result;
		}
		
		public static TSource First<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			return source.Where (predicate).First ();
		}
		
		public static TSource FirstOrDefault<TSource> (this ParallelQuery<TSource> source)
		{
			return source.DefaultIfEmpty ().First ();
		}
		
		public static TSource FirstOrDefault<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			return source.Where (predicate).FirstOrDefault ();
		}
		#endregion
		
		#region Last
		public static TSource Last<TSource> (this ParallelQuery<TSource> source)
		{
			return source.Reverse ().First ();
		}
		
		public static TSource Last<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			return source.Reverse ().First (predicate);
		}
		
		public static TSource LastOrDefault<TSource> (this ParallelQuery<TSource> source)
		{
			return source.Reverse ().FirstOrDefault ();
		}
		
		public static TSource LastOrDefault<TSource> (this ParallelQuery<TSource> source, Func<TSource, bool> predicate)
		{
			return source.Reverse ().FirstOrDefault (predicate);
		}
		#endregion

		#region Zip
		public static ParallelQuery<TResult> Zip<TFirst, TSecond, TResult> (this ParallelQuery<TFirst> first,
		                                                                      ParallelQuery<TSecond> second,
		                                                                      Func<TFirst, TSecond, TResult> resultSelector)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			return new ParallelQuery<TResult> (new QueryZipNode<TFirst, TSecond, TResult> (resultSelector, first.Node, second.Node));
		}

		[ObsoleteAttribute("The second data source of a binary operator must be of type System.Linq.ParallelQuery<T> rather "
		                   + "than System.Collections.Generic.IEnumerable<T>. To fix this problem, use the AsParallel() "
		                   + "extension method to convert the right data source to System.Linq.ParallelQuery<T>.")]
		public static ParallelQuery<TResult> Zip<TFirst, TSecond, TResult> (this ParallelQuery<TFirst> first,
		                                                                      IEnumerable<TSecond> second,
		                                                                      Func<TFirst, TSecond, TResult> resultSelector)
		{
			throw new NotSupportedException ();
		}
		#endregion
	}
}
#endif
