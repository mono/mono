#if NET_4_0
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

namespace System.Threading
{
	public class SemaphoreSlim : IDisposable
	{
		readonly int max;
		
		int currCount;

		bool isDisposed;
		
		SpinWait wait = new SpinWait ();
		
		public SemaphoreSlim (int initial) : this (initial, int.MaxValue)
		{
		}
		
		public SemaphoreSlim (int initial, int max)
		{
			if (initial < 0 || initial > max || max < 0)
				throw new ArgumentOutOfRangeException ("The initial  argument is negative, initial is greater than max, or max is not positive.");
			
			this.max = max;
			this.currCount = initial;
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
			if (releaseCount < 0)
				throw new ArgumentOutOfRangeException ("releaseCount", "	The releaseCount must be positive.");
			
			// As we have to take care of the max limit we resort to CAS
			int oldValue, newValue;
			do {
				oldValue = currCount;
				newValue = (currCount + releaseCount);
				newValue = newValue > max ? max : newValue;
			} while (Interlocked.CompareExchange (ref currCount, newValue, oldValue) != oldValue);
			
			return oldValue;
		}
		
		public void Wait ()
		{
			CheckState ();
			do {
				int result = Interlocked.Decrement (ref currCount);
				if (result >= 0)
					break;
				
				// We revert back the operation
				Interlocked.Increment (ref currCount);
				while (Thread.VolatileRead (ref currCount) <= 0) {
					wait.SpinOnce ();
				}
			} while (true);
		}
		
		public bool Wait (TimeSpan ts)
		{
			CheckState();
			return Wait ((int)ts.TotalMilliseconds);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			CheckState ();
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout",
				                                       "millisecondsTimeout is a negative number other than -1");
			if (millisecondsTimeout == -1) {
				Wait ();
				return true;
			}
			
			do {
				int result = Interlocked.Decrement (ref currCount);
				if (result >= 0)
					break;
				
				// We revert back the operation
				result = Interlocked.Increment (ref currCount);
				Watch sw = Watch.StartNew ();
				while (Thread.VolatileRead (ref currCount) <= 0) {
					if (sw.ElapsedMilliseconds > millisecondsTimeout) {
						sw.Stop ();
						return false;
					}
					wait.SpinOnce ();
				}
			} while (true);
			
			return true;
		}
		
		[MonoTODO ("Cf CountdownEvent for ManualResetEvent usage")]
		public WaitHandle AvailableWaitHandle {
			get {
				return null;
			}
		}
	}
}
#endif
