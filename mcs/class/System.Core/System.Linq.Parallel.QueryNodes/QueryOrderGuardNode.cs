// QueryOrderGuardNode.cs
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
using System.Collections.Generic;

namespace System.Linq.Parallel.QueryNodes
{
	internal interface QueryOrderGuardNode : IVisitableNode {
		bool EnsureOrder { get; }
	}

	internal abstract class QueryOrderGuardNode<T> : QueryStreamNode<T, T>, QueryOrderGuardNode
	{
		bool ensureOrder;

		internal QueryOrderGuardNode (QueryBaseNode<T> parent, bool ensureOrder)
			: base (parent, ensureOrder)
		{
			this.ensureOrder = ensureOrder;
		}

		public bool EnsureOrder {
			get {
				return ensureOrder;
			}
		}

		internal override IEnumerable<T> GetSequential ()
		{
			return Parent.GetSequential ();
		}

		public override void Visit (INodeVisitor visitor)
		{
			visitor.Visit ((QueryOrderGuardNode)this);
		}
	}

	internal class QueryAsUnorderedNode<T> : QueryOrderGuardNode<T>
	{
		internal QueryAsUnorderedNode (QueryBaseNode<T> parent)
			: base (parent, false)
		{

		}

		internal override IList<IEnumerable<T>> GetEnumerables (QueryOptions options)
		{
			return Parent.GetEnumerables (options);
		}

		internal override IList<IEnumerable<KeyValuePair<long, T>>> GetOrderedEnumerables (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options);
		}

	}

	internal class QueryAsOrderedNode<T> : QueryOrderGuardNode<T>
	{
		internal QueryAsOrderedNode (QueryBaseNode<T> parent)
			: base (parent, true)
		{

		}

		internal override IList<IEnumerable<T>> GetEnumerables (QueryOptions options)
		{
			return Parent.GetEnumerables (options);
		}

		internal override IList<IEnumerable<KeyValuePair<long, T>>> GetOrderedEnumerables (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options);
		}

	}
}
#endif
