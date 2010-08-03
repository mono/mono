#if NET_4_0
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

using System;
using System.Diagnostics;

namespace System.Threading
{
	public class ManualResetEventSlim : IDisposable
	{
		const int isSet    = 1;
		const int isNotSet = 0;
		const int defaultSpinCount = 10;
		const int deepSleepTime = 20;

		int state;
		readonly int spinCount;

		ManualResetEvent handle;

		public ManualResetEventSlim () : this (false, defaultSpinCount)
		{
		}

		public ManualResetEventSlim (bool initState) : this (initState, defaultSpinCount)
		{
		}

		public ManualResetEventSlim (bool initState, int spinCount)
		{
			if (spinCount < 0)
				throw new ArgumentOutOfRangeException ("spinCount is less than 0", "spinCount");

			this.state = initState ? isSet : isNotSet;
			this.spinCount = spinCount;
			this.handle = new ManualResetEvent (initState);
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
			Interlocked.Exchange (ref state, isNotSet);
			handle.Reset ();
		}

		public void Set ()
		{
			Interlocked.Exchange (ref state, isSet);
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

		public bool Wait (TimeSpan ts)
		{
			return Wait ((int)ts.TotalMilliseconds, CancellationToken.None);
		}

		public void Wait (CancellationToken token)
		{
			Wait (-1, token);
		}

		public bool Wait (int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout",
				                                       "millisecondsTimeout is a negative number other than -1");

			Watch s = Watch.StartNew ();
			SpinWait sw = new SpinWait ();

			while (state == isNotSet) {
				token.ThrowIfCancellationRequested ();

				if (millisecondsTimeout > -1 && s.ElapsedMilliseconds > millisecondsTimeout)
					return false;

				if (sw.Count < spinCount)
					sw.SpinOnce ();
				else
					if (handle.WaitOne (Math.Min (Math.Max (millisecondsTimeout - (int)s.ElapsedMilliseconds, 1), deepSleepTime)))
						return true;
			}

			return true;
		}

		public bool Wait (TimeSpan ts, CancellationToken token)
		{
			return Wait ((int)ts.TotalMilliseconds, token);
		}

		public WaitHandle WaitHandle {
			get {
				return handle;
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool managedRes)
		{

		}
		#endregion

	}
}
#endif
