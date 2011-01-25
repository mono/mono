// ManuelResetEventSlim.cs
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

#if NET_4_0 || MOBILE

using System;

namespace System.Threading
{
	[System.Diagnostics.DebuggerDisplayAttribute ("Set = {IsSet}")]
	public class ManualResetEventSlim : IDisposable
	{
		const int isSet    = 1;
		const int isNotSet = 0;
		const int defaultSpinCount = 100;

		int state;
		readonly int spinCount;

		ManualResetEvent handle;

		readonly static Watch sw = Watch.StartNew ();

		public ManualResetEventSlim () : this (false, defaultSpinCount)
		{
		}

		public ManualResetEventSlim (bool initialState) : this (initialState, defaultSpinCount)
		{
		}

		public ManualResetEventSlim (bool initialState, int spinCount)
		{
			if (spinCount < 0)
				throw new ArgumentOutOfRangeException ("spinCount is less than 0", "spinCount");

			this.state = initialState ? isSet : isNotSet;
			this.spinCount = spinCount;
		}

		public bool IsSet {
			get {
				return state == isSet;
			}
		}

		public int SpinCount {
			get {
				return spinCount;
			}
		}

		public void Reset ()
		{
			state = isNotSet;
			if (handle != null)
				handle.Reset ();
		}

		public void Set ()
		{
			state = isSet;
			if (handle != null)
				handle.Set ();
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
			return Wait ((int)timeout.TotalMilliseconds, CancellationToken.None);
		}

		public void Wait (CancellationToken cancellationToken)
		{
			Wait (-1, cancellationToken);
		}

		public bool Wait (int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout",
				                                       "millisecondsTimeout is a negative number other than -1");

			long start = millisecondsTimeout == -1 ? 0 : sw.ElapsedMilliseconds;
			SpinWait wait = new SpinWait ();

			while (state == isNotSet) {
				cancellationToken.ThrowIfCancellationRequested ();

				if (millisecondsTimeout > -1 && (sw.ElapsedMilliseconds - start) > millisecondsTimeout)
					return false;

				if (wait.Count < spinCount) {
					wait.SpinOnce ();
				} else {
					int waitTime = millisecondsTimeout == -1 ? -1 : Math.Max (millisecondsTimeout - (int)(sw.ElapsedMilliseconds - start) , 1);
					WaitHandle handle = WaitHandle;
					if (state == isSet)
						return true;
					if (WaitHandle.WaitAny (new[] { handle, cancellationToken.WaitHandle }, waitTime, false) == 0)
						return true;
				}
			}

			return true;
		}

		public bool Wait (TimeSpan timeout, CancellationToken cancellationToken)
		{
			return Wait ((int)timeout.TotalMilliseconds, cancellationToken);
		}

		public WaitHandle WaitHandle {
			get {
				if (handle != null)
					return handle;
				return LazyInitializer.EnsureInitialized (ref handle,
				                                          () => new ManualResetEvent (state == isSet ? true : false));
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			Dispose(true);
		}

		protected virtual void Dispose (bool disposing)
		{

		}
		#endregion

	}
}
#endif
