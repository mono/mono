// SemaphoreSlim.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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

using System;
using System.Diagnostics;

#if NET_4_0
namespace System.Threading
{
	public class SemaphoreSlim : IDisposable
	{
		const int spinCount = 10;
		const int deepSleepTime = 20;

		readonly int max;
		int currCount;
		bool isDisposed;

		EventWaitHandle handle;

		public SemaphoreSlim (int initial) : this (initial, int.MaxValue)
		{
		}

		public SemaphoreSlim (int initial, int max)
		{
			if (initial < 0 || initial > max || max < 0)
				throw new ArgumentOutOfRangeException ("The initial  argument is negative, initial is greater than max, or max is not positive.");

			this.max = max;
			this.currCount = initial;
			this.handle = new ManualResetEvent (initial == 0);
		}

		~SemaphoreSlim ()
		{
			Dispose(false);
		}

		public void Dispose ()
		{
			Dispose(true);
		}

		protected virtual void Dispose (bool managedRes)
		{
			isDisposed = true;
		}

		void CheckState ()
		{
			if (isDisposed)
				throw new ObjectDisposedException ("The SemaphoreSlim has been disposed.");
		}

		public int CurrentCount {
			get {
				return currCount;
			}
		}

		public int Release ()
		{
			return Release(1);
		}

		public int Release (int releaseCount)
		{
			CheckState ();
			if (releaseCount < 1)
				throw new ArgumentOutOfRangeException ("releaseCount", "releaseCount is less than 1");

			// As we have to take care of the max limit we resort to CAS
			int oldValue, newValue;
			do {
				oldValue = currCount;
				newValue = (currCount + releaseCount);
				newValue = newValue > max ? max : newValue;
			} while (Interlocked.CompareExchange (ref currCount, newValue, oldValue) != oldValue);

			handle.Set ();

			return oldValue;
		}

		public void Wait ()
		{
			Wait (CancellationToken.None);
		}

		public bool Wait (TimeSpan ts)
		{
			return Wait ((int)ts.TotalMilliseconds, CancellationToken.None);
		}

		public bool Wait (int millisecondsTimeout)
		{
			return Wait (millisecondsTimeout, CancellationToken.None);
		}

		public void Wait (CancellationToken token)
		{
			Wait (-1, token);
		}

		public bool Wait (TimeSpan ts, CancellationToken token)
		{
			CheckState();
			return Wait ((int)ts.TotalMilliseconds, token);
		}

		public bool Wait (int millisecondsTimeout, CancellationToken token)
		{
			CheckState ();
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout",
				                                       "millisecondsTimeout is a negative number other than -1");

			Watch sw = Watch.StartNew ();

			Func<bool> stopCondition = () => millisecondsTimeout >= 0 && sw.ElapsedMilliseconds > millisecondsTimeout;

			do {
				bool shouldWait;
				int result;

				do {
					token.ThrowIfCancellationRequested ();
					if (stopCondition ())
						return false;

					shouldWait = true;
					result = currCount;

					if (result > 0)
						shouldWait = false;
					else
						break;
				} while (Interlocked.CompareExchange (ref currCount, result - 1, result) != result);

				if (!shouldWait) {
					if (result == 1)
						handle.Reset ();
					break;
				}

				SpinWait wait = new SpinWait ();

				while (Thread.VolatileRead (ref currCount) <= 0) {
					token.ThrowIfCancellationRequested ();
					if (stopCondition ())
						return false;

					if (wait.Count > spinCount)
						handle.WaitOne (Math.Min (Math.Max (millisecondsTimeout - (int)sw.ElapsedMilliseconds, 1), deepSleepTime));
					else
						wait.SpinOnce ();
				}
			} while (true);

			return true;
		}

		public WaitHandle AvailableWaitHandle {
			get {
				return handle;
			}
		}
	}
}
#endif
