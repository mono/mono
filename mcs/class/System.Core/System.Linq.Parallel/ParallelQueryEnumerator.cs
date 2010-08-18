//
// ParallelEnumerator.cs
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
using System.Linq.Parallel.QueryNodes;

namespace System.Linq.Parallel
{
	internal class ParallelQueryEnumerator<T> : IEnumerator<T>
	{
		readonly int DefaultBufferSize = ParallelExecuter.GetBestWorkerNumber () * 50;

		BlockingCollection<T> buffer;
		IEnumerator<T> loader;
		QueryOptions options;
		OrderingEnumerator<T> ordEnumerator;

		T current;

		Action waitAction;

		internal ParallelQueryEnumerator (QueryBaseNode<T> node)
		{
			this.options = ParallelExecuter.CheckQuery (node);
			Setup ();

			// Launch adding to the buffer asynchronously via Tasks
			if (options.BehindOrderGuard.Value) {
				waitAction = ParallelExecuter.ProcessAndCallback (node,
				                                                  (e, i) => ordEnumerator.Add (e),
				                                                  (i) => ordEnumerator.EndParticipation (),
				                                                  ordEnumerator.Stop,
				                                                  options);
			} else {
				waitAction = ParallelExecuter.ProcessAndCallback (node,
				                                                  buffer.Add,
				                                                  buffer.CompleteAdding,
				                                                  options);
			}

			if (options.Options.HasValue && options.Options.Value == ParallelMergeOptions.FullyBuffered)
				waitAction ();
		}

		void Setup ()
		{
			if (!options.BehindOrderGuard.Value) {
				if (options.Options.HasValue && (options.Options.Value == ParallelMergeOptions.NotBuffered
				                                 || options.Options.Value == ParallelMergeOptions.FullyBuffered)) {
					buffer = new BlockingCollection<T> ();
				} else {
					buffer = new BlockingCollection<T> (DefaultBufferSize);
				}

				IEnumerable<T> source = buffer.GetConsumingEnumerable (options.Token);

				loader = source.GetEnumerator ();
			} else {
				loader = ordEnumerator = new OrderingEnumerator<T> (options.PartitionCount);
			}
		}

		public void Dispose ()
		{

		}

		public void Reset ()
		{
			throw new NotSupportedException ();
		}

		public bool MoveNext ()
		{
			// If there are no stuff in the buffer
			// but CompleteAdding hasn't been called,
			// MoveNext blocks until further results are produced
			if (!loader.MoveNext ())
				return false;

			current = loader.Current;
			return true;
		}

		public T Current {
			get {
				return current;
			}
		}

		object IEnumerator.Current {
			get {
				return current;
			}
		}
	}
}
#endif
