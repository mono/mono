//
// QueryCheckerVisitor.cs
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

namespace System.Linq
{
	using OptionsList = Tuple<ParallelMergeOptions?, ParallelExecutionMode?, CancellationToken?, int, CancellationTokenSource>;

	internal class QueryCheckerVisitor : INodeVisitor
	{
		// Information gathering
		ParallelMergeOptions? options = null;
		ParallelExecutionMode? mode = null;
		CancellationToken? token = null;
		int? degreeOfParallelism = null;
		bool useStrip;
		CancellationToken implementerToken = CancellationToken.None;

		int partitionCount;
		bool? behindOrderGuard = null;

		internal QueryCheckerVisitor (int partitionCount)
		{
			this.partitionCount = partitionCount;
		}

		#region INodeVisitor implementation
		public void Visit<T> (QueryBaseNode<T> node)
		{
			// Nothing to do atm. Later we can check if the node is a
			// Take or a Skip and set accordingly useStrip
		}

		public void Visit<U, V> (QueryChildNode<U, V> node)
		{
			node.Parent.Visit (this);
		}

		public void Visit<T> (QueryOptionNode<T> node)
		{
			MergeOptions (node.GetOptions ());

			Visit<T, T> ((QueryChildNode<T, T>)node);
		}

		public void Visit<T> (QueryStartNode<T> node)
		{
			if (behindOrderGuard == null)
				behindOrderGuard = false;
			if (degreeOfParallelism != null)
				partitionCount = degreeOfParallelism.Value;
		}

		public void Visit<T, TParent> (QueryStreamNode<T, TParent> node)
		{
			if (node.IsIndexed)
				useStrip = true;

			Visit<T, TParent> ((QueryChildNode<T, TParent>)node);
		}

		public void Visit<T> (QueryOrderGuardNode<T> node)
		{
			if (behindOrderGuard == null) {
				if (node.EnsureOrder) {
					behindOrderGuard = true;
					//useStrip = true;
				} else {
					behindOrderGuard = false;
				}
			}

			Visit<T, T> ((QueryStreamNode<T, T>)node);
		}

		public void Visit<TFirst, TSecond, TResult> (QueryMuxNode<TFirst, TSecond, TResult> node)
		{
			Visit<TResult, TFirst> ((QueryChildNode<TResult, TFirst>)node);
		}

		#endregion

		internal QueryOptions Options {
			get {
				return new QueryOptions (options, mode, token == null ? CancellationToken.None : token.Value,
				                         useStrip, behindOrderGuard, partitionCount, implementerToken);
			}
		}

		internal bool UseStrip {
			get {
				return useStrip;
			}
		}

		internal bool BehindOrderGuard {
			get {
				return behindOrderGuard.Value;
			}
		}

		void MergeOptions (OptionsList list)
		{
			if (list.Item1 != null) {
				if (options == null)
					options = list.Item1;
				else
					Throw ("WithMergeOptions");
			}

			if (list.Item2 != null) {
				if (mode == null)
					mode = list.Item2;
				else
					Throw ("WithExecutionMode");
			}

			if (list.Item3 != null) {
				if (token == null)
					token = list.Item3;
				else
					Throw ("WithCancellationToken");
			}

			if (list.Item4 != -1) {
				if (degreeOfParallelism == null)
					degreeOfParallelism = list.Item4;
				else
					Throw ("WithDegreeOfParallelism");
			}

			// That one is treated specially
			if (list.Item5 != null) {
				implementerToken = implementerToken.Chain (list.Item5);
			}
		}

		void Throw (string methName)
		{
			throw new InvalidOperationException ("You can't have more than one " + methName + " node in a query");
		}
	}
}
#endif
