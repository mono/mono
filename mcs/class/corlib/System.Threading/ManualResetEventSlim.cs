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

#if NET_4_0

namespace System.Threading
{
	[System.Diagnostics.DebuggerDisplayAttribute ("Set = {IsSet}")]
	public class ManualResetEventSlim : IDisposable
	{
		readonly int spinCount;

		ManualResetEvent handle;
		internal AtomicBooleanValue disposed;
		int used;
		long state;

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

			this.state = initialState ? 1 : 0;
			this.spinCount = spinCount;
		}

		public bool IsSet {
			get {
				return (state & 1) == 1;
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

			var stamp = UpdateStateWithOp (false);
			if (handle != null)
				CommitChangeToHandle (stamp);
		}

		public void Set ()
		{
			var stamp = UpdateStateWithOp (true);
			if (handle != null)
				CommitChangeToHandle (stamp);
		}

		long UpdateStateWithOp (bool set)
		{
			long oldValue, newValue;
			do {
				oldValue = state;
				newValue = (long)(((oldValue >> 1) + 1) << 1) | (set ? 1u : 0u);
			} while (Interlocked.CompareExchange (ref state, newValue, oldValue) != oldValue);
			return newValue;
		}

		void CommitChangeToHandle (long stamp)
		{
			Interlocked.Increment (ref used);
			var tmpHandle = handle;
			if (tmpHandle != null) {
				// First in all case we carry the operation we were called for
 				if ((stamp & 1) == 1)
					tmpHandle.Set ();
				else
					tmpHandle.Reset ();

				/* Then what may happen is that the two suboperations (state change and handle change)
				 * overlapped with others. In our case it doesn't matter if the two suboperations aren't
				 * executed together at the same time, the only thing we have to make sure of is that both
				 * state and handle are synchronized on the last visible state change.
				 *
				 * For instance if S is state change and H is handle change, for 3 concurrent operations
				 * we may have the following serialized timeline: S1 S2 H2 S3 H3 H1
				 * Which is perfectly fine (all S were converted to H at some stage) but in that case
				 * we have a mismatch between S and H at the end because the last operations done were
				 * S3/H1. We thus need to repeat H3 to get to the desired final state.
				 */
				long currentState;
				do {
					currentState = state;
					if (currentState != stamp && (stamp & 1) != (currentState & 1)) {
						if ((currentState & 1) == 1)
							tmpHandle.Set ();
						else
							tmpHandle.Reset ();
					}
				} while (currentState != state);
			}
			Interlocked.Decrement (ref used);
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

			if (!IsSet) {
				SpinWait wait = new SpinWait ();

				while (!IsSet) {
					cancellationToken.ThrowIfCancellationRequested ();

					if (wait.Count < spinCount) {
						wait.SpinOnce ();
						continue;
					}

					break;
				}

				if (IsSet)
					return true;

				WaitHandle handle = WaitHandle;

				if (cancellationToken.CanBeCanceled) {
					var result = WaitHandle.WaitAny (new[] { handle, cancellationToken.WaitHandle }, millisecondsTimeout, false);
					if (result == 1)
						throw new OperationCanceledException (cancellationToken);
					if (result == WaitHandle.WaitTimeout)
						return false;
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

				var isSet = IsSet;
				var mre = new ManualResetEvent (IsSet);
				if (Interlocked.CompareExchange (ref handle, mre, null) == null) {
					//
					// Ensure the Set has not ran meantime
					//
					if (isSet != IsSet) {
						if (IsSet) {
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
				if (used > 0) {
					// A tiny wait (just a few cycles normally) before releasing
					SpinWait wait = new SpinWait ();
					while (used > 0)
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
