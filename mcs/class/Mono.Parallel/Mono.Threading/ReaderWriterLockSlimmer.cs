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

		public void EnterReadLock (ref bool taken)
		{
			if (taken)
				throw new ArgumentException ("taken", "taken needs to be set to false");

			SpinWait sw = new SpinWait ();
			bool cont = true;

			do {
				while ((rwlock & (RwWrite | RwWait)) > 0)
					sw.SpinOnce ();

				try {}
				finally {
					if ((Interlocked.Add (ref rwlock, RwRead) & (RwWait | RwWait)) == 0) {
						taken = true;
						cont = false;
					} else {
						Interlocked.Add (ref rwlock, -RwRead);
					}
				}
			} while (cont);
		}

		public void TryEnterReadLock (ref bool taken)
		{
			if (taken)
				throw new ArgumentException ("taken", "taken needs to be set to false");

			try {}
			finally {
				if ((Interlocked.Add (ref rwlock, RwRead) & (RwWait | RwWrite)) == 0)
					taken = true;
				else
					Interlocked.Add (ref rwlock, -RwRead);
			}
		}

		public void ExitReadLock ()
		{
			Interlocked.Add (ref rwlock, -RwRead);
		}

		public void EnterWriteLock (ref bool taken)
		{
			if (taken)
				throw new ArgumentException ("taken", "taken needs to be set to false");

			SpinWait sw = new SpinWait ();
			int state = rwlock;

			try {
				do {
					state = rwlock;
					if (state < RwWrite) {
						try {}
						finally {
							if (Interlocked.CompareExchange (ref rwlock, RwWrite, state) == state)
								taken = true;
						}
						if (taken)
							return;

						state = rwlock;
					}

					while ((state & RwWait) == 0 && Interlocked.CompareExchange (ref rwlock, state | RwWait, state) != state)
						state = rwlock;

					while (rwlock > RwWait)
						sw.SpinOnce ();
				} while (true);
			} finally {
				state = rwlock;
				if (!taken && (state & RwWait) != 0)
					Interlocked.CompareExchange (ref rwlock, state - RwWait, state);
			}
		}

		public void TryEnterWriteLock (ref bool taken)
		{
			if (taken)
				throw new ArgumentException ("taken", "taken needs to be set to false");

			int state = rwlock;

			if (state >= RwWrite)
				return;

			try {}
			finally {
				if (Interlocked.CompareExchange (ref rwlock, RwWrite, state) == state)
					taken = true;
			}
		}

		public void ExitWriteLock ()
		{
			Interlocked.Add (ref rwlock, -RwWrite);
		}
	}
}

#endif