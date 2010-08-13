// SpinWait.cs
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

#if NET_4_0 || BOOTSTRAP_NET_4_0
using System;

namespace System.Threading
{
	public struct SpinWait
	{
		// The number of step until SpinOnce yield on multicore machine
		const           int  step = 5;
		const           int  maxTime = 50;
		static readonly bool isSingleCpu = (Environment.ProcessorCount == 1);

		int ntime;

		public void SpinOnce ()
		{
			if (isSingleCpu) {
				// On a single-CPU system, spinning does no good
				Thread.Yield ();
			} else {
				if ((ntime = ntime == maxTime ? maxTime : ntime + 1) % step == 0)
					Thread.Yield ();
				else
					// Multi-CPU system might be hyper-threaded, let other thread run
					Thread.SpinWait (ntime << 1);
			}
		}

		public static void SpinUntil (Func<bool> predicate)
		{
			SpinWait sw = new SpinWait ();
			while (!predicate ())
				sw.SpinOnce ();
		}

		public static bool SpinUntil (Func<bool> predicate, TimeSpan ts)
		{
			return SpinUntil (predicate, (int)ts.TotalMilliseconds);
		}

		public static bool SpinUntil (Func<bool> predicate, int milliseconds)
		{
			SpinWait sw = new SpinWait ();
			Watch watch = Watch.StartNew ();

			while (!predicate ()) {
				if (watch.ElapsedMilliseconds > milliseconds)
					return false;
				sw.SpinOnce ();
			}

			return true;
		}

		public void Reset ()
		{
			ntime = 0;
		}

		public bool NextSpinWillYield {
			get {
				return isSingleCpu ? true : ntime % step == 0;
			}
		}

		public int Count {
			get {
				return ntime;
			}
		}
	}
}
#endif
