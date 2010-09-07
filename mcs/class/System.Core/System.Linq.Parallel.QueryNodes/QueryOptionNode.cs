//
// QueryOptionNode.cs
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
using System.Collections.Generic;

namespace System.Linq.Parallel.QueryNodes
{
	// The first four elements correspond to the public operator With*
	// Last CancellationToken parameter is used internally for ImplementerToken
	using OptionsList = Tuple<ParallelMergeOptions?, ParallelExecutionMode?, CancellationToken?, int, CancellationTokenSource>;

	internal class QueryOptionNode<T> : QueryChildNode<T, T>
	{

		public QueryOptionNode (QueryBaseNode<T> parent)
			: base (parent)
		{

		}

		internal virtual OptionsList GetOptions ()
		{
			return new OptionsList (null, null, null, -1, null);
		}

		internal override IList<IEnumerable<T>> GetEnumerables (QueryOptions options)
		{
			return Parent.GetEnumerables (options);
		}

		internal override IList<IEnumerable<KeyValuePair<long, T>>> GetOrderedEnumerables (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options);
		}

		internal override IEnumerable<T> GetSequential ()
		{
			return Parent.GetSequential ();
		}

		public override void Visit (INodeVisitor visitor)
		{
			visitor.Visit<T> (this);
		}
	}

	internal class ParallelExecutionModeNode<T> : QueryOptionNode<T>
	{
		ParallelExecutionMode mode;

		internal ParallelExecutionModeNode (ParallelExecutionMode mode, QueryBaseNode<T> parent)
			: base (parent)
		{
			this.mode = mode;
		}

		internal override OptionsList GetOptions ()
		{
			return new OptionsList (null, mode, null, -1, null);
		}
	}


	internal class ParallelMergeOptionsNode<T> : QueryOptionNode<T>
	{
		ParallelMergeOptions opts;

		internal ParallelMergeOptionsNode (ParallelMergeOptions opts, QueryBaseNode<T> parent)
			: base (parent)
		{
			this.opts = opts;
		}

		internal override OptionsList GetOptions ()
		{
			return new OptionsList (opts, null, null, -1, null);
		}
	}


	internal class CancellationTokenNode<T> : QueryOptionNode<T>
	{
		CancellationToken token;

		internal CancellationTokenNode (CancellationToken token, QueryBaseNode<T> parent)
			: base (parent)
		{
			this.token = token;
		}

		internal override OptionsList GetOptions ()
		{
			return new OptionsList (null, null, token, -1, null);
		}
	}

	internal class DegreeOfParallelismNode<T> : QueryOptionNode<T>
	{
		int degreeParallelism;

		internal DegreeOfParallelismNode (int degreeParallelism, QueryBaseNode<T> parent)
			: base (parent)
		{
			this.degreeParallelism = degreeParallelism;
		}

		internal override OptionsList GetOptions ()
		{
			return new OptionsList (null, null, null, degreeParallelism, null);
		}
	}

	internal class ImplementerTokenNode<T> : QueryOptionNode<T>
	{
		CancellationTokenSource source;

		internal ImplementerTokenNode (CancellationTokenSource token, QueryBaseNode<T> parent)
			: base (parent)
		{
			this.source = token;
		}

		internal override OptionsList GetOptions ()
		{
			return new OptionsList (null, null, null, -1, source);
		}
	}
}
#endif
