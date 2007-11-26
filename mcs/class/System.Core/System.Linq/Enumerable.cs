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
//
// Authors:
//        Marek Safar (marek.safar@gmail.com)
//        Antonello Provenzano  <antonello@deveel.com>
//        Alejandro Serrano "Serras" (trupill@yahoo.es)
//        Jb Evain  (jbevain@novell.com)
//

// precious: http://www.hookedonlinq.com

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq
{
	public static class Enumerable
	{
		#region Aggregate

		public static TSource Aggregate<TSource> (this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
		{
			CheckSourceAndFunc (source, func);

			// custom foreach so that we can efficiently throw an exception 
			// if zero elements and treat the first element differently
			using (var enumerator = source.GetEnumerator ()) {
				if (!enumerator.MoveNext ())
					throw new InvalidOperationException ("No elements in source list");

				TSource folded = enumerator.Current;
				while (enumerator.MoveNext ())
					folded = func (folded, enumerator.Current);
				return folded;
			}
		}

		public static TAccumulate Aggregate<TSource, TAccumulate> (this IEnumerable<TSource> source,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			CheckSourceAndFunc (source, func);

			TAccumulate folded = seed;
			foreach (TSource element in source)
				folded = func (folded, element);
			return folded;
		}

		public static TResult Aggregate<TSource, TAccumulate, TResult> (this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			CheckSourceAndFunc (source, func);
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			TAccumulate result = seed;
			foreach (TSource e in source)
				result = func (result, e);
			return resultSelector (result);
		}

		#endregion

		#region All

		public static bool All<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			foreach (TSource element in source)
				if (!predicate (element))
					return false;

			return true;
		}

		#endregion

		#region Any

		public static bool Any<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			foreach (TSource element in source)
				return true;
			return false;
		}

		public static bool Any<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			foreach (TSource element in source)
				if (predicate (element))
					return true;
			return false;
		}

		#endregion

		#region AsEnumerable

		public static IEnumerable<TSource> AsEnumerable<TSource> (this IEnumerable<TSource> source)
		{
			return source;
		}

		#endregion

		#region Average

		public static double Average (this IEnumerable<int> source)
		{
			CheckSource (source);

			long sum = 0;
			long counter = 0;
			foreach (int element in source) {
				sum += element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return (double) sum / (double) counter;
		}

		public static double? Average (this IEnumerable<int?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			long sum = 0;
			long counter = 0;
			foreach (int? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (double?) sum / (double?) counter);
		}

		public static double Average (this IEnumerable<long> source)
		{
			CheckSource (source);

			long sum = 0;
			long counter = 0;
			foreach (long element in source) {
				sum += element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return (double) sum / (double) counter;
		}

		public static double? Average (this IEnumerable<long?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			long sum = 0;
			long counter = 0;
			foreach (long? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (double?) sum / (double?) counter);
		}

		public static double Average (this IEnumerable<double> source)
		{
			CheckSource (source);

			double sum = 0;
			double counter = 0;
			foreach (double element in source) {
				sum += element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return sum / counter;
		}

		public static double? Average (this IEnumerable<double?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			double sum = 0;
			double counter = 0;
			foreach (double? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (double?) (sum / counter));
		}

		public static decimal Average (this IEnumerable<decimal> source)
		{
			CheckSource (source);

			decimal sum = 0;
			decimal counter = 0;
			foreach (decimal element in source) {
				sum += element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return sum / counter;
		}

		public static decimal? Average (this IEnumerable<decimal?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			decimal sum = 0;
			decimal counter = 0;
			foreach (decimal? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (decimal?) (sum / counter));
		}

		public static double Average<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			CheckSourceAndSelector (source, selector);

			long sum = 0;
			long counter = 0;
			foreach (TSource item in source) {
				sum += selector (item);
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return (double) sum / (double) counter;
		}

		public static double? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			long sum = 0;
			long counter = 0;
			foreach (TSource item in source) {
				int? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (double?) sum / (double?) counter);
		}

		public static double Average<TSource> (this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			CheckSourceAndSelector (source, selector);

			long sum = 0;
			long counter = 0;
			foreach (TSource item in source) {
				sum += selector (item);
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return (double) sum / (double) counter;
		}

		public static double? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			long sum = 0;
			long counter = 0;
			foreach (TSource item in source) {
				long? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (double?) sum / (double?) counter);
		}

		public static double Average<TSource> (this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			CheckSourceAndSelector (source, selector);

			double sum = 0;
			double counter = 0;
			foreach (TSource item in source) {
				sum += selector (item);
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return sum / counter;
		}

		public static double? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			double sum = 0;
			double counter = 0;
			foreach (TSource item in source) {
				double? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (double?) (sum / counter));
		}

		public static decimal Average<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			CheckSourceAndSelector (source, selector);

			decimal sum = 0;
			decimal counter = 0;
			foreach (TSource item in source) {
				sum += selector (item);
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return sum / counter;
		}

		public static decimal? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			decimal sum = 0;
			decimal counter = 0;
			foreach (TSource item in source) {
				decimal? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					sum += element.Value;
					counter++;
				}
			}
			return (onlyNull ? null : (decimal?) (sum / counter));
		}
		#endregion

		#region Cast

		public static IEnumerable<TResult> Cast<TResult> (this IEnumerable source)
		{
			CheckSource (source);

			foreach (object element in source)
				yield return (TResult) element;
		}

		#endregion

		#region Concat

		public static IEnumerable<TSource> Concat<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			CheckFirstAndSecond (first, second);

			foreach (TSource element in first)
				yield return element;
			foreach (TSource element in second)
				yield return element;
		}

		#endregion

		#region Contains

		public static bool Contains<TSource> (this IEnumerable<TSource> source, TSource value)
		{
			var collection = source as ICollection<TSource>;
			if (collection != null)
				return collection.Contains (value);

			return Contains<TSource> (source, value, null);
		}

		public static bool Contains<TSource> (this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
		{
			CheckSource (source);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			foreach (TSource e in source) {
				if (comparer.Equals (e, value))
					return true;
			}

			return false;
		}
		#endregion

		#region Count

		public static int Count<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			var collection = source as ICollection<TSource>;
			if (collection != null)
				return collection.Count;

			int counter = 0;
			foreach (var element in source)
				counter++;

			return counter;
		}

		public static int Count<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> selector)
		{
			CheckSourceAndSelector (source, selector);

			int counter = 0;
			foreach (var element in source)
				if (selector (element))
					counter++;

			return counter;
		}

		#endregion

		#region DefaultIfEmpty

		public static IEnumerable<TSource> DefaultIfEmpty<TSource> (this IEnumerable<TSource> source)
		{
			return DefaultIfEmpty (source, default (TSource));
		}

		public static IEnumerable<TSource> DefaultIfEmpty<TSource> (this IEnumerable<TSource> source, TSource defaultValue)
		{
			CheckSource (source);

			bool noYield = true;
			foreach (TSource item in source) {
				noYield = false;
				yield return item;
			}

			if (noYield)
				yield return defaultValue;
		}

		#endregion

		#region Distinct

		public static IEnumerable<TSource> Distinct<TSource> (this IEnumerable<TSource> source)
		{
			return Distinct<TSource> (source, null);
		}

		public static IEnumerable<TSource> Distinct<TSource> (this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
		{
			CheckSource (source);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var items = new List<TSource> (); // TODO: use a HashSet here
			foreach (var element in source) {
				if (! items.Contains (element, comparer)) {
					items.Add (element);
					yield return element;
				}
			}
		}

		#endregion

		#region ElementAt

		public static TSource ElementAt<TSource> (this IEnumerable<TSource> source, int index)
		{
			CheckSource (source);
			if (index < 0)
				throw new ArgumentOutOfRangeException ();

			var list = source as IList<TSource>;
			if (list != null)
				return list [index];

			int counter = 0;
			foreach (var element in source) {
				if (counter == index)
					return element;
				counter++;
			}

			throw new ArgumentOutOfRangeException ();
		}

		#endregion

		#region ElementAtOrDefault

		public static TSource ElementAtOrDefault<TSource> (this IEnumerable<TSource> source, int index)
		{
			CheckSource (source);
			if (index < 0)
				return default (TSource);

			var list = source as IList<TSource>;
			if (list != null)
				return index < list.Count ? list [index] : default (TSource);

			int counter = 0;
			foreach (TSource element in source) {
				if (counter == index)
					return element;
				counter++;
			}

			return default (TSource);
		}

		#endregion

		#region Empty

		public static IEnumerable<TResult> Empty<TResult> ()
		{
			return new TResult [0];
		}

		#endregion

		#region Except

		public static IEnumerable<TSource> Except<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return Except (first, second, null);
		}

		public static IEnumerable<TSource> Except<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			CheckFirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var items = new List<TSource> (Distinct (second));
			foreach (TSource element in first) {
				if (! items.Contains (element, comparer))
					yield return element;
			}
		}

		#endregion

		#region First

		public static TSource First<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			foreach (TSource element in source)
				return element;

			throw new InvalidOperationException ();
		}


		public static TSource First<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			foreach (TSource element in source) {
				if (predicate (element))
					return element;
			}

			throw new InvalidOperationException ();
		}

		#endregion

		#region FirstOrDefault

		public static TSource FirstOrDefault<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			foreach (TSource element in source)
				return element;

			return default (TSource);
		}


		public static TSource FirstOrDefault<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			foreach (TSource element in source) {
				if (predicate (element))
					return element;
			}

			return default (TSource);
		}

		#endregion

		#region GroupBy

		private static List<T> ContainsGroup<K, T> (
				Dictionary<K, List<T>> items, K key, IEqualityComparer<K> comparer)
		{
			IEqualityComparer<K> comparerInUse = (comparer ?? EqualityComparer<K>.Default);
			foreach (KeyValuePair<K, List<T>> value in items) {
				if (comparerInUse.Equals (value.Key, key))
					return value.Value;
			}
			return null;
		}


		public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			return GroupBy<TSource, TKey> (source, keySelector, null);
		}


		public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			CheckSourceAndKeySelector (source, keySelector);

			Dictionary<TKey, List<TSource>> groups = new Dictionary<TKey, List<TSource>> ();
			List<TSource> nullList = new List<TSource> ();
			int counter = 0;
			int nullCounter = -1;

			foreach (TSource element in source) {
				TKey key = keySelector (element);
				if (key == null) {
					nullList.Add (element);
					if (nullCounter == -1) {
						nullCounter = counter;
						counter++;
					}
				} else {
					List<TSource> group = ContainsGroup<TKey, TSource> (groups, key, comparer);
					if (group == null) {
						group = new List<TSource> ();
						groups.Add (key, group);
						counter++;
					}
					group.Add (element);
				}
			}

			counter = 0;
			foreach (KeyValuePair<TKey, List<TSource>> group in groups) {
				if (counter == nullCounter) {
					Grouping<TKey, TSource> nullGroup = new Grouping<TKey, TSource> (default (TKey), nullList);
					yield return nullGroup;
					counter++;
				}
				Grouping<TKey, TSource> grouping = new Grouping<TKey, TSource> (group.Key, group.Value);
				yield return grouping;
				counter++;
			}
		}


		public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return GroupBy<TSource, TKey, TElement> (source, keySelector, elementSelector, null);
		}


		public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			CheckSourceAndKeyElementSelectors (source, keySelector, elementSelector);

			Dictionary<TKey, List<TElement>> groups = new Dictionary<TKey, List<TElement>> ();
			List<TElement> nullList = new List<TElement> ();
			int counter = 0;
			int nullCounter = -1;

			foreach (TSource item in source) {
				TKey key = keySelector (item);
				TElement element = elementSelector (item);
				if (key == null) {
					nullList.Add (element);
					if (nullCounter == -1) {
						nullCounter = counter;
						counter++;
					}
				} else {
					List<TElement> group = ContainsGroup<TKey, TElement> (groups, key, comparer);
					if (group == null) {
						group = new List<TElement> ();
						groups.Add (key, group);
						counter++;
					}
					group.Add (element);
				}
			}

			counter = 0;
			foreach (KeyValuePair<TKey, List<TElement>> group in groups) {
				if (counter == nullCounter) {
					Grouping<TKey, TElement> nullGroup = new Grouping<TKey, TElement> (default (TKey), nullList);
					yield return nullGroup;
					counter++;
				}
				Grouping<TKey, TElement> grouping = new Grouping<TKey, TElement> (group.Key, group.Value);
				yield return grouping;
				counter++;
			}
		}

		#endregion

		# region GroupJoin

		public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
		{
			return GroupJoin (outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
		}

		public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			CheckJoinSelectors (outer, inner, outerKeySelector, innerKeySelector, resultSelector);

			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			Lookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);
			/*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
			foreach (U element in inner)
			{
					K innerKey = innerKeySelector (element);
					if (!innerKeys.ContainsKey (innerKey))
							innerKeys.Add (innerKey, new List<U> ());
					innerKeys[innerKey].Add (element);
			}*/

			foreach (TOuter element in outer) {
				TKey outerKey = outerKeySelector (element);
				if (innerKeys.Contains (outerKey))
					yield return resultSelector (element, innerKeys [outerKey]);
				else
					yield return resultSelector (element, Empty<TInner> ());
			}	
		}

		#endregion

		#region Intersect

		public static IEnumerable<TSource> Intersect<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return Intersect (first, second, null);
		}

		public static IEnumerable<TSource> Intersect<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			CheckFirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			List<TSource> items = new List<TSource> (Distinct (first));
			foreach (TSource element in second) {
				if (Contains (items, element, comparer))
					yield return element;
			}
		}

		#endregion

		# region Join

		public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			CheckJoinSelectors (outer, inner, outerKeySelector, innerKeySelector, resultSelector);

			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			Lookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);
			/*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
			foreach (U element in inner)
			{
					K innerKey = innerKeySelector (element);
					if (!innerKeys.ContainsKey (innerKey))
							innerKeys.Add (innerKey, new List<U> ());
					innerKeys[innerKey].Add (element);
			}*/

			foreach (TOuter element in outer) {
				TKey outerKey = outerKeySelector (element);
				if (innerKeys.Contains (outerKey)) {
					foreach (TInner innerElement in innerKeys [outerKey])
						yield return resultSelector (element, innerElement);
				}
			}
		}

		public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
		{
			return Join<TOuter, TInner, TKey, TResult> (outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
		}

		# endregion

		#region Last

		public static TSource Last<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			bool noElements = true;
			TSource lastElement = default (TSource);
			foreach (TSource element in source) {
				if (noElements) noElements = false;
				lastElement = element;
			}

			if (!noElements)
				return lastElement;
			else
				throw new InvalidOperationException ();
		}

		public static TSource Last<TSource> (this IEnumerable<TSource> source,
				Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			bool noElements = true;
			TSource lastElement = default (TSource);
			foreach (TSource element in source) {
				if (predicate (element)) {
					if (noElements) noElements = false;
					lastElement = element;
				}
			}

			if (!noElements)
				return lastElement;
			else
				throw new InvalidOperationException ();
		}

		#endregion

		#region LastOrDefault

		public static TSource LastOrDefault<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			var list = source as IList<TSource>;
			if (list != null)
				return list.Count > 0 ? list [list.Count - 1] : default (TSource);

			TSource lastElement = default (TSource);
			foreach (TSource element in source)
				lastElement = element;

			return lastElement;
		}

		public static TSource LastOrDefault<TSource> (this IEnumerable<TSource> source,
			Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			TSource lastElement = default (TSource);
			foreach (TSource element in source) {
				if (predicate (element))
					lastElement = element;
			}

			return lastElement;
		}

		#endregion

		#region LongCount
		public static long LongCount<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			long counter = 0;
			foreach (TSource element in source)
				counter++;
			return counter;
		}


		public static long LongCount<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> selector)
		{
			CheckSourceAndSelector (source, selector);

			long counter = 0;
			foreach (TSource element in source)
				if (selector (element))
					counter++;

			return counter;
		}

		#endregion

		#region Max

		public static int Max (this IEnumerable<int> source)
		{
			CheckSource (source);

			int maximum = int.MinValue;
			int counter = 0;
			foreach (int element in source) {
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}

		public static int? Max (this IEnumerable<int?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			int? maximum = int.MinValue;
			foreach (int? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}

		public static long Max (this IEnumerable<long> source)
		{
			CheckSource (source);

			long maximum = long.MinValue;
			int counter = 0;
			foreach (long element in source) {
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static long? Max (this IEnumerable<long?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			long? maximum = long.MinValue;
			foreach (long? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static double Max (this IEnumerable<double> source)
		{
			CheckSource (source);

			double maximum = double.MinValue;
			int counter = 0;
			foreach (double element in source) {
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static double? Max (this IEnumerable<double?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			double? maximum = double.MinValue;
			foreach (double? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static decimal Max (this IEnumerable<decimal> source)
		{
			CheckSource (source);

			decimal maximum = decimal.MinValue;
			int counter = 0;
			foreach (decimal element in source) {
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static decimal? Max (this IEnumerable<decimal?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			decimal? maximum = decimal.MinValue;
			foreach (decimal? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static TSource Max<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			bool notAssigned = true;
			TSource maximum = default (TSource);
			int counter = 0;
			foreach (TSource element in source) {
				if (notAssigned) {
					maximum = element;
					notAssigned = false;
				} else {
					int comparison;
					if (element is IComparable<TSource>)
						comparison = ((IComparable<TSource>) element).CompareTo (maximum);
					else if (element is System.IComparable)
						comparison = ((System.IComparable) element).CompareTo (maximum);
					else
						throw new ArgumentNullException ();

					if (comparison > 0)
						maximum = element;
				}
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static int Max<TSource> (this IEnumerable<TSource> source,
				Func<TSource, int> selector)
		{
			CheckSourceAndSelector (source, selector);

			int maximum = int.MinValue;
			int counter = 0;
			foreach (TSource item in source) {
				int element = selector (item);
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static int? Max<TSource> (this IEnumerable<TSource> source,
				Func<TSource, int?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			int? maximum = int.MinValue;
			foreach (TSource item in source) {
				int? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static long Max<TSource> (this IEnumerable<TSource> source,
			Func<TSource, long> selector)
		{
			CheckSourceAndSelector (source, selector);

			long maximum = long.MinValue;
			int counter = 0;
			foreach (TSource item in source) {
				long element = selector (item);
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static long? Max<TSource> (this IEnumerable<TSource> source,
			Func<TSource, long?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			long? maximum = long.MinValue;
			foreach (TSource item in source) {
				long? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static double Max<TSource> (this IEnumerable<TSource> source,
			Func<TSource, double> selector)
		{
			CheckSourceAndSelector (source, selector);

			double maximum = double.MinValue;
			int counter = 0;
			foreach (TSource item in source) {
				double element = selector (item);
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static double? Max<TSource> (this IEnumerable<TSource> source,
			Func<TSource, double?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			double? maximum = double.MinValue;
			foreach (TSource item in source) {
				double? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static decimal Max<TSource> (this IEnumerable<TSource> source,
			Func<TSource, decimal> selector)
		{
			CheckSourceAndSelector (source, selector);

			decimal maximum = decimal.MinValue;
			int counter = 0;
			foreach (TSource item in source) {
				decimal element = selector (item);
				if (element > maximum)
					maximum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}


		public static decimal? Max<TSource> (this IEnumerable<TSource> source,
			Func<TSource, decimal?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			decimal? maximum = decimal.MinValue;
			foreach (TSource item in source) {
				decimal? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element > maximum)
						maximum = element;
				}
			}
			return (onlyNull ? null : maximum);
		}


		public static TResult Max<TSource, TResult> (this IEnumerable<TSource> source,
				Func<TSource, TResult> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool notAssigned = true;
			TResult maximum = default (TResult);
			int counter = 0;
			foreach (TSource item in source) {
				TResult element = selector (item);
				if (notAssigned) {
					maximum = element;
					notAssigned = false;
				} else {
					int comparison;
					if (element is IComparable<TResult>)
						comparison = ((IComparable<TResult>) element).CompareTo (maximum);
					else if (element is System.IComparable)
						comparison = ((System.IComparable) element).CompareTo (maximum);
					else
						throw new ArgumentNullException ();

					if (comparison > 0)
						maximum = element;
				}
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return maximum;
		}

		#endregion

		#region Min

		public static int Min (this IEnumerable<int> source)
		{
			CheckSource (source);

			int minimum = int.MaxValue;
			int counter = 0;
			foreach (int element in source) {
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static int? Min (this IEnumerable<int?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			int? minimum = int.MaxValue;
			foreach (int? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}

		public static long Min (this IEnumerable<long> source)
		{
			CheckSource (source);

			long minimum = long.MaxValue;
			int counter = 0;
			foreach (long element in source) {
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static long? Min (this IEnumerable<long?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			long? minimum = long.MaxValue;
			foreach (long? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static double Min (this IEnumerable<double> source)
		{
			CheckSource (source);

			double minimum = double.MaxValue;
			int counter = 0;
			foreach (double element in source) {
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static double? Min (this IEnumerable<double?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			double? minimum = double.MaxValue;
			foreach (double? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static decimal Min (this IEnumerable<decimal> source)
		{
			CheckSource (source);

			decimal minimum = decimal.MaxValue;
			int counter = 0;
			foreach (decimal element in source) {
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static decimal? Min (this IEnumerable<decimal?> source)
		{
			CheckSource (source);

			bool onlyNull = true;
			decimal? minimum = decimal.MaxValue;
			foreach (decimal? element in source) {
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static TSource Min<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			bool notAssigned = true;
			TSource minimum = default (TSource);
			int counter = 0;
			foreach (TSource element in source) {
				if (notAssigned) {
					minimum = element;
					notAssigned = false;
				} else {
					int comparison;
					if (element is IComparable<TSource>)
						comparison = ((IComparable<TSource>) element).CompareTo (minimum);
					else if (element is System.IComparable)
						comparison = ((System.IComparable) element).CompareTo (minimum);
					else
						throw new ArgumentNullException ();

					if (comparison < 0)
						minimum = element;
				}
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static int Min<TSource> (this IEnumerable<TSource> source,
			Func<TSource, int> selector)
		{
			CheckSourceAndSelector (source, selector);

			int minimum = int.MaxValue;
			int counter = 0;
			foreach (TSource item in source) {
				int element = selector (item);
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static int? Min<TSource> (this IEnumerable<TSource> source,
				Func<TSource, int?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			int? minimum = int.MaxValue;
			foreach (TSource item in source) {
				int? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static long Min<TSource> (this IEnumerable<TSource> source,
				Func<TSource, long> selector)
		{
			CheckSourceAndSelector (source, selector);

			long minimum = long.MaxValue;
			int counter = 0;
			foreach (TSource item in source) {
				long element = selector (item);
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static long? Min<TSource> (this IEnumerable<TSource> source,
				Func<TSource, long?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			long? minimum = long.MaxValue;
			foreach (TSource item in source) {
				long? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static double Min<TSource> (this IEnumerable<TSource> source,
			Func<TSource, double> selector)
		{
			CheckSourceAndSelector (source, selector);

			double minimum = double.MaxValue;
			int counter = 0;
			foreach (TSource item in source) {
				double element = selector (item);
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static double? Min<TSource> (this IEnumerable<TSource> source,
				Func<TSource, double?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			double? minimum = double.MaxValue;
			foreach (TSource item in source) {
				double? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static decimal Min<TSource> (this IEnumerable<TSource> source,
				Func<TSource, decimal> selector)
		{
			CheckSourceAndSelector (source, selector);

			decimal minimum = decimal.MaxValue;
			int counter = 0;
			foreach (TSource item in source) {
				decimal element = selector (item);
				if (element < minimum)
					minimum = element;
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}


		public static decimal? Min<TSource> (this IEnumerable<TSource> source,
				Func<TSource, decimal?> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool onlyNull = true;
			decimal? minimum = decimal.MaxValue;
			foreach (TSource item in source) {
				decimal? element = selector (item);
				if (element.HasValue) {
					onlyNull = false;
					if (element < minimum)
						minimum = element;
				}
			}
			return (onlyNull ? null : minimum);
		}


		public static TResult Min<TSource, TResult> (this IEnumerable<TSource> source,
				Func<TSource, TResult> selector)
		{
			CheckSourceAndSelector (source, selector);

			bool notAssigned = true;
			TResult minimum = default (TResult);
			int counter = 0;
			foreach (TSource item in source) {
				TResult element = selector (item);
				if (notAssigned) {
					minimum = element;
					notAssigned = false;
				} else {
					int comparison;
					if (element is IComparable<TResult>)
						comparison = ((IComparable<TResult>) element).CompareTo (minimum);
					else if (element is System.IComparable)
						comparison = ((System.IComparable) element).CompareTo (minimum);
					else
						throw new ArgumentNullException ();

					if (comparison < 0)
						minimum = element;
				}
				counter++;
			}

			if (counter == 0)
				throw new InvalidOperationException ();
			else
				return minimum;
		}

		#endregion

		#region OfType

		public static IEnumerable<TResult> OfType<TResult> (this IEnumerable source)
		{
			CheckSource (source);

			foreach (object element in source)
				if (element is TResult)
					yield return (TResult) element;
		}

		#endregion

		#region OrderBy

		public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector)
		{
			return OrderBy<TSource, TKey> (source, keySelector, null);
		}


		public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector,
				IComparer<TKey> comparer)
		{
			CheckSourceAndKeySelector (source, keySelector);

			return new InternalOrderedSequence<TSource, TKey> (
					source, keySelector, comparer, false);
		}

		#endregion

		#region OrderByDescending

		public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector)
		{
			return OrderByDescending<TSource, TKey> (source, keySelector, null);
		}


		public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			CheckSourceAndKeySelector (source, keySelector);

			return new InternalOrderedSequence<TSource, TKey> (
					source, keySelector, comparer, true);
		}

		#endregion

		#region Range

		public static IEnumerable<int> Range (int start, int count)
		{
			int upto = (start + count - 1);

			if (count < 0 || upto > int.MaxValue)
				throw new ArgumentOutOfRangeException ();

			for (int i = start; i <= upto; i++)
				yield return i;
		}

		#endregion

		#region Repeat

		public static IEnumerable<TResult> Repeat<TResult> (TResult element, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < count; i++)
				yield return element;
		}

		#endregion


		#region Reverse

		public static IEnumerable<TSource> Reverse<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			List<TSource> list = new List<TSource> (source);
			list.Reverse ();
			return list;
		}

		#endregion

		#region Select

		public static IEnumerable<TResult> Select<TSource, TResult> (this IEnumerable<TSource> source,
				Func<TSource, TResult> selector)
		{
			CheckSourceAndSelector (source, selector);

			foreach (TSource element in source)
				yield return selector (element);
		}

		public static IEnumerable<TResult> Select<TSource, TResult> (this IEnumerable<TSource> source,
				Func<TSource, int, TResult> selector)
		{
			CheckSourceAndSelector (source, selector);

			int counter = 0;
			foreach (TSource element in source) {
				yield return selector (element, counter);
				counter++;
			}
		}

		#endregion

		#region SelectMany

		public static IEnumerable<TResult> SelectMany<TSource, TResult> (this IEnumerable<TSource> source,
				Func<TSource, IEnumerable<TResult>> selector)
		{
			CheckSourceAndSelector (source, selector);

			foreach (TSource element in source)
				foreach (TResult item in selector (element))
					yield return item;
		}


		public static IEnumerable<TResult> SelectMany<TSource, TResult> (this IEnumerable<TSource> source,
				Func<TSource, int, IEnumerable<TResult>> selector)
		{
			CheckSourceAndSelector (source, selector);

			int counter = 0;
			foreach (TSource element in source) {
				foreach (TResult item in selector (element, counter++))
					yield return item;
				counter++;
			}
		}

		public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult> (this IEnumerable<TSource> source,
			Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			CheckSourceAndCollectionSelectors (source, collectionSelector, selector);

			foreach (TSource element in source)
				foreach (TCollection collection in collectionSelector (element))
					yield return selector (element, collection);
		}

		public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult> (this IEnumerable<TSource> source,
		Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			CheckSourceAndCollectionSelectors (source, collectionSelector, selector);

			int counter = 0;
			foreach (TSource element in source)
				foreach (TCollection collection in collectionSelector (element, counter++))
					yield return selector (element, collection);
		}

		#endregion

		#region Single

		public static TSource Single<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			bool otherElement = false;
			TSource singleElement = default (TSource);
			foreach (TSource element in source) {
				if (otherElement) throw new InvalidOperationException ();
				if (!otherElement) otherElement = true;
				singleElement = element;
			}

			if (otherElement)
				return singleElement;
			else
				throw new InvalidOperationException ();
		}


		public static TSource Single<TSource> (this IEnumerable<TSource> source,
				Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			bool otherElement = false;
			TSource singleElement = default (TSource);
			foreach (TSource element in source) {
				if (predicate (element)) {
					if (otherElement) throw new InvalidOperationException ();
					if (!otherElement) otherElement = true;
					singleElement = element;
				}
			}

			if (otherElement)
				return singleElement;
			else
				throw new InvalidOperationException ();
		}

		#endregion

		#region SingleOrDefault

		public static TSource SingleOrDefault<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			bool otherElement = false;
			TSource singleElement = default (TSource);
			foreach (TSource element in source) {
				if (otherElement) throw new InvalidOperationException ();
				if (!otherElement) otherElement = true;
				singleElement = element;
			}

			return singleElement;
		}


		public static TSource SingleOrDefault<TSource> (this IEnumerable<TSource> source,
				Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			bool otherElement = false;
			TSource singleElement = default (TSource);
			foreach (TSource element in source) {
				if (predicate (element)) {
					if (otherElement) throw new InvalidOperationException ();
					if (!otherElement) otherElement = true;
					singleElement = element;
				}
			}

			return singleElement;
		}

		#endregion

		#region Skip
		public static IEnumerable<TSource> Skip<TSource> (this IEnumerable<TSource> source, int count)
		{
			CheckSource (source);

			int i = 0;
			foreach (var element in source) {
				if (i++ < count)
					continue;

				yield return element;
			}
		}
		#endregion

		#region SkipWhile


		public static IEnumerable<TSource> SkipWhile<TSource> (
				this IEnumerable<TSource> source,
				Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			bool yield = false;

			foreach (TSource element in source) {
				if (yield)
					yield return element;
				else
					if (!predicate (element)) {
						yield return element;
						yield = true;
					}
			}
		}

		public static IEnumerable<TSource> SkipWhile<TSource> (this IEnumerable<TSource> source,
				Func<TSource, int, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			int counter = 0;
			bool yield = false;

			foreach (TSource element in source) {
				if (yield)
					yield return element;
				else
					if (!predicate (element, counter)) {
						yield return element;
						yield = true;
					}
				counter++;
			}
		}

		#endregion

		#region Sum

		public static int Sum (this IEnumerable<int> source)
		{
			CheckSource (source);

			int sum = 0;
			foreach (int element in source)
				sum += element;

			return sum;
		}

		public static int Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			CheckSourceAndSelector (source, selector);

			int sum = 0;
			foreach (TSource element in source)
				sum += selector (element);

			return sum;
		}


		public static int? Sum (this IEnumerable<int?> source)
		{
			CheckSource (source);

			int? sum = 0;
			foreach (int? element in source)
				if (element.HasValue)
					sum += element.Value;

			return sum;
		}


		public static int? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			CheckSourceAndSelector (source, selector);

			int? sum = 0;
			foreach (TSource element in source) {
				int? item = selector (element);
				if (item.HasValue)
					sum += item.Value;
			}

			return sum;
		}


		public static long Sum (this IEnumerable<long> source)
		{
			CheckSource (source);

			long sum = 0;
			foreach (long element in source)
				sum += element;

			return sum;
		}


		public static long Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			CheckSourceAndSelector (source, selector);

			long sum = 0;
			foreach (TSource element in source)
				sum += selector (element);

			return sum;
		}


		public static long? Sum (this IEnumerable<long?> source)
		{
			CheckSource (source);

			long? sum = 0;
			foreach (long? element in source)
				if (element.HasValue)
					sum += element.Value;

			return sum;
		}


		public static long? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			CheckSourceAndSelector (source, selector);

			long? sum = 0;
			foreach (TSource element in source) {
				long? item = selector (element);
				if (item.HasValue)
					sum += item.Value;
			}

			return sum;
		}


		public static double Sum (this IEnumerable<double> source)
		{
			CheckSource (source);

			double sum = 0;
			foreach (double element in source)
				sum += element;

			return sum;
		}


		public static double Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			CheckSourceAndSelector (source, selector);

			double sum = 0;
			foreach (TSource element in source)
				sum += selector (element);

			return sum;
		}


		public static double? Sum (this IEnumerable<double?> source)
		{
			CheckSource (source);

			double? sum = 0;
			foreach (double? element in source)
				if (element.HasValue)
					sum += element.Value;

			return sum;
		}


		public static double? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			CheckSourceAndSelector (source, selector);

			double? sum = 0;
			foreach (TSource element in source) {
				double? item = selector (element);
				if (item.HasValue)
					sum += item.Value;
			}

			return sum;
		}


		public static decimal Sum (this IEnumerable<decimal> source)
		{
			CheckSource (source);

			decimal sum = 0;
			foreach (decimal element in source)
				sum += element;

			return sum;
		}


		public static decimal Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			CheckSourceAndSelector (source, selector);

			decimal sum = 0;
			foreach (TSource element in source)
				sum += selector (element);

			return sum;
		}


		public static decimal? Sum (this IEnumerable<decimal?> source)
		{
			CheckSource (source);

			decimal? sum = 0;
			foreach (decimal? element in source)
				if (element.HasValue)
					sum += element.Value;

			return sum;
		}


		public static decimal? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			CheckSourceAndSelector (source, selector);

			decimal? sum = 0;
			foreach (TSource element in source) {
				decimal? item = selector (element);
				if (item.HasValue)
					sum += item.Value;
			}

			return sum;
		}

		#endregion

		#region Take

		public static IEnumerable<TSource> Take<TSource> (this IEnumerable<TSource> source, int count)
		{
			CheckSource (source);

			if (count <= 0)
				yield break;

			int counter = 0;
			foreach (TSource element in source) {
				if (counter++ == count)
					yield break;

				yield return element;
			}
		}

		#endregion

		#region TakeWhile

		public static IEnumerable<TSource> TakeWhile<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			foreach (TSource element in source) {
				if (!predicate (element))
					yield break;

				yield return element;
			}
		}

		public static IEnumerable<TSource> TakeWhile<TSource> (this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			int counter = 0;
			foreach (var element in source) {
				if (!predicate (element, counter))
					yield break;

				yield return element;
				counter++;
			}
		}

		#endregion

		#region ThenBy

		public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey> (this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return ThenBy<TSource, TKey> (source, keySelector, null);
		}


		public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey> (this IOrderedEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			CheckSourceAndKeySelector (source, keySelector);

			return source.CreateOrderedEnumerable (keySelector, comparer, false);
		}

		#endregion

		#region ThenByDescending

		public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey> (this IOrderedEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			return ThenByDescending<TSource, TKey> (source, keySelector, null);
		}


		public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey> (this IOrderedEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			CheckSourceAndKeySelector (source, keySelector);

			return source.CreateOrderedEnumerable (keySelector, comparer, true);
		}

		#endregion

		#region ToArray
		public static TSource [] ToArray<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			List<TSource> list = new List<TSource> (source);
			return list.ToArray ();
		}

		#endregion

		#region ToDictionary
		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return ToDictionary<TSource, TKey, TElement> (source, keySelector, elementSelector, null);
		}


		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			CheckSourceAndKeyElementSelectors (source, keySelector, elementSelector);

			var dict = new Dictionary<TKey, TElement> (comparer);
			foreach (var e in source)
				dict.Add (keySelector (e), elementSelector (e));

			return dict;
		}
		#endregion

		#region ToList
		public static List<TSource> ToList<TSource> (this IEnumerable<TSource> source)
		{
			CheckSource (source);

			return new List<TSource> (source);
		}
		#endregion

		#region ToLookup

		public static Lookup<TKey, TSource> ToLookup<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return ToLookup<TSource, TKey> (source, keySelector, null);
		}


		public static Lookup<TKey, TSource> ToLookup<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			CheckSourceAndKeySelector (source, keySelector);

			var dictionary = new Dictionary<TKey, List<TSource>> (comparer ?? EqualityComparer<TKey>.Default);
			foreach (TSource element in source) {
				TKey key = keySelector (element);
				if (key == null)
					throw new ArgumentNullException ();
				if (!dictionary.ContainsKey (key))
					dictionary.Add (key, new List<TSource> ());
				dictionary [key].Add (element);
			}
			return new Lookup<TKey, TSource> (dictionary);
		}


		public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return ToLookup<TSource, TKey, TElement> (source, keySelector, elementSelector, null);
		}


		public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			CheckSourceAndKeyElementSelectors (source, keySelector, elementSelector);

			Dictionary<TKey, List<TElement>> dictionary = new Dictionary<TKey, List<TElement>> (comparer ?? EqualityComparer<TKey>.Default);
			foreach (TSource element in source) {
				TKey key = keySelector (element);
				if (key == null)
					throw new ArgumentNullException ();
				if (!dictionary.ContainsKey (key))
					dictionary.Add (key, new List<TElement> ());
				dictionary [key].Add (elementSelector (element));
			}
			return new Lookup<TKey, TElement> (dictionary);
		}

		#endregion

		#region ToSequence

		public static IEnumerable<T> ToSequence<T> (this IEnumerable<T> source)
		{
			return (IEnumerable<T>) source;
		}

		#endregion

		#region SequenceEqual

		public static bool SequenceEqual<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return first.SequenceEqual (second, null);
		}

		public static bool SequenceEqual<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			CheckFirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var first_enumerator = first.GetEnumerator ();
			var second_enumerator = second.GetEnumerator ();

			while (first_enumerator.MoveNext ()) {
				if (!second_enumerator.MoveNext ())
					return false;

				if (!comparer.Equals (first_enumerator.Current, second_enumerator.Current))
					return false;
			}

			return !second_enumerator.MoveNext ();
		}

		#endregion

		#region Union

		public static IEnumerable<TSource> Union<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			CheckFirstAndSecond (first, second);

			return first.Union (second, null);
		}

		public static IEnumerable<TSource> Union<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			CheckFirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var items = new List<TSource> (); // TODO: use a HashSet here
			foreach (var element in first) {
				if (! items.Contains (element, comparer)) {
					items.Add (element);
					yield return element;
				}
			}

			foreach (var element in second) {
				if (! items.Contains (element, comparer)) {
					items.Add (element);
					yield return element;
				}
			}
		}

		#endregion

		#region Where

		public static IEnumerable<TSource> Where<TSource> (this IEnumerable<TSource> source,
				Func<TSource, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			foreach (TSource element in source)
				if (predicate (element))
					yield return element;
		}


		public static IEnumerable<TSource> Where<TSource> (this IEnumerable<TSource> source,
				Func<TSource, int, bool> predicate)
		{
			CheckSourceAndPredicate (source, predicate);

			int counter = 0;
			foreach (TSource element in source) {
				if (predicate (element, counter))
					yield return element;
				counter++;
			}
		}

		#endregion

		// These methods are not included in the
		// .NET Standard Query Operators Specification,
		// but they provide additional useful commands

		#region Checks

		static void CheckSource (object source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
		}

		static void CheckSourceAndFunc (object source, object func)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (func == null)
				throw new ArgumentNullException ("func");
		}

		static void CheckSourceAndSelector (object source, object selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (selector == null)
				throw new ArgumentNullException ("selector");
		}

		static void CheckSourceAndPredicate (object source, object predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
		}

		static void CheckFirstAndSecond (object first, object second)
		{
			if (first == null)
				throw new ArgumentNullException ("first");
			if (second == null)
				throw new ArgumentNullException ("second");
		}

		static void CheckSourceAndKeySelector (object source, object keySelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
		}

		static void CheckSourceAndKeyElementSelectors (object source, object keySelector, object elementSelector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (elementSelector == null)
				throw new ArgumentNullException ("elementSelector");
		}

		static void CheckSourceAndCollectionSelectors (object source, object collectionSelector, object selector)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (collectionSelector == null)
				throw new ArgumentNullException ("collectionSelector");
			if (selector == null)
				throw new ArgumentNullException ("selector");
		}

		static void CheckJoinSelectors (object outer, object inner, object outerKeySelector, object innerKeySelector, object resultSelector)
		{
			if (outer == null)
				throw new ArgumentNullException ("outer");
			if (inner == null)
				throw new ArgumentNullException ("inner");
			if (outerKeySelector == null)
				throw new ArgumentNullException ("outerKeySelector");
			if (innerKeySelector == null)
				throw new ArgumentNullException ("innerKeySelector");
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
		}

		#endregion

		#region ToReadOnlyCollection
		internal static ReadOnlyCollection<TSource> ToReadOnlyCollection<TSource> (IEnumerable<TSource> source)
		{
			if (source == null)
				return new ReadOnlyCollection<TSource> (new List<TSource> ());

			if (typeof (ReadOnlyCollection<TSource>).IsInstanceOfType (source))
				return source as ReadOnlyCollection<TSource>;

			return new ReadOnlyCollection<TSource> (ToArray<TSource> (source));
		}
		#endregion
	}
}
