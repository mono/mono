//
// QueryOptions.cs
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

namespace System.Linq.Parallel
{
	internal class QueryOptions
	{
		public ParallelMergeOptions? Options {
			get;
			private set;
		}

		public ParallelExecutionMode? Mode {
			get;
			private set;
		}

		public CancellationToken Token {
			get;
			private set;
		}
		/* This token is to be used by some operator (like Take) to tell that
		 * the execution of the query can be prematurly stopped
		 *
		 * It is set when passing QueryOptions to the different node's Get method
		 * and ParallelExecuter should check after the call to this method is this guy has been
		 * set. Operator may chain up multiple cancellation token that way.
		 * When checking for this token, the task body should simply return.
		 */
		public CancellationToken ImplementerToken {
			get;
			private set;
		}

		public bool UseStrip {
			get;
			private set;
		}

		public bool? BehindOrderGuard {
			get;
			private set;
		}

		public int PartitionCount {
			get;
			private set;
		}

		public Tuple<bool, bool, bool> PartitionerSettings {
			get;
			internal set;
		}

		public CancellationToken MergedToken {
			get;
			private set;
		}

		public bool ShouldBeSequential {
			get;
			private set;
		}

		public QueryOptions (ParallelMergeOptions? options,
		                     ParallelExecutionMode? mode,
		                     CancellationToken token,
		                     bool useStrip,
		                     bool? behindOrderGuard,
		                     int partitionCount,
		                     CancellationToken implementerToken,
		                     bool shouldBeSequential)
		{
			Options = options;
			Mode = mode;
			Token = token;
			UseStrip = useStrip;
			BehindOrderGuard = behindOrderGuard;
			PartitionCount = partitionCount;
			PartitionerSettings = null;
			ImplementerToken = implementerToken;
			ShouldBeSequential = shouldBeSequential;

			MergeTokens (token, implementerToken);
		}

		void MergeTokens (CancellationToken token, CancellationToken implementerToken)
		{
			bool implementedNone = implementerToken == CancellationToken.None;
			bool tokenNone = token == CancellationToken.None;
			if (!implementedNone && !tokenNone)
				MergedToken = CancellationTokenSource.CreateLinkedTokenSource (implementerToken, token).Token;
			else if (implementedNone && !tokenNone)
				MergedToken = token;
			else if (!implementedNone && tokenNone)
				MergedToken = implementerToken;
			else
				MergedToken = CancellationToken.None;
		}
	}
}
#endif
