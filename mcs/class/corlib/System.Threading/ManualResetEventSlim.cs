// ManualResetEventSlim.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

namespace System.Threading
{
	[System.Diagnostics.DebuggerDisplayAttribute ("Set = {IsSet}")]
	public class ManualResetEventSlim : IDisposable
	{
		readonly int spinCount;

		ManualResetEvent handle;
		internal AtomicBooleanValue disposed;
		bool used;
		bool set;

		public ManualResetEventSlim ()
			: this (false, 10)
		{
		}

		public ManualResetEventSlim (bool initialState)
			: this (initialState, 10)
		{
		}

		public ManualResetEventSlim (bool initialState, int spinCount)
		{
			if (spinCount < 0 || spinCount > 2047)
				throw new ArgumentOutOfRangeException ("spinCount");

			this.set = initialState;
			this.spinCount = spinCount;
		}

		public bool IsSet {
			get {
				return set;
			}
		}

		public int SpinCount {
			get {
				return spinCount;
			}
		}

		public void Reset ()
		{
			ThrowIfDisposed ();

			set = false;
			Thread.MemoryBarrier ();
			if (handle != null) {
				used = true;
				Thread.MemoryBarrier ();
				var tmpHandle = handle;
				if (tmpHandle != null)
					tmpHandle.Reset ();
				Thread.MemoryBarrier ();
				used = false;
			}
		}

		public void Set ()
		{
			set = true;
			Thread.MemoryBarrier ();
			if (handle != null) {
				used = true;
				Thread.MemoryBarrier ();
				var tmpHandle = handle;
				if (tmpHandle != null)
					tmpHandle.Set ();
				Thread.MemoryBarrier ();
				used = false;
			}
		}

		public void Wait ()
		{
			Wait (CancellationToken.None);
		}

		public bool Wait (int millisecondsTimeout)
		{
			return Wait (millisecondsTimeout, CancellationToken.None);
		}

		public bool Wait (TimeSpan timeout)
		{
			return Wait (CheckTimeout (timeout), CancellationToken.None);
		}

		public void Wait (CancellationToken cancellationToken)
		{
			Wait (Timeout.Infinite, cancellationToken);
		}

		public bool Wait (int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			ThrowIfDisposed ();

			if (!set) {
				SpinWait wait = new SpinWait ();

				while (!set) {
					cancellationToken.ThrowIfCancellationRequested ();

					if (wait.Count < spinCount) {
						wait.SpinOnce ();
						continue;
					}

					break;
				}

				if (set)
					return true;

				WaitHandle handle = WaitHandle;

				if (cancellationToken.CanBeCanceled) {
					if (WaitHandle.WaitAny (new[] { handle, cancellationToken.WaitHandle }, millisecondsTimeout, false) == 0)
						return false;

					cancellationToken.ThrowIfCancellationRequested ();
				} else {
					if (!handle.WaitOne (millisecondsTimeout, false))
						return false;
				}
			}

			return true;
		}

		public bool Wait (TimeSpan timeout, CancellationToken cancellationToken)
		{
			return Wait (CheckTimeout (timeout), cancellationToken);
		}

		public WaitHandle WaitHandle {
			get {
				ThrowIfDisposed ();

				if (handle != null)
					return handle;

				var isSet = set;
				var mre = new ManualResetEvent (isSet);
				if (Interlocked.CompareExchange (ref handle, mre, null) == null) {
					//
					// Ensure the Set has not ran meantime
					//
					if (isSet != set) {
						if (set) {
							mre.Set ();
						} else {
							mre.Reset ();
						}
					}
				} else {
					//
					// Release the event when other thread was faster
					//
					mre.Dispose ();
				}

				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposed.TryRelaxedSet ())
				return;

			if (handle != null) {
				var tmpHandle = Interlocked.Exchange (ref handle, null);
				if (used) {
					// A tiny wait (just a few cycles normally) before releasing
					SpinWait wait = new SpinWait ();
					while (used)
						wait.SpinOnce ();
				}
				tmpHandle.Dispose ();
			}
		}

		void ThrowIfDisposed ()
		{
			if (disposed.Value)
				throw new ObjectDisposedException ("ManualResetEventSlim");
		}

		static int CheckTimeout (TimeSpan timeout)
		{
			try {
				return checked ((int)timeout.TotalMilliseconds);
			} catch (System.OverflowException) {
				throw new ArgumentOutOfRangeException ("timeout");
			}
		}
	}
}
#endif
