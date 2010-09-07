//
// QueryStartNode.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq.Parallel.QueryNodes
{
	internal class QueryStartNode<T> : QueryBaseNode<T>
	{
		readonly IEnumerable<T> source;
		readonly Partitioner<T> customPartitioner;

		internal QueryStartNode (IEnumerable<T> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			this.source = source;
		}

		internal QueryStartNode (Partitioner<T> custom)
		{
			if (custom == null)
				throw new ArgumentNullException ("custom");

			this.customPartitioner = custom;
		}

		// If possible, this property will return the number of element the query
		// is going to process. If that number if pretty low, executing the query
		// sequentially is better
		internal int Count {
			get {
				if (source == null)
					return -1;

				ICollection coll = source as ICollection;
				return coll == null ? -1 : coll.Count;
			}
		}

		public override void Visit (INodeVisitor visitor)
		{
			visitor.Visit<T> (this);
		}

		internal override IEnumerable<T> GetSequential ()
		{
			if (source != null)
				return source;

			return WrapHelper.Wrap (customPartitioner.GetPartitions (1))[0];
		}

		internal override IList<IEnumerable<T>> GetEnumerables (QueryOptions options)
		{
			if (customPartitioner != null) {
				return WrapHelper.Wrap (customPartitioner.GetPartitions (options.PartitionCount));
			}

			Partitioner<T> partitioner
				= (options.UseStrip) ? ParallelPartitioner.CreateForStrips (source, 1) : ParallelPartitioner.CreateBest (source);

			return WrapHelper.Wrap (partitioner.GetPartitions (options.PartitionCount));
		}

		internal override IList<IEnumerable<KeyValuePair<long, T>>> GetOrderedEnumerables (QueryOptions options)
		{
			OrderablePartitioner<T> partitioner = null;
			if (customPartitioner != null) {
				partitioner = customPartitioner as OrderablePartitioner<T>;
				if (partitioner == null)
					throw new InvalidOperationException ("The partitionner you are using doesn't support ordered partitionning");
			} else {
				partitioner =
					(options.UseStrip) ? ParallelPartitioner.CreateForStrips (source, 1) : ParallelPartitioner.CreateBest (source);
			}

			options.PartitionerSettings = Tuple.Create (partitioner.KeysOrderedAcrossPartitions,
			                                            partitioner.KeysOrderedInEachPartition,
			                                            partitioner.KeysNormalized);

			// We only support one style of partitioning at the moment.
			// Standard partitioners follow this style.
			if (options.UseStrip && (!partitioner.KeysOrderedInEachPartition || partitioner.KeysOrderedAcrossPartitions))
				throw new NotImplementedException ("Partitioner must have KeysOrderedInEachPartition "
				                                   + "and !KeysOrderedAcrossPartitions"
				                                   + "to be used with indexed operators");

			return WrapHelper.Wrap (partitioner.GetOrderablePartitions (options.PartitionCount));
		}
	}
}
#endif