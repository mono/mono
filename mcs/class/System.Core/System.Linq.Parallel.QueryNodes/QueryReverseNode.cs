//
// QueryReverseNode.cs
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
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq
{
	internal class QueryReverseNode<TSource> : QueryStreamNode<TSource, TSource>
	{
		ParallelQuery<TSource> source;

		public QueryReverseNode (ParallelQuery<TSource> source)
			: base (source.Node, false)
		{
			this.source = source;
		}

		internal override IEnumerable<TSource> GetSequential ()
		{
			return Parent.GetSequential ().Reverse ();
		}

		// As stated in the doc, in this case we do nothing
		internal override IList<IEnumerable<TSource>> GetEnumerables (QueryOptions options)
		{
			return Parent.GetEnumerables (options);
		}

		internal override IList<IEnumerable<KeyValuePair<long, TSource>>> GetOrderedEnumerables (QueryOptions options)
		{
			ReverseList<TSource> reversed = new ReverseList<TSource> (source.ToListOrdered ());
			OrderablePartitioner<TSource> partitioner = ParallelPartitioner.CreateForStrips (reversed, 1);

			return WrapHelper.Wrap (partitioner.GetOrderablePartitions (options.PartitionCount));
		}
	}
}

#endif
