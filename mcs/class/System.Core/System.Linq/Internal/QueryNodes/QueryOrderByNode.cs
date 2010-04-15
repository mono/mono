#if NET_4_0
//
// QueryOrderByNode.cs
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

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq
{
	internal class QueryOrderByNode<T> : QueryOrderGuardNode<T>
	{
		Comparison<T> comparison;

		public QueryOrderByNode (QueryBaseNode<T> parent, Comparison<T> comparison)
			: base (parent, true)
		{
			this.comparison = comparison;
		}


		public QueryOrderByNode (QueryOrderByNode<T> parent, Comparison<T> comparison)
			: base (parent.Parent, true)
		{
			this.comparison = MergeComparison (parent.ComparisonFunc, comparison);
		}

		public Comparison<T> ComparisonFunc {
			get {
				return comparison;
			}
		}

		internal override IEnumerable<T> GetSequential ()
		{
			return Parent.GetSequential ().OrderBy ((e) => e, new ComparisonComparer (comparison));
		}

		private class ComparisonComparer : IComparer<T>
		{
			Comparison<T> comparison;

			internal ComparisonComparer (Comparison<T> comparison)
			{
				this.comparison = comparison;
			}

			int IComparer<T>.Compare (T x, T y)
			{
				return comparison (x, y);
			}
		}

		internal override IList<IEnumerable<T>> GetEnumerables (QueryOptions options)
		{
			throw new InvalidOperationException ("Shouldn't be called");
		}

		internal override IList<IEnumerable<KeyValuePair<long, T>>> GetOrderedEnumerables (QueryOptions options)
		{
			int partitionCount;
			IList<T> aggregList = GetAggregatedList (out partitionCount);
			IList<T> result = ParallelQuickSort<T>.Sort (aggregList, comparison);

			OrderablePartitioner<T> partitioner = ParallelPartitioner.CreateForStrips (result, 1);

			return WrapHelper.Wrap (partitioner.GetOrderablePartitions (options.PartitionCount));
		}

		IList<T> GetAggregatedList (out int partitionCount)
		{
			AggregationList<T> result = null;
			partitionCount = -1;

			ParallelExecuter.ProcessAndAggregate<T, IList<T>> (Parent, () => new List<T> (),
			                                                   LocalCall,
			                                                   (ls) => { result = new AggregationList<T> (ls); });

			return result;
		}

		IList<T> LocalCall (IList<T> list, T element)
		{
			list.Add (element);
			return list;
		}

		static Comparison<T> MergeComparison (Comparison<T> source, Comparison<T> other)
		{
			return (e1, e2) => {
				int result = source (e1, e2);
				return result == 0 ? other (e1, e2) : result;
			};
		}
	}
}
#endif
