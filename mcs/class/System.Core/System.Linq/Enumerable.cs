//
// Enumerable.cs
//
// Authors:
//  Marek Safar (marek.safar@gmail.com)
//  Antonello Provenzano  <antonello@deveel.com>
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

// precious: http://www.hookedonlinq.com

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq
{
	public static class Enumerable
	{
		enum Fallback {
			Default,
			Throw
		}

#if !FULL_AOT_RUNTIME
		static class PredicateOf<T> {
			public static readonly Func<T, bool> Always = (t) => true;
		}
#endif

		static class Function<T> {
			public static readonly Func<T, T> Identity = (t) => t;
		}
		
		static class EmptyOf<T> {
			public static readonly T[] Instance = new T [0];
		}
		
		static class ReadOnlyCollectionOf<T> {
			public static readonly ReadOnlyCollection<T> Empty = new ReadOnlyCollection<T> (EmptyOf<T>.Instance);
		}

		#region Aggregate

		public static TSource Aggregate<TSource> (this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
		{
			Check.SourceAndFunc (source, func);

			TSource folded;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				folded = list [0];
				for (int i = 1, c = list.Count; i < c; ++i)
					folded = func (folded, list [i]);
				return folded;
			}

			// custom foreach so that we can efficiently throw an exception
			// if zero elements and treat the first element differently
			using (var enumerator = source.GetEnumerator ()) {
				if (!enumerator.MoveNext ())
					throw EmptySequence ();

				folded = enumerator.Current;
				while (enumerator.MoveNext ())
					folded = func (folded, enumerator.Current);
				return folded;
			}
		}

		public static TAccumulate Aggregate<TSource, TAccumulate> (this IEnumerable<TSource> source,
			TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
		{
			Check.SourceAndFunc (source, func);

			var folded = seed;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					folded = func (folded, list [i]);
			} else {
				foreach (var element in source)
					folded = func (folded, element);
			}

			return folded;
		}

		public static TResult Aggregate<TSource, TAccumulate, TResult> (this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
		{
			Check.SourceAndFunc (source, func);
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");

			var result = seed;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					result = func (result, list [i]);
			} else {
				foreach (var element in source)
					result = func (result, element);
			}

			return resultSelector (result);
		}

		#endregion

		#region All

		public static bool All<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (!predicate (list [i]))
						return false;
			} else {
				foreach (var element in source)
					if (!predicate (element))
						return false;
			}

			return true;
		}

		#endregion

		#region Any

		public static bool Any<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var collection = source as ICollection<TSource>;
			if (collection != null)
				return collection.Count > 0;

			using (var enumerator = source.GetEnumerator ())
				return enumerator.MoveNext ();
		}

		public static bool Any<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (predicate (list [i]))
						return true;
			} else {
				foreach (var element in source)
					if (predicate (element))
						return true;
			}

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
			Check.Source (source);

			long total = 0;
			long count = 0;

			var list = source as IList<int>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source){
					checked {
						total += element;
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / (double) count;
		}

		public static double Average (this IEnumerable<long> source)
		{
			Check.Source (source);

			long total = 0;
			long count = 0;

			var list = source as IList<long>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source){
					checked {
						total += element;
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / (double) count;
		}

		public static double Average (this IEnumerable<double> source)
		{
			Check.Source (source);

			double total = 0;
			long count = 0;

			var list = source as IList<double>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source){
					checked {
						total += element;
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / count;
		}

		public static float Average (this IEnumerable<float> source)
		{
			Check.Source (source);

			float total = 0;
			long count = 0;

			var list = source as IList<float>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source){
					checked {
						total += element;
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / count;
		}

		public static decimal Average (this IEnumerable<decimal> source)
		{
			Check.Source (source);

			decimal total = 0;
			long count = 0;

			var list = source as IList<decimal>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source){
					checked {
						total += element;
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / count;
		}

		public static double? Average (this IEnumerable<int?> source)
		{
			Check.Source (source);

			long total = 0;
			long count = 0;

			var list = source as IList<int?>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source){
					if (element.HasValue) {
						checked {
							total += element.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new double? (total / (double) count);
		}

		public static double? Average (this IEnumerable<long?> source)
		{
			Check.Source (source);

			long total = 0;
			long count = 0;

			var list = source as IList<long?>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source){
					if (element.HasValue) {
						checked {
							total += element.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new double? (total / (double) count);

		}

		public static double? Average (this IEnumerable<double?> source)
		{
			Check.Source (source);

			double total = 0;
			long count = 0;

			var list = source as IList<double?>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source){
					if (element.HasValue) {
						checked {
							total += element.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new double? (total / count);

		}

		public static decimal? Average (this IEnumerable<decimal?> source)
		{
			Check.Source (source);

			decimal total = 0;
			long count = 0;

			var list = source as IList<decimal?>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source){
					if (element.HasValue) {
						checked {
							total += element.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new decimal? (total / count);

		}

		public static float? Average (this IEnumerable<float?> source)
		{
			Check.Source (source);

			float total = 0;
			long count = 0;

			var list = source as IList<float?>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source){
					if (element.HasValue) {
						checked {
							total += element.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new float? (total / count);

		}

		public static double Average<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector (source, selector);

			long total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source){
					checked {
						total += selector (element);
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / (double) count;
		}

		public static double? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector (source, selector);

			long total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i) {
					var val = selector (list [i]);
					if (val.HasValue)
						total = checked (total + val.Value);
				}
			} else {
				foreach (var element in source) {
					var val = selector (element);
					if (val.HasValue) {
						checked {
							total += val.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new double? (total / (double) count);
		}

		public static double Average<TSource> (this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector (source, selector);

			long total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source){
					checked {
						total += selector (element);
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / (double) count;

		}

		public static double? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector (source, selector);

			long total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i) {
					var val = selector (list [i]);
					if (val.HasValue)
						total = checked (total + val.Value);
				}
			} else {
				foreach (var element in source) {
					var val = selector (element);
					if (val.HasValue) {
						checked {
							total += val.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new double? (total / (double) count);
		}

		public static double Average<TSource> (this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector (source, selector);

			double total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source) {
					checked {
						total += selector (element);
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / count;

		}

		public static double? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector (source, selector);

			double total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i) {
					var val = selector (list [i]);
					if (val.HasValue)
						total = checked (total + val.Value);
				}
			} else {
				foreach (var element in source) {
					var val = selector (element);
					if (val.HasValue) {
						checked {
							total += val.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new double? (total / count);

		}

		public static float Average<TSource> (this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector (source, selector);

			float total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source) {
					checked {
						total += selector (element);
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / count;
		}

		public static float? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector (source, selector);

			float total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i) {
					var val = selector (list [i]);
					if (val.HasValue)
						total = checked (total + val.Value);
				}
			} else {
				foreach (var element in source) {
					var val = selector (element);
					if (val.HasValue) {
						checked {
							total += val.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new float? (total / count);
		}

		public static decimal Average<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector (source, selector);

			decimal total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source) {
					checked {
						total += selector (element);
						count += 1;
					}
				}
			}

			if (count == 0)
				throw EmptySequence ();

			return total / count;
		}

		public static decimal? Average<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector (source, selector);

			decimal total = 0;
			long count = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				count = list.Count;
				for (int i = 0; i < count; ++i) {
					var val = selector (list [i]);
					if (val.HasValue)
						total = checked (total + val.Value);
				}
			} else {
				foreach (var element in source) {
					var val = selector (element);
					if (val.HasValue) {
						checked {
							total += val.Value;
							count += 1;
						}
					}
				}
			}

			if (count == 0)
				return null;

			return new decimal? (total / count);
		}

		#endregion

		#region Cast

		public static IEnumerable<TResult> Cast<TResult> (this IEnumerable source)
		{
			Check.Source (source);

			var actual = source as IEnumerable<TResult>;
			if (actual != null)
				return actual;

			var list = source as IList;
			if (list != null)
				return CreateCastIterator<TResult> (list);

			return CreateCastIterator<TResult> (source);
		}

		static IEnumerable<TResult> CreateCastIterator<TResult> (IList source)
		{
			for (int i = 0, c = source.Count; i < c; ++i)
				yield return (TResult) source [i];
		}

		static IEnumerable<TResult> CreateCastIterator<TResult> (IEnumerable source)
		{
			foreach (TResult element in source)
				yield return element;
		}

		#endregion

		#region Concat

		public static IEnumerable<TSource> Concat<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			Check.FirstAndSecond (first, second);

			var first_list = first as IList<TSource>;
			var second_list = second as IList<TSource>;
			if (first_list != null && second_list != null)
				return CreateConcatIterator (first_list, second_list);
			else if (first_list != null)
				return CreateConcatIterator (first_list, second);
			else if (second_list != null)
				return CreateConcatIterator (first, second_list);

			return CreateConcatIterator (first, second);
		}

		static IEnumerable<TSource> CreateConcatIterator<TSource> (IList<TSource> first, IList<TSource> second)
		{
			for (int i = 0, c1 = first.Count; i < c1; ++i)
				yield return first [i];
			for (int j = 0, c2 = second.Count; j < c2; ++j)
				yield return second [j];
		}

		static IEnumerable<TSource> CreateConcatIterator<TSource> (IList<TSource> first, IEnumerable<TSource> second)
		{
			for (int i = 0, c1 = first.Count; i < c1; ++i)
				yield return first [i];
			foreach (var element in second)
				yield return element;
		}

		static IEnumerable<TSource> CreateConcatIterator<TSource> (IEnumerable<TSource> first, IList<TSource> second)
		{
			foreach (var element in first)
				yield return element;
			for (int j = 0, c2 = second.Count; j < c2; ++j)
				yield return second [j];
		}

		static IEnumerable<TSource> CreateConcatIterator<TSource> (IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			foreach (var element in first)
				yield return element;
			foreach (var element in second)
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
			Check.Source (source);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (comparer.Equals (list [i], value))
						return true;
			} else {
				foreach (var element in source)
					if (comparer.Equals (element, value))
						return true;
			}

			return false;
		}
		#endregion

		#region Count

		public static int Count<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var collection = source as ICollection<TSource>;
			if (collection != null)
				return collection.Count;

			int counter = 0;
			using (var enumerator = source.GetEnumerator ())
				while (enumerator.MoveNext ())
					checked { counter++; }

			return counter;
		}

		public static int Count<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndSelector (source, predicate);

			int counter = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (predicate (list [i]))
						checked { counter++; }
			} else {
				foreach (var element in source)
					if (predicate (element))
						checked { counter++; }
			}

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
			Check.Source (source);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateDefaultIfEmptyIterator (list, defaultValue);

			return CreateDefaultIfEmptyIterator (source, defaultValue);
		}

		static IEnumerable<TSource> CreateDefaultIfEmptyIterator<TSource> (IList<TSource> source, TSource defaultValue)
		{
			if (source.Count == 0) {
				yield return defaultValue;
			} else {
				for (int i = 0, c = source.Count; i < c; ++i)
					yield return source [i];
			}
		}

		static IEnumerable<TSource> CreateDefaultIfEmptyIterator<TSource> (IEnumerable<TSource> source, TSource defaultValue)
		{
			bool empty = true;
			foreach (TSource item in source) {
				empty = false;
				yield return item;
			}

			if (empty)
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
			Check.Source (source);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var list = source as IList<TSource>;
			if (list != null)
				return CreateDistinctIterator (list, comparer);

			return CreateDistinctIterator (source, comparer);
		}

		static IEnumerable<TSource> CreateDistinctIterator<TSource> (IList<TSource> source, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (comparer);
			for (int i = 0, c = source.Count; i < c; ++i) {
				if (!items.Contains (source [i])) {
					items.Add (source [i]);
					yield return source [i];
				}
			}
		}

		static IEnumerable<TSource> CreateDistinctIterator<TSource> (IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (comparer);
			foreach (var element in source) {
				if (! items.Contains (element)) {
					items.Add (element);
					yield return element;
				}
			}
		}

		#endregion

		#region ElementAt

		static TSource ElementAt<TSource> (this IEnumerable<TSource> source, int index, Fallback fallback)
		{
			int counter = 0;

			foreach (var element in source) {
				if (index == counter++)
					return element;
			}

			if (fallback == Fallback.Throw)
				throw new ArgumentOutOfRangeException ();

			return default (TSource);
		}

		public static TSource ElementAt<TSource> (this IEnumerable<TSource> source, int index)
		{
			Check.Source (source);

			if (index < 0)
				throw new ArgumentOutOfRangeException ();

			var list = source as IList<TSource>;
			if (list != null)
				return list [index];

#if NET_4_5
			var readOnlyList = source as IReadOnlyList<TSource>;
			if (readOnlyList != null)
				return readOnlyList[index];
#endif

			return source.ElementAt (index, Fallback.Throw);
		}

		#endregion

		#region ElementAtOrDefault

		public static TSource ElementAtOrDefault<TSource> (this IEnumerable<TSource> source, int index)
		{
			Check.Source (source);

			if (index < 0)
				return default (TSource);

			var list = source as IList<TSource>;
			if (list != null)
				return index < list.Count ? list [index] : default (TSource);

#if NET_4_5
			var readOnlyList = source as IReadOnlyList<TSource>;
			if (readOnlyList != null)
				return index < readOnlyList.Count ? readOnlyList [index] : default (TSource);
#endif

			return source.ElementAt (index, Fallback.Default);
		}

		#endregion

		#region Empty

		public static IEnumerable<TResult> Empty<TResult> ()
		{
			return EmptyOf<TResult>.Instance;
		}

		#endregion

		#region Except

		public static IEnumerable<TSource> Except<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return Except (first, second, null);
		}

		public static IEnumerable<TSource> Except<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			return CreateExceptIterator (first, second, comparer);
		}

		static IEnumerable<TSource> CreateExceptIterator<TSource> (IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (second, comparer);
			foreach (var element in first) {
				if (items.Add (element))
					yield return element;
			}
		}

		#endregion

		#region First

		static TSource First<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
		{
			foreach (var element in source)
				if (predicate (element))
					return element;

			if (fallback == Fallback.Throw)
				throw NoMatchingElement ();

			return default (TSource);
		}

		public static TSource First<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count != 0)
					return list [0];
			} else {
				using (var enumerator = source.GetEnumerator ()) {
					if (enumerator.MoveNext ())
						return enumerator.Current;
				}
			}

			throw EmptySequence ();
		}

		public static TSource First<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.First (predicate, Fallback.Throw);
		}

		#endregion

		#region FirstOrDefault

		public static TSource FirstOrDefault<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

#if !FULL_AOT_RUNTIME
			return source.First (PredicateOf<TSource>.Always, Fallback.Default);
#else
			var list = source as IList<TSource>;
			if (list != null)
				return list.Count == 0 ? default (TSource) : list [0]

			// inline the code to reduce dependency o generic causing AOT errors on device (e.g. bug #3285)
			foreach (var element in source)
				return element;

			return default (TSource);
#endif
		}

		public static TSource FirstOrDefault<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.First (predicate, Fallback.Default);
		}

		#endregion

		#region GroupBy

		public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector)
		{
			return GroupBy<TSource, TKey> (source, keySelector, null);
		}

		public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeySelector (source, keySelector);

			return CreateGroupByIterator (source, keySelector, comparer);
		}

		static IEnumerable<IGrouping<TKey, TSource>> CreateGroupByIterator<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			var groups = new Dictionary<TKey, List<TSource>> (comparer);
			var nullList = new List<TSource> ();
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
					List<TSource> group;
					if (!groups.TryGetValue (key, out group)) {
						group = new List<TSource> ();
						groups.Add (key, group);
						counter++;
					}
					group.Add (element);
				}
			}

			counter = 0;
			foreach (var group in groups) {
				if (counter == nullCounter) {
					yield return new Grouping<TKey, TSource> (default (TKey), nullList);
					counter++;
				}

				yield return new Grouping<TKey, TSource> (group.Key, group.Value);
				counter++;
			}

			if (counter == nullCounter) {
				yield return new Grouping<TKey, TSource> (default (TKey), nullList);
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
			Check.SourceAndKeyElementSelectors (source, keySelector, elementSelector);

			return CreateGroupByIterator (source, keySelector, elementSelector, comparer);
		}

		static IEnumerable<IGrouping<TKey, TElement>> CreateGroupByIterator<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			var groups = new Dictionary<TKey, List<TElement>> (comparer);
			var nullList = new List<TElement> ();
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
					List<TElement> group;
					if (!groups.TryGetValue (key, out group)) {
						group = new List<TElement> ();
						groups.Add (key, group);
						counter++;
					}
					group.Add (element);
				}
			}

			counter = 0;
			foreach (var group in groups) {
				if (counter == nullCounter) {
					yield return new Grouping<TKey, TElement> (default (TKey), nullList);
					counter++;
				}

				yield return new Grouping<TKey, TElement> (group.Key, group.Value);
				counter++;
			}

			if (counter == nullCounter) {
				yield return new Grouping<TKey, TElement> (default (TKey), nullList);
				counter++;
			}
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
			Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
		{
			return GroupBy (source, keySelector, elementSelector, resultSelector, null);
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
			Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			Check.GroupBySelectors (source, keySelector, elementSelector, resultSelector);

			return CreateGroupByIterator (source, keySelector, elementSelector, resultSelector, comparer);
		}

		static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TElement, TResult> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector,
			Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			IEnumerable<IGrouping<TKey, TElement>> groups = GroupBy<TSource, TKey, TElement> (
				source, keySelector, elementSelector, comparer);

			foreach (IGrouping<TKey, TElement> group in groups)
				yield return resultSelector (group.Key, group);
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
		{
			return GroupBy (source, keySelector, resultSelector, null);
		}

		public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyResultSelectors (source, keySelector, resultSelector);

			return CreateGroupByIterator (source, keySelector, resultSelector, comparer);
		}

		static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TResult> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			IEnumerable<IGrouping<TKey,TSource>> groups = GroupBy<TSource, TKey> (source, keySelector, comparer);

			foreach (IGrouping<TKey, TSource> group in groups)
				yield return resultSelector (group.Key, group);
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
			Check.JoinSelectors (outer, inner, outerKeySelector, innerKeySelector, resultSelector);

			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			var list = outer as IList<TOuter>;
			if (list != null)
				return CreateGroupJoinIterator (list, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

			return CreateGroupJoinIterator (outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		}

		static IEnumerable<TResult> CreateGroupJoinIterator<TOuter, TInner, TKey, TResult> (this IList<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);

			for (int i = 0, c = outer.Count; i < c; ++i) {
				TKey outerKey = outerKeySelector (outer [i]);
				if (outerKey != null && innerKeys.Contains (outerKey))
					yield return resultSelector (outer [i], innerKeys [outerKey]);
				else
					yield return resultSelector (outer [i], Empty<TInner> ());
			}
		}

		static IEnumerable<TResult> CreateGroupJoinIterator<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
			IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);
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
				if (outerKey != null && innerKeys.Contains (outerKey))
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
			Check.FirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var list = first as IList<TSource>;
			if (list != null)
				return CreateIntersectIterator (list, second, comparer);

			return CreateIntersectIterator (first, second, comparer);
		}

		static IEnumerable<TSource> CreateIntersectIterator<TSource> (IList<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (second, comparer);
			for (int i = 0, c = first.Count; i < c; ++i) {
				if (items.Remove (first [i]))
					yield return first [i];
			}
		}

		static IEnumerable<TSource> CreateIntersectIterator<TSource> (IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (second, comparer);
			foreach (TSource element in first) {
				if (items.Remove (element))
					yield return element;
			}
		}

		#endregion

		# region Join

		public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			Check.JoinSelectors (outer, inner, outerKeySelector, innerKeySelector, resultSelector);

			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			var outer_list = outer as IList<TOuter>;
			var inner_list = outer as IList<TInner>;
			if (outer_list != null && inner_list != null)
				return CreateJoinIterator (outer_list, inner_list, outerKeySelector, innerKeySelector, resultSelector, comparer);
			else if (outer_list != null)
				return CreateJoinIterator (outer_list, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
			else if (inner_list != null)
				return CreateJoinIterator (outer, inner_list, outerKeySelector, innerKeySelector, resultSelector, comparer);


			return CreateJoinIterator (outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
		}

		static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult> (IList<TOuter> outer,
			IList<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);

			for (int i = 0, c1 = outer.Count; i < c1; ++i) {
				TKey outerKey = outerKeySelector (outer [i]);
				if (outerKey != null && innerKeys.Contains (outerKey)) {
					for (int j = 0, c2 = inner.Count; j < c2; ++j)
						yield return resultSelector (outer [i], inner [j]);
				}
			}
		}

		static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult> (IList<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);

			for (int i = 0, c1 = outer.Count; i < c1; ++i) {
				TKey outerKey = outerKeySelector (outer [i]);
				if (outerKey != null && innerKeys.Contains (outerKey)) {
					foreach (TInner innerElement in innerKeys [outerKey])
						yield return resultSelector (outer [i], innerElement);
				}
			}
		}

		static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult> (IEnumerable<TOuter> outer,
			IList<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);

			foreach (TOuter outerElement in outer) {
				TKey outerKey = outerKeySelector (outerElement);
				if (outerKey != null && innerKeys.Contains (outerKey)) {
					for (int j = 0, c2 = inner.Count; j < c2; ++j)
						yield return resultSelector (outerElement, inner [j]);
				}
			}
		}

		static IEnumerable<TResult> CreateJoinIterator<TOuter, TInner, TKey, TResult> (IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
		{
			ILookup<TKey, TInner> innerKeys = ToLookup<TInner, TKey> (inner, innerKeySelector, comparer);
			/*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
			foreach (U element in inner)
			{
					K innerKey = innerKeySelector (element);
					if (!innerKeys.ContainsKey (innerKey))
							innerKeys.Add (innerKey, new List<U> ());
					innerKeys[innerKey].Add (element);
			}*/

			foreach (TOuter outerElement in outer) {
				TKey outerKey = outerKeySelector (outerElement);
				if (outerKey != null && innerKeys.Contains (outerKey)) {
					foreach (TInner innerElement in innerKeys [outerKey])
						yield return resultSelector (outerElement, innerElement);
				}
			}
		}

		public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult> (this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
		{
			return outer.Join (inner, outerKeySelector, innerKeySelector, resultSelector, null);
		}

		#endregion

		#region Last

		static TSource Last<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
		{
			var item = default (TSource);

			var list = source as IList<TSource>;
			if (list != null) {
				for (int c = list.Count, i = c - 1; i >= 0; --i)
					if (predicate (list [i]))
						return list [i];
			} else {
				var empty = true;
				foreach (var element in source) {
					if (predicate (element)) {
						item = element;
						empty = false;
					}
				}
				if (!empty)
					return item;
			}

			if (fallback == Fallback.Throw)
				throw NoMatchingElement ();

			return item;
		}

		public static TSource Last<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var collection = source as ICollection<TSource>;
			if (collection != null && collection.Count == 0)
				throw EmptySequence ();

			var list = source as IList<TSource>;
			if (list != null)
				return list [list.Count - 1];

#if !FULL_AOT_RUNTIME
			return source.Last (PredicateOf<TSource>.Always, Fallback.Throw);
#else
			var empty = true;
			var item = default (TSource);

			foreach (var element in source) {
				item = element;
				empty = false;
			}

			if (!empty)
				return item;

			throw EmptySequence ();
#endif
        }

		public static TSource Last<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.Last (predicate, Fallback.Throw);
		}

		#endregion

		#region LastOrDefault

		public static TSource LastOrDefault<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var list = source as IList<TSource>;
			if (list != null)
				return list.Count > 0 ? list [list.Count - 1] : default (TSource);

