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
		public void Visit (QueryBaseNode node)
		{

		}

		public void Visit (QueryChildNode node)
		{
			node.Parent.Visit (this);
		}

		public void Visit (QueryOptionNode node)
		{
			Visit ((QueryChildNode)node);
		}

		public void Visit (QueryStartNode node)
		{
		}

		public void Visit (QueryStreamNode node)
		{
			Visit ((QueryChildNode)node);
		}

		public void Visit (QueryOrderGuardNode node)
		{
			BehindOrderGuard = node.EnsureOrder;
		}

		public void Visit (QueryMuxNode node)
		{
			Visit ((QueryChildNode)node);
		}

		public void Visit (QueryHeadWorkerNode node)
		{
			Visit ((QueryStreamNode)node);
		}
		#endregion
	}
}
#endif
