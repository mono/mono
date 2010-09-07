//
// ParallelQuery.cs
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
	public class ParallelQuery : IEnumerable
	{
		ParallelExecutionMode execMode = ParallelExecutionMode.Default;
		ParallelMergeOptions mergeOptions = ParallelMergeOptions.Default;

		internal ParallelQuery ()
		{

		}

		internal ParallelMergeOptions MergeOptions {
			get {
				return mergeOptions;
			}
			set {
				mergeOptions = value;
			}
		}

		internal ParallelExecutionMode ExecMode {
			get {
				return execMode;
			}
			set {
				execMode = value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumeratorTrick ();
		}

		// Trick to get the correct IEnumerator from ParallelQuery<TSource>
		internal virtual IEnumerator GetEnumeratorTrick ()
		{
			return null;
		}
		
		internal virtual ParallelQuery<object> TypedQuery {
			get {
				return null;
			}
		}
	}

	public class ParallelQuery<TSource> : ParallelQuery, IEnumerable<TSource>, IEnumerable
	{
		QueryBaseNode<TSource> node;

		internal ParallelQuery (QueryBaseNode<TSource> node)
		{
			this.node = node;
		}

		internal QueryBaseNode<TSource> Node {
			get {
				return node;
			}
		}

		public virtual IEnumerator<TSource> GetEnumerator ()
		{
			return GetEnumeratorInternal ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator)GetEnumeratorInternal ();
		}

		IEnumerator<TSource> GetEnumeratorInternal ()
		{
			return new ParallelQueryEnumerator<TSource> (node);
		}

		internal override IEnumerator GetEnumeratorTrick ()
		{
			return (IEnumerator)GetEnumeratorInternal ();
		}
		
		internal override ParallelQuery<object> TypedQuery {
			get {
				return new ParallelQuery<object> (new QueryCastNode<TSource> (node));
			}
		}
	}
}
#endif
