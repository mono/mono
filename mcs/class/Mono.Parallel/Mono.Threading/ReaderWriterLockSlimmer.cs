// ReaderWriterLockSlimmer.cs
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
//
//

#if NET_4_0

using System;
using System.Threading;

namespace Mono.Threading
{
	public struct ReaderWriterLockSlimmer
	{
		const int RwWait = 1;
		const int RwWrite = 2;
		const int RwRead = 4;

		int rwlock;

		public void EnterReadLock ()
		{
			SpinWait sw = new SpinWait ();
			do {
				while ((rwlock & (RwWrite | RwWait)) > 0)
					sw.SpinOnce ();

				if ((Interlocked.Add (ref rwlock, RwRead) & (RwWait | RwWait)) == 0)
					return;

				Interlocked.Add (ref rwlock, -RwRead);
			} while (true);
		}

		public void ExitReadLock ()
		{
			Interlocked.Add (ref rwlock, -RwRead);
		}

		public void EnterWriteLock ()
		{
			SpinWait sw = new SpinWait ();
			do {
				int state = rwlock;
				if (state < RwWrite) {
					if (Interlocked.CompareExchange (ref rwlock, RwWrite, state) == state)
						return;
					state = rwlock;
				}
				// We register our interest in taking the Write lock (if upgradeable it's already done)
				while ((state & RwWait) == 0 && Interlocked.CompareExchange (ref rwlock, state | RwWait, state) != state)
					state = rwlock;
				// Before falling to sleep
				while (rwlock > RwWait)
					sw.SpinOnce ();
			} while (true);
		}

		public void ExitWriteLock ()
		{
			Interlocked.Add (ref rwlock, -RwWrite);
		}
	}
}