// TargetBuffer.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
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
//
//

#if NET_4_0 || MOBILE

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	internal class TargetBuffer<T> : IEnumerable<ITargetBlock<T>>
	{
		ConcurrentQueue<TargetWaiter> targetWaiters = new ConcurrentQueue<TargetWaiter> ();

		class TargetWaiter : IDisposable
		{
			public volatile bool Disabled;
			public readonly ITargetBlock<T> Target;
			public readonly bool UnlinkAfterOne;
			
			ConcurrentQueue<TargetWaiter> queue;
			AtomicBooleanValue removed;

			public TargetWaiter (ITargetBlock<T> target, bool unlinkAfterOne, ConcurrentQueue<TargetWaiter> queue)
			{
				Target = target;
				UnlinkAfterOne = unlinkAfterOne;
				this.queue = queue;
			}

			public void Dispose ()
			{
				TargetWaiter t;
				Disabled = true;

				Thread.MemoryBarrier ();

				if (queue.TryPeek (out t) && t == this && removed.TryRelaxedSet ()) {
					queue.TryDequeue (out t);
				} else {
					SpinWait wait = new SpinWait ();
					while (queue.TryPeek (out t) && t == this)
						wait.SpinOnce ();
				}
			}
		}

		public IDisposable AddTarget (ITargetBlock<T> block, bool unlinkAfterOne)
		{
			TargetWaiter w = new TargetWaiter (block, unlinkAfterOne, targetWaiters);
			targetWaiters.Enqueue (w);

			return w;
		}

		public ITargetBlock<T> Current {
			get {
				TargetWaiter w;
				
				while (true) {
					if (!targetWaiters.TryPeek (out w))
						return null;

					if (w.Disabled == true) {
						w.Dispose ();
						continue;
					} else if (w.UnlinkAfterOne) {
						w.Dispose ();
					}

					return w.Target;
				}
			}
		}

		public IEnumerator<ITargetBlock<T>> GetEnumerator ()
		{
			return targetWaiters.Select (w => w.Target).GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return targetWaiters.Select (w => w.Target).GetEnumerator ();
		}
	}
}

#endif
