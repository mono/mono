//
// QueryIsOrderedVisitor.cs
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
using System.Linq.Parallel.QueryNodes;

namespace System.Linq.Parallel
{
	internal class QueryIsOrderedVisitor : INodeVisitor
	{
		internal bool BehindOrderGuard {
			get;
			private set;
		}

		#region INodeVisitor implementation
		public void Visit<T> (QueryBaseNode<T> node)
		{

		}

		public void Visit<U, V> (QueryChildNode<U, V> node)
		{
			node.Parent.Visit (this);
		}

		public void Visit<T> (QueryOptionNode<T> node)
		{
			Visit<T, T> ((QueryChildNode<T, T>)node);
		}

		public void Visit<T> (QueryStartNode<T> node)
		{
		}

		public void Visit<T, TParent> (QueryStreamNode<T, TParent> node)
		{
			Visit<T, TParent> ((QueryChildNode<T, TParent>)node);
		}

		public void Visit<T> (QueryOrderGuardNode<T> node)
		{
			BehindOrderGuard = node.EnsureOrder;
		}

		public void Visit<TFirst, TSecond, TResult> (QueryMuxNode<TFirst, TSecond, TResult> node)
		{
			Visit<TResult, TFirst> ((QueryChildNode<TResult, TFirst>)node);
		}

		public void Visit<T> (QueryHeadWorkerNode<T> node)
		{
			Visit<T, T> ((QueryStreamNode<T, T>)node);
		}
		#endregion
	}
}
#endif