#if !FULL_AOT_RUNTIME
			return source.Last (PredicateOf<TSource>.Always, Fallback.Default);
#else
			var item = default (TSource);

			foreach (var element in source)
				item = element;

			return item;
#endif
		}

		public static TSource LastOrDefault<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.Last (predicate, Fallback.Default);
		}

		#endregion

		#region LongCount

		public static long LongCount<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

#if !NET_2_1
			var array = source as TSource [];
			if (array != null)
				return array.LongLength;
#endif

			long counter = 0;
			using (var enumerator = source.GetEnumerator ())
				while (enumerator.MoveNext ())
					counter++;

			return counter;
		}

		public static long LongCount<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndSelector (source, predicate);

			long counter = 0;

			var array = source as TSource [];
			if (array != null) {
				for (int i = 0; i < array.LongLength; ++i)
					if (predicate (array [i]))
						++counter;
			} else {
				foreach (TSource element in source)
					if (predicate (element))
						++counter;
			}

			return counter;
		}

		#endregion

		#region Max

		public static int Max (this IEnumerable<int> source)
		{
			Check.Source (source);

			var max = int.MinValue;

			var list = source as IList<int>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (list [i], max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (element, max);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return max;
		}

		public static long Max (this IEnumerable<long> source)
		{
			Check.Source (source);

			var max = long.MinValue;

			var list = source as IList<long>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (list [i], max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (element, max);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return max;
		}

		public static double Max (this IEnumerable<double> source)
		{
			Check.Source (source);

			var max = double.MinValue;

			var list = source as IList<double>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (list [i], max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (element, max);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return max;
		}

		public static float Max (this IEnumerable<float> source)
		{
			Check.Source (source);

			var max = float.MinValue;

			var list = source as IList<float>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (list [i], max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (element, max);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return max;
		}

		public static decimal Max (this IEnumerable<decimal> source)
		{
			Check.Source (source);

			var max = decimal.MinValue;

			var list = source as IList<decimal>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (list [i], max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (element, max);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return max;
		}

		public static int? Max (this IEnumerable<int?> source)
		{
			Check.Source (source);

			var max = int.MinValue;

			var list = source as IList<int?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (list [i].HasValue)
						max = Math.Max (list [i].Value, max);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (element.HasValue) {
						max = Math.Max (element.Value, max);
						empty = false;
					}
				}
				if (empty)
					return null;
			}

			return max;
		}

		public static long? Max (this IEnumerable<long?> source)
		{
			Check.Source (source);

			var max = long.MinValue;

			var list = source as IList<long?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (list [i].HasValue)
						max = Math.Max (list [i].Value, max);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (element.HasValue) {
						max = Math.Max (element.Value, max);
						empty = false;
					}
				}
				if (empty)
					return null;
			}

			return max;
		}

		public static double? Max (this IEnumerable<double?> source)
		{
			Check.Source (source);

			var max = double.MinValue;

			var list = source as IList<double?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (list [i].HasValue)
						max = Math.Max (list [i].Value, max);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (element.HasValue) {
						max = Math.Max (element.Value, max);
						empty = false;
					}
				}
				if (empty)
					return null;
			}

			return max;
		}

		public static float? Max (this IEnumerable<float?> source)
		{
			Check.Source (source);

			var max = float.MinValue;

			var list = source as IList<float?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (list [i].HasValue)
						max = Math.Max (list [i].Value, max);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (element.HasValue) {
						max = Math.Max (element.Value, max);
						empty = false;
					}
				}
				if (empty)
					return null;
			}

			return max;
		}

		public static decimal? Max (this IEnumerable<decimal?> source)
		{
			Check.Source (source);

			var max = decimal.MinValue;

			var list = source as IList<decimal?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (list [i].HasValue)
						max = Math.Max (list [i].Value, max);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (element.HasValue) {
						max = Math.Max (element.Value, max);
						empty = false;
					}
				}
				if (empty)
					return null;
			}

			return max;
		}

		// TODO: test nullable and non-nullable
		public static TSource Max<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var comparer = Comparer<TSource>.Default;
			var list = source as IList<TSource>;

			TSource max = default (TSource);
			
			if (default (TSource) == null) {
				if (list != null) {
					for (int i = 0, c = list.Count; i < c; i++) {
						if (list [i] == null)
							continue;
						if (max == null || comparer.Compare (list [i], max) > 0)
							max = list [i];
					}
				} else {
					foreach (var element in source) {
						if (element == null)
							continue;
						if (max == null || comparer.Compare (element, max) > 0)
							max = element;
					}
				}
			} else {
				if (list != null) {
					if (list.Count == 0)
						throw EmptySequence ();
					max = list [0];
					for (int i = 1, c = list.Count; i < c; i++) {
						if (comparer.Compare (list [i], max) > 0)
							max = list [i];
					}
				} else {
					bool empty = true;
					foreach (var element in source) {
						if (empty) {
							max = element;
							empty = false;
							continue;
						}
						if (comparer.Compare (element, max) > 0)
							max = element;
					}
					if (empty)
						throw EmptySequence ();
				}
			}

			return max;
		}

		public static int Max<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector (source, selector);

			var max = int.MinValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (selector (list [i]), max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (selector (element), max);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return max;
		}

		public static long Max<TSource> (this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector (source, selector);

			var max = long.MinValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (selector (list [i]), max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (selector (element), max);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return max;
		}

		public static double Max<TSource> (this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector (source, selector);

			var max = double.MinValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (selector (list [i]), max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (selector (element), max);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return max;
		}

		public static float Max<TSource> (this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector (source, selector);

			var max = float.MinValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (selector (list [i]), max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (selector (element), max);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return max;
		}

		public static decimal Max<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector (source, selector);

			var max = decimal.MinValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					max = Math.Max (selector (list [i]), max);
			} else {
				bool empty = true;
				foreach (var element in source){
					max = Math.Max (selector (element), max);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return max;
		}

		public static int? Max<TSource> (this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector (source, selector);

			int? max = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			} else {
				foreach (var element in source) {
					var item = selector (element);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			}

			return max;
		}

		public static long? Max<TSource> (this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector (source, selector);

			long? max = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			} else {
				foreach (var element in source) {
					var item = selector (element);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			}

			return max;
		}

		public static double? Max<TSource> (this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector (source, selector);

			double? max = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			} else {
				foreach (var element in source) {
					var item = selector (element);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			}

			return max;
		}

		public static float? Max<TSource> (this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector (source, selector);

			float? max = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			} else {
				foreach (var element in source) {
					var item = selector (element);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			}

			return max;
		}

		public static decimal? Max<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector (source, selector);

			decimal? max = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			} else {
				foreach (var element in source) {
					var item = selector (element);

					if (!max.HasValue)
						max = item;
					else if (item > max)
						max = item;
				}
			}

			return max;
		}

		public static TResult Max<TSource, TResult> (this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			Check.SourceAndSelector (source, selector);

			// TODO: inline
			return source.Select (selector).Max ();
		}

		#endregion

		#region Min

		public static int Min (this IEnumerable<int> source)
		{
			Check.Source (source);

			var min = int.MaxValue;

			var list = source as IList<int>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (list [i], min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (element, min);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return min;
		}

		public static long Min (this IEnumerable<long> source)
		{
			Check.Source (source);

			var min = long.MaxValue;

			var list = source as IList<long>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (list [i], min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (element, min);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return min;
		}

		public static double Min (this IEnumerable<double> source)
		{
			Check.Source (source);

			var min = double.MaxValue;

			var list = source as IList<double>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (list [i], min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (element, min);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return min;
		}

		public static float Min (this IEnumerable<float> source)
		{
			Check.Source (source);

			var min = float.MaxValue;

			var list = source as IList<float>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (list [i], min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (element, min);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return min;
		}

		public static decimal Min (this IEnumerable<decimal> source)
		{
			Check.Source (source);

			var min = decimal.MaxValue;

			var list = source as IList<decimal>;
			if (list != null) {
				if (list.Count == 0)
					throw EmptySequence ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (list [i], min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (element, min);
					empty = false;
				}
				if (empty)
					throw EmptySequence ();
			}

			return min;
		}

		public static int? Min (this IEnumerable<int?> source)
		{
			Check.Source (source);

			var min = int.MaxValue;

			var list = source as IList<int?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (!list [i].HasValue)
						continue;

					min = Math.Min (list [i].Value, min);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (!element.HasValue)
						continue;

					min = Math.Min (element.Value, min);
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static long? Min (this IEnumerable<long?> source)
		{
			Check.Source (source);

			var min = long.MaxValue;

			var list = source as IList<long?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (!list [i].HasValue)
						continue;

					min = Math.Min (list [i].Value, min);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (!element.HasValue)
						continue;

					min = Math.Min (element.Value, min);
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static double? Min (this IEnumerable<double?> source)
		{
			Check.Source (source);

			var min = double.MaxValue;

			var list = source as IList<double?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (!list [i].HasValue)
						continue;

					min = Math.Min (list [i].Value, min);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (!element.HasValue)
						continue;

					min = Math.Min (element.Value, min);
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static float? Min (this IEnumerable<float?> source)
		{
			Check.Source (source);

			var min = float.MaxValue;

			var list = source as IList<float?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (!list [i].HasValue)
						continue;

					min = Math.Min (list [i].Value, min);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (!element.HasValue)
						continue;

					min = Math.Min (element.Value, min);
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static decimal? Min (this IEnumerable<decimal?> source)
		{
			Check.Source (source);

			var min = decimal.MaxValue;

			var list = source as IList<decimal?>;
			if (list != null) {
				if (list.Count == 0)
					return null;
				for (int i = 0, c = list.Count; i < c; ++i) {
					if (!list [i].HasValue)
						continue;

					min = Math.Min (list [i].Value, min);
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					if (!element.HasValue)
						continue;

					min = Math.Min (element.Value, min);
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static TSource Min<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			var comparer = Comparer<TSource>.Default;
			var list = source as IList<TSource>;

			TSource min = default (TSource);

			if (default (TSource) == null) {
				if (list != null) {
					for (int i = 0, c = list.Count; i < c; i++) {
						if (list [i] == null)
							continue;
						if (min == null || comparer.Compare (list [i], min) < 0)
							min = list [i];
					}
				} else {
					foreach (var element in source) {
						if (element == null)
							continue;
						if (min == null || comparer.Compare (element, min) < 0)
							min = element;
					}
				}
			} else {
				if (list != null) {
					if (list.Count == 0)
						throw EmptySequence ();
					min = list [0];
					for (int i = 1, c = list.Count; i < c; i++) {
						if (comparer.Compare (list [i], min) < 0)
							min = list [i];
					}
				} else {
					bool empty = true;
					foreach (var element in source) {
						if (empty) {
							min = element;
							empty = false;
							continue;
						}
						if (comparer.Compare (element, min) < 0)
							min = element;
					}
					if (empty)
						throw EmptySequence ();
				}
			}

			return min;
		}

		public static int Min<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector (source, selector);

			var min = int.MaxValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (selector (list [i]), min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (selector (element), min);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return min;
		}

		public static long Min<TSource> (this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector (source, selector);

			var min = long.MaxValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (selector (list [i]), min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (selector (element), min);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return min;
		}

		public static double Min<TSource> (this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector (source, selector);

			var min = double.MaxValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (selector (list [i]), min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (selector (element), min);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return min;
		}

		public static float Min<TSource> (this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector (source, selector);

			var min = float.MaxValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (selector (list [i]), min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (selector (element), min);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return min;
		}

		public static decimal Min<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector (source, selector);

			var min = decimal.MaxValue;

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count == 0)
					throw NoMatchingElement ();
				for (int i = 0, c = list.Count; i < c; ++i)
					min = Math.Min (selector (list [i]), min);
			} else {
				bool empty = true;
				foreach (var element in source){
					min = Math.Min (selector (element), min);
					empty = false;
				}
				if (empty)
					throw NoMatchingElement ();
			}

			return min;
		}

		public static int? Min<TSource> (this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector (source, selector);

			int? min = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					var item = selector (element);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static long? Min<TSource> (this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector (source, selector);

			long? min = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					var item = selector (element);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static float? Min<TSource> (this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector (source, selector);

			float? min = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					var item = selector (element);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static double? Min<TSource> (this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector (source, selector);

			double? min = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					var item = selector (element);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static decimal? Min<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector (source, selector);

			decimal? min = null;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var item = selector (list [i]);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
				}
			} else {
				bool empty = true;
				foreach (var element in source){
					var item = selector (element);

					if (!min.HasValue)
						min = item;
					else if (item < min)
						min = item;
					empty = false;
				}
				if (empty)
					return null;
			}

			return min;
		}

		public static TResult Min<TSource, TResult> (this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			Check.SourceAndSelector (source, selector);

			// TODO: inline
			return source.Select (selector).Min ();
		}

		#endregion

		#region OfType

		public static IEnumerable<TResult> OfType<TResult> (this IEnumerable source)
		{
			Check.Source (source);

			var list = source as IList;
			if (list != null)
				return CreateOfTypeIterator<TResult> (list);

			return CreateOfTypeIterator<TResult> (source);
		}

		static IEnumerable<TResult> CreateOfTypeIterator<TResult> (IList source)
		{
			for (int i = 0, c = source.Count; i < c; i++)
				if (source [i] is TResult)
					yield return (TResult) source [i];
		}

		static IEnumerable<TResult> CreateOfTypeIterator<TResult> (IEnumerable source)
		{
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
			Check.SourceAndKeySelector (source, keySelector);

			return new OrderedSequence<TSource, TKey> (source, keySelector, comparer, SortDirection.Ascending);
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
			Check.SourceAndKeySelector (source, keySelector);

			return new OrderedSequence<TSource, TKey> (source, keySelector, comparer, SortDirection.Descending);
		}

		#endregion

		#region Range

		public static IEnumerable<int> Range (int start, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if (((long) start + count) - 1L > int.MaxValue)
				throw new ArgumentOutOfRangeException ();

			return CreateRangeIterator (start, count);
		}

		static IEnumerable<int> CreateRangeIterator (int start, int count)
		{
			for (int i = 0; i < count; i++)
				yield return start + i;
		}

		#endregion

		#region Repeat

		public static IEnumerable<TResult> Repeat<TResult> (TResult element, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ();

			return CreateRepeatIterator (element, count);
		}

		static IEnumerable<TResult> CreateRepeatIterator<TResult> (TResult element, int count)
		{
			for (int i = 0; i < count; i++)
				yield return element;
		}

		#endregion

		#region Reverse

		public static IEnumerable<TSource> Reverse<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			return CreateReverseIterator (source);
		}

		static IEnumerable<TSource> CreateReverseIterator<TSource> (IEnumerable<TSource> source)
		{
			var list = source as IList<TSource> ?? source.ToList ();

			for (int i = list.Count - 1; i >= 0; i--)
				yield return list [i];
		}

		#endregion

		#region Select

		public static IEnumerable<TResult> Select<TSource, TResult> (this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			Check.SourceAndSelector (source, selector);

			var array = source as TSource[];
			if (array != null)
				return CreateSelectIterator (array, selector);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSelectIterator (list, selector);

			return CreateSelectIterator (source, selector);
		}

		static IEnumerable<TResult> CreateSelectIterator<TSource, TResult> (TSource[] source, Func<TSource, TResult> selector)
		{
			for (int i = 0, c = source.Length; i < c; ++i)
				yield return selector (source [i]);
		}

		static IEnumerable<TResult> CreateSelectIterator<TSource, TResult> (IList<TSource> source, Func<TSource, TResult> selector)
		{
			for (int i = 0, c = source.Count; i < c; ++i)
				yield return selector (source [i]);
		}

		static IEnumerable<TResult> CreateSelectIterator<TSource, TResult> (IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			foreach (var element in source)
				yield return selector (element);
		}

		public static IEnumerable<TResult> Select<TSource, TResult> (this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
		{
			Check.SourceAndSelector (source, selector);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSelectIterator (list, selector);

			return CreateSelectIterator (source, selector);
		}

		static IEnumerable<TResult> CreateSelectIterator<TSource, TResult> (IList<TSource> source, Func<TSource, int, TResult> selector)
		{
			for (int i = 0, c = source.Count; i < c; ++i)
				yield return selector (source [i], i);
		}

		static IEnumerable<TResult> CreateSelectIterator<TSource, TResult> (IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
		{
			int counter = 0;
			foreach (TSource element in source) {
				yield return selector (element, counter);
				counter++;
			}
		}

		#endregion

		#region SelectMany

		public static IEnumerable<TResult> SelectMany<TSource, TResult> (this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			Check.SourceAndSelector (source, selector);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSelectManyIterator (list, selector);

			return CreateSelectManyIterator (source, selector);
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult> (IList<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				foreach (TResult item in selector (source [i]))
					yield return item;
			}
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult> (IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			foreach (TSource element in source)
				foreach (TResult item in selector (element))
					yield return item;
		}

		public static IEnumerable<TResult> SelectMany<TSource, TResult> (this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
		{
			Check.SourceAndSelector (source, selector);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSelectManyIterator (list, selector);

			return CreateSelectManyIterator (source, selector);
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult> (IList<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				foreach (TResult item in selector (source [i], i))
					yield return item;
			}
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TResult> (IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
		{
			int counter = 0;
			foreach (TSource element in source) {
				foreach (TResult item in selector (element, counter))
					yield return item;
				counter++;
			}
		}

		public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult> (this IEnumerable<TSource> source,
			Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			Check.SourceAndCollectionSelectors (source, collectionSelector, resultSelector);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSelectManyIterator (list, collectionSelector, resultSelector);

			return CreateSelectManyIterator (source, collectionSelector, resultSelector);
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult> (IList<TSource> source,
			Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				foreach (TCollection collection in collectionSelector (source [i]))
					yield return selector (source [i], collection);
			}
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult> (IEnumerable<TSource> source,
			Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			foreach (TSource element in source)
				foreach (TCollection collection in collectionSelector (element))
					yield return selector (element, collection);
		}

		public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult> (this IEnumerable<TSource> source,
			Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
		{
			Check.SourceAndCollectionSelectors (source, collectionSelector, resultSelector);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSelectManyIterator (list, collectionSelector, resultSelector);

			return CreateSelectManyIterator (source, collectionSelector, resultSelector);
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult> (IList<TSource> source,
			Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				foreach (TCollection collection in collectionSelector (source [i], i))
					yield return selector (source [i], collection);
			}
		}

		static IEnumerable<TResult> CreateSelectManyIterator<TSource, TCollection, TResult> (IEnumerable<TSource> source,
			Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> selector)
		{
			int counter = 0;
			foreach (TSource element in source)
				foreach (TCollection collection in collectionSelector (element, counter++))
					yield return selector (element, collection);
		}

		#endregion

		#region Single

		static TSource Single<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
		{
			var item = default (TSource);

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count > 1) {
					throw MoreThanOneMatchingElement ();
				} else if (list.Count == 0) {
					if (fallback == Fallback.Throw)
						throw NoMatchingElement ();
				} else {
					item = list [0];
				}
			} else {
				var found = false;
				foreach (var element in source) {
					if (!predicate (element))
						continue;

					if (found)
						throw MoreThanOneMatchingElement ();

					found = true;
					item = element;
				}

				if (!found && fallback == Fallback.Throw)
					throw NoMatchingElement ();
			}

			return item;
		}

		public static TSource Single<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

#if !FULL_AOT_RUNTIME
			return source.Single (PredicateOf<TSource>.Always, Fallback.Throw);
#else
			var item = default (TSource);

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count > 1)
					throw MoreThanOneMatchingElement ();
				else if (list.Count == 0)
					throw NoMatchingElement ();
				else
					item = list [0];
			} else {
				var found = false;
				foreach (var element in source) {
					if (found)
						throw MoreThanOneMatchingElement ();

					found = true;
					item = element;
				}
				if (!found)
					throw NoMatchingElement ();
			}

			return item;
#endif
        }

		public static TSource Single<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.Single (predicate, Fallback.Throw);
		}

		#endregion

		#region SingleOrDefault

		public static TSource SingleOrDefault<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

#if !FULL_AOT_RUNTIME
			return source.Single (PredicateOf<TSource>.Always, Fallback.Default);
#else
			var item = default (TSource);

			var list = source as IList<TSource>;
			if (list != null) {
				if (list.Count > 1)
					throw MoreThanOneMatchingElement ();
				if (list.Count == 1)
					item = list [0];
			} else {
				var found = false;
				foreach (var element in source) {
					if (found)
						throw MoreThanOneMatchingElement ();

					found = true;
					item = element;
				}
			}

			return item;
#endif
        }

		public static TSource SingleOrDefault<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return source.Single (predicate, Fallback.Default);
		}

		#endregion

		#region Skip

		public static IEnumerable<TSource> Skip<TSource> (this IEnumerable<TSource> source, int count)
		{
			Check.Source (source);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSkipIterator (list, count);

			return CreateSkipIterator (source, count);
		}

		static IEnumerable<TSource> CreateSkipIterator<TSource> (IList<TSource> source, int count)
		{
			for (int i = count, c = source.Count; i < c; ++i)
				yield return source [i];
		}

		static IEnumerable<TSource> CreateSkipIterator<TSource> (IEnumerable<TSource> source, int count)
		{
			var enumerator = source.GetEnumerator ();
			try {
				while (count-- > 0)
					if (!enumerator.MoveNext ())
						yield break;

				while (enumerator.MoveNext ())
					yield return enumerator.Current;

			} finally {
				enumerator.Dispose ();
			}
		}

		#endregion

		#region SkipWhile

		public static IEnumerable<TSource> SkipWhile<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateSkipWhileIterator (list, predicate);

			return CreateSkipWhileIterator (source, predicate);
		}

		static IEnumerable<TSource> CreateSkipWhileIterator<TSource> (IList<TSource> source, Func<TSource, bool> predicate)
		{
			int i, c = source.Count;
			for (i = 0; i < c; ++i)
				if (!predicate (source [i]))
					break;
			for (; i < c; ++i)
				yield return source [i];
		}

		static IEnumerable<TSource> CreateSkipWhileIterator<TSource> (IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
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

		public static IEnumerable<TSource> SkipWhile<TSource> (this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			return CreateSkipWhileIterator (source, predicate);
		}

		static IEnumerable<TSource> CreateSkipWhileIterator<TSource> (IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
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
			Check.Source (source);


			int total = 0;

			var list = source as IList<int>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source)
					total = checked (total + element);
			}

			return total;
		}

		public static int? Sum (this IEnumerable<int?> source)
		{
			Check.Source (source);

			int total = 0;

			var list = source as IList<int?>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source)
					if (element.HasValue)
						total = checked (total + element.Value);
			}

			return total;
		}

		public static int Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, int> selector)
		{
			Check.SourceAndSelector (source, selector);

			int total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source)
					total = checked (total + selector (element));
			}

			return total;
		}

		public static int? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, int?> selector)
		{
			Check.SourceAndSelector (source, selector);

			int total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var value = selector (list [i]);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			} else {
				foreach (var element in source) {
					var value = selector (element);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			}

			return total;
		}

		public static long Sum (this IEnumerable<long> source)
		{
			Check.Source (source);

			long total = 0;

			var list = source as IList<long>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + list [i]);
				return total;
			} else {
				foreach (var element in source)
					total = checked (total + element);
			}

			return total;
		}

		public static long? Sum (this IEnumerable<long?> source)
		{
			Check.Source (source);

			long total = 0;

			var list = source as IList<long?>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source)
					if (element.HasValue)
						total = checked (total + element.Value);
			}

			return total;
		}

		public static long Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, long> selector)
		{
			Check.SourceAndSelector (source, selector);

			long total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source)
					total = checked (total + selector (element));
			}

			return total;
		}

		public static long? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, long?> selector)
		{
			Check.SourceAndSelector (source, selector);

			long total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var value = selector (list [i]);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			} else {
				foreach (var element in source) {
					var value = selector (element);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			}

			return total;
		}

		public static double Sum (this IEnumerable<double> source)
		{
			Check.Source (source);

			double total = 0;

			var list = source as IList<double>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source)
					total = checked (total + element);
			}

			return total;
		}

		public static double? Sum (this IEnumerable<double?> source)
		{
			Check.Source (source);

			double total = 0;

			var list = source as IList<double?>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source)
					if (element.HasValue)
						total = checked (total + element.Value);
			}

			return total;
		}

		public static double Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, double> selector)
		{
			Check.SourceAndSelector (source, selector);

			double total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source)
					total = checked (total + selector (element));
			}

			return total;
		}

		public static double? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, double?> selector)
		{
			Check.SourceAndSelector (source, selector);

			double total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var value = selector (list [i]);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			} else {
				foreach (var element in source) {
					var value = selector (element);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			}

			return total;
		}

		public static float Sum (this IEnumerable<float> source)
		{
			Check.Source (source);

			float total = 0;

			var list = source as IList<float>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source)
					total = checked (total + element);
			}

			return total;
		}

		public static float? Sum (this IEnumerable<float?> source)
		{
			Check.Source (source);

			float total = 0;

			var list = source as IList<float?>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source)
					if (element.HasValue)
						total = checked (total + element.Value);
			}

			return total;

		}

		public static float Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, float> selector)
		{
			Check.SourceAndSelector (source, selector);

			float total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source)
					total = checked (total + selector (element));
			}

			return total;
		}

		public static float? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, float?> selector)
		{
			Check.SourceAndSelector (source, selector);

			float total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var value = selector (list [i]);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			} else {
				foreach (var element in source) {
					var value = selector (element);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			}

			return total;
		}

		public static decimal Sum (this IEnumerable<decimal> source)
		{
			Check.Source (source);

			decimal total = 0;

			var list = source as IList<decimal>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + list [i]);
			} else {
				foreach (var element in source)
					total = checked (total + element);
			}

			return total;
		}

		public static decimal? Sum (this IEnumerable<decimal?> source)
		{
			Check.Source (source);

			decimal total = 0;

			var list = source as IList<decimal?>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					if (list [i].HasValue)
						total = checked (total + list [i].Value);
			} else {
				foreach (var element in source)
					if (element.HasValue)
						total = checked (total + element.Value);
			}

			return total;

		}

		public static decimal Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal> selector)
		{
			Check.SourceAndSelector (source, selector);

			decimal total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i)
					total = checked (total + selector (list [i]));
			} else {
				foreach (var element in source)
					total = checked (total + selector (element));
			}

			return total;
		}

		public static decimal? Sum<TSource> (this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			Check.SourceAndSelector (source, selector);

			decimal total = 0;

			var list = source as IList<TSource>;
			if (list != null) {
				for (int i = 0, c = list.Count; i < c; ++i) {
					var value = selector (list [i]);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			} else {
				foreach (var element in source) {
					var value = selector (element);
					if (value.HasValue)
						total = checked (total + value.Value);
				}
			}

			return total;
		}
		
		#endregion

		#region Take

		public static IEnumerable<TSource> Take<TSource> (this IEnumerable<TSource> source, int count)
		{
			Check.Source (source);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateTakeIterator (list, count);

			return CreateTakeIterator (source, count);
		}

		static IEnumerable<TSource> CreateTakeIterator<TSource> (IList<TSource> source, int count)
		{
			for (int i = 0, c = source.Count; i < c && i < count; ++i)
				yield return source [i];
		}

		static IEnumerable<TSource> CreateTakeIterator<TSource> (IEnumerable<TSource> source, int count)
		{
			if (count <= 0)
				yield break;

			int counter = 0;
			foreach (TSource element in source) {
				yield return element;

				if (++counter == count)
					yield break;
			}
		}

		#endregion

		#region TakeWhile

		public static IEnumerable<TSource> TakeWhile<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateTakeWhileIterator (list, predicate);

			return CreateTakeWhileIterator (source, predicate);
		}

		static IEnumerable<TSource> CreateTakeWhileIterator<TSource> (IList<TSource> source, Func<TSource, bool> predicate)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				if (!predicate (source [i]))
					yield break;
				yield return source [i];
			}
		}

		static IEnumerable<TSource> CreateTakeWhileIterator<TSource> (IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			foreach (var element in source) {
				if (!predicate (element))
					yield break;

				yield return element;
			}
		}

		public static IEnumerable<TSource> TakeWhile<TSource> (this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateTakeWhileIterator (list, predicate);

			return CreateTakeWhileIterator (source, predicate);
		}

		static IEnumerable<TSource> CreateTakeWhileIterator<TSource> (IList<TSource> source, Func<TSource, int, bool> predicate)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				if (!predicate (source [i], i))
					yield break;
				yield return source [i];
			}
		}

		static IEnumerable<TSource> CreateTakeWhileIterator<TSource> (IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
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
			Check.SourceAndKeySelector (source, keySelector);

#if FULL_AOT_RUNTIME
			var oe = source as OrderedEnumerable <TSource>;
			if (oe != null)
				return oe.CreateOrderedEnumerable (keySelector, comparer, false);
#endif

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
			Check.SourceAndKeySelector (source, keySelector);

#if FULL_AOT_RUNTIME
			var oe = source as OrderedEnumerable <TSource>;
			if (oe != null)
				return oe.CreateOrderedEnumerable (keySelector, comparer, true);
#endif
			return source.CreateOrderedEnumerable (keySelector, comparer, true);
		}

		#endregion

		#region ToArray

		public static TSource [] ToArray<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			TSource[] array;
			var collection = source as ICollection<TSource>;
			if (collection != null) {
				if (collection.Count == 0)
					return EmptyOf<TSource>.Instance;
				
				array = new TSource [collection.Count];
				collection.CopyTo (array, 0);
				return array;
			}

			int pos = 0;
			array = EmptyOf<TSource>.Instance;
			foreach (var element in source) {
				if (pos == array.Length) {
					if (pos == 0)
						array = new TSource [4];
					else
						Array.Resize (ref array, pos * 2);
				}

				array[pos++] = element;
			}

			if (pos != array.Length)
				Array.Resize (ref array, pos);
			
			return array;
		}

		#endregion

		#region ToDictionary
		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return ToDictionary<TSource, TKey, TElement> (source, keySelector, elementSelector, null);
		}

		public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyElementSelectors (source, keySelector, elementSelector);

			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			var dict = new Dictionary<TKey, TElement> (comparer);
			foreach (var e in source)
				dict.Add (keySelector (e), elementSelector (e));

			return dict;
		}

		public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector)
		{
			return ToDictionary (source, keySelector, null);
		}

		public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey> (this IEnumerable<TSource> source,
				Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			return ToDictionary<TSource, TKey, TSource> (source, keySelector, Function<TSource>.Identity, comparer);
		}

		#endregion

		#region ToList
		public static List<TSource> ToList<TSource> (this IEnumerable<TSource> source)
		{
			Check.Source (source);

			return new List<TSource> (source);
		}
		#endregion

		#region ToLookup

		public static ILookup<TKey, TSource> ToLookup<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			return ToLookup<TSource, TKey, TSource> (source, keySelector, Function<TSource>.Identity, null);
		}

		public static ILookup<TKey, TSource> ToLookup<TSource, TKey> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
		{
			return ToLookup<TSource, TKey, TSource> (source, keySelector, Function<TSource>.Identity, comparer);
		}

		public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			return ToLookup<TSource, TKey, TElement> (source, keySelector, elementSelector, null);
		}

		public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement> (this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
		{
			Check.SourceAndKeyElementSelectors (source, keySelector, elementSelector);

			List<TElement> nullKeyElements = null;
			
			var dictionary = new Dictionary<TKey, List<TElement>> (comparer ?? EqualityComparer<TKey>.Default);
			foreach (var element in source) {
				var key = keySelector (element);

				List<TElement> list;
				
				if (key == null) {
					if (nullKeyElements == null)
						nullKeyElements = new List<TElement> ();
					
					list = nullKeyElements;
				} else if (!dictionary.TryGetValue (key, out list)) {
					list = new List<TElement> ();
					dictionary.Add (key, list);
				}

				list.Add (elementSelector (element));
			}

			return new Lookup<TKey, TElement> (dictionary, nullKeyElements);
		}

		#endregion

		#region SequenceEqual

		public static bool SequenceEqual<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return first.SequenceEqual (second, null);
		}

		public static bool SequenceEqual<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			using (IEnumerator<TSource> first_enumerator = first.GetEnumerator (),
				second_enumerator = second.GetEnumerator ()) {

				while (first_enumerator.MoveNext ()) {
					if (!second_enumerator.MoveNext ())
						return false;

					if (!comparer.Equals (first_enumerator.Current, second_enumerator.Current))
						return false;
				}

				return !second_enumerator.MoveNext ();
			}
		}

		#endregion

		#region Union

		public static IEnumerable<TSource> Union<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			Check.FirstAndSecond (first, second);

			return first.Union (second, null);
		}

		public static IEnumerable<TSource> Union<TSource> (this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			Check.FirstAndSecond (first, second);

			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			var first_list = first as IList<TSource>;
			var second_list = second as IList<TSource>;
			if (first_list != null && second_list != null)
				return CreateUnionIterator (first_list, second_list, comparer);
			else if (first_list != null)
				return CreateUnionIterator (first_list, second, comparer);
			else if (second_list != null)
				return CreateUnionIterator (first, second_list, comparer);

			return CreateUnionIterator (first, second, comparer);
		}

		static IEnumerable<TSource> CreateUnionIterator<TSource> (IList<TSource> first, IList<TSource> second, IEqualityComparer<TSource> comparer)
		{
			int i, c;
			var items = new HashSet<TSource> (comparer);

			for (i = 0, c = first.Count; i < c; ++i) {
				if (!items.Contains (first [i])) {
					items.Add (first [i]);
					yield return first [i];
				}
			}

			for (i = 0, c = second.Count; i < c; ++i) {
				if (!items.Contains (second [i])) {
					items.Add (second [i]);
					yield return second [i];
				}
			}
		}

		static IEnumerable<TSource> CreateUnionIterator<TSource> (IList<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (comparer);

			for (int i = 0, c = first.Count; i < c; ++i) {
				if (!items.Contains (first [i])) {
					items.Add (first [i]);
					yield return first [i];
				}
			}

			foreach (var element in second) {
				if (! items.Contains (element)) {
					items.Add (element);
					yield return element;
				}
			}
		}

		static IEnumerable<TSource> CreateUnionIterator<TSource> (IEnumerable<TSource> first, IList<TSource> second, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (comparer);

			foreach (var element in first) {
				if (! items.Contains (element)) {
					items.Add (element);
					yield return element;
				}
			}

			for (int i = 0, c = second.Count; i < c; ++i) {
				if (!items.Contains (second [i])) {
					items.Add (second [i]);
					yield return second [i];
				}
			}
		}

		static IEnumerable<TSource> CreateUnionIterator<TSource> (IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
		{
			var items = new HashSet<TSource> (comparer);
			foreach (var element in first) {
				if (! items.Contains (element)) {
					items.Add (element);
					yield return element;
				}
			}

			foreach (var element in second) {
				if (! items.Contains (element)) {
					items.Add (element);
					yield return element;
				}
			}
		}

		#endregion
		
#if NET_4_0
		#region Zip
		
		public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult> (this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			Check.FirstAndSecond (first, second);
			if (resultSelector == null)
				throw new ArgumentNullException ("resultSelector");
				
			var first_list = first as IList<TFirst>;
			var second_list = second as IList<TSecond>;
			if (first_list != null && second_list != null)
				return CreateZipIterator (first_list, second_list, resultSelector);
			else if (first_list != null)
				return CreateZipIterator (first_list, second, resultSelector);
			else if (second_list != null)
				return CreateZipIterator (first, second_list, resultSelector);

			return CreateZipIterator (first, second, resultSelector);
		}
		
		static IEnumerable<TResult> CreateZipIterator<TFirst, TSecond, TResult> (IList<TFirst> first, IList<TSecond> second, Func<TFirst, TSecond, TResult> selector)
		{
			for (int i = 0, c1 = first.Count, c2 = second.Count; i < c1 && i < c2; ++i)
				yield return selector (first [i], second [i]);
		}
		
		static IEnumerable<TResult> CreateZipIterator<TFirst, TSecond, TResult> (IList<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
		{
			using (IEnumerator<TSecond> second_enumerator = second.GetEnumerator ()) {
				for (int i = 0, c = first.Count; i < c && second_enumerator.MoveNext (); ++i)
					yield return selector (first [i], second_enumerator.Current);
			}
		}
		
		static IEnumerable<TResult> CreateZipIterator<TFirst, TSecond, TResult> (IEnumerable<TFirst> first, IList<TSecond> second, Func<TFirst, TSecond, TResult> selector)
		{
			using (IEnumerator<TFirst> first_enumerator = first.GetEnumerator ()) {
				for (int i = 0, c = second.Count; first_enumerator.MoveNext () && i < c; ++i)
					yield return selector (first_enumerator.Current, second [i]);
			}
		}
		
		static IEnumerable<TResult> CreateZipIterator<TFirst, TSecond, TResult> (IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
		{
			using (IEnumerator<TFirst> first_enumerator = first.GetEnumerator ()) {
				using (IEnumerator<TSecond> second_enumerator = second.GetEnumerator ()) {

					while (first_enumerator.MoveNext () && second_enumerator.MoveNext ()) {
						yield return selector (first_enumerator.Current, second_enumerator.Current);
					}
				}
			}
		}
		
		#endregion
#endif		

		#region Where

		public static IEnumerable<TSource> Where<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var array = source as TSource[];
			if (array != null)
				return CreateWhereIterator (array, predicate);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateWhereIterator (list, predicate);

			return CreateWhereIterator (source, predicate);
		}

		static IEnumerable<TSource> CreateWhereIterator<TSource> (TSource[] source, Func<TSource, bool> predicate)
		{
			for (int i = 0; i < source.Length; ++i)
				if (predicate (source [i]))
					yield return source [i];
		}

		static IEnumerable<TSource> CreateWhereIterator<TSource> (IList<TSource> source, Func<TSource, bool> predicate)
		{
			for (int i = 0, c = source.Count; i < c; ++i)
				if (predicate (source [i]))
					yield return source [i];
		}

		static IEnumerable<TSource> CreateWhereIterator<TSource> (IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			foreach (TSource element in source)
				if (predicate (element))
					yield return element;
		}	

		public static IEnumerable<TSource> Where<TSource> (this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			Check.SourceAndPredicate (source, predicate);

			var list = source as IList<TSource>;
			if (list != null)
				return CreateWhereIterator (list, predicate);

			return CreateWhereIterator (source, predicate);
		}

		static IEnumerable<TSource> CreateWhereIterator<TSource> (IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
		{
			int counter = 0;
			foreach (TSource element in source) {
				if (predicate (element, counter))
					yield return element;
				counter++;
			}
		}

		static IEnumerable<TSource> CreateWhereIterator<TSource> (IList<TSource> source, Func<TSource, int, bool> predicate)
		{
			for (int i = 0, c = source.Count; i < c; ++i) {
				if (predicate (source [i], i))
					yield return source [i];
			}
		}

		#endregion

		internal static ReadOnlyCollection<TSource> ToReadOnlyCollection<TSource> (this IEnumerable<TSource> source)
		{
			if (source == null)
				return ReadOnlyCollectionOf<TSource>.Empty;

			var ro = source as ReadOnlyCollection<TSource>;
			if (ro != null)
				return ro;

			return new ReadOnlyCollection<TSource> (source.ToArray<TSource> ());
		}

		#region Exception helpers

		static Exception EmptySequence ()
		{
			return new InvalidOperationException (Locale.GetText ("Sequence contains no elements"));
		}
		static Exception NoMatchingElement ()
		{
			return new InvalidOperationException (Locale.GetText ("Sequence contains no matching element"));
		}
		static Exception MoreThanOneElement ()
		{
			return new InvalidOperationException (Locale.GetText ("Sequence contains more than one element"));
		}
		static Exception MoreThanOneMatchingElement ()
		{
			return new InvalidOperationException (Locale.GetText ("Sequence contains more than one matching element"));
		}

		#endregion
	}
}
