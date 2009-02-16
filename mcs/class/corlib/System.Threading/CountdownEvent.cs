#if NET_4_0
// CountdownEvent.cs
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
	public class CountdownEvent: ISupportsCancellation, IDisposable
	{
		int count;
		readonly int initial;
		bool isCanceled;
		
		public CountdownEvent(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count is negative");
			this.initial = this.count = count;
		}
		
		/*~CountdownEvent()
		{
			Dispose(false);
		}*/
		
		public void Decrement()
		{
			Decrement(1);
		}
		
		public void Decrement(int num)
		{
			if (num < 0)
				throw new ArgumentOutOfRangeException("num");
			if (IsSet || num > count)
				throw new InvalidOperationException();
			Interlocked.Add(ref count, -num);
		}
		
		public void Increment()
		{
			Increment(1);
		}
		
		public void Increment(int num)
		{
			if (num < 0)
				throw new ArgumentOutOfRangeException("num");
			if (IsSet || num > int.MaxValue - count)
				throw new InvalidOperationException();
			Interlocked.Add(ref count, num);
		}
		
		public bool TryIncrement()
		{
			return TryIncrement(1);
		}
		
		public bool TryIncrement(int num)
		{
			if (IsSet)
				return false;
			if (num < 0)
				throw new ArgumentOutOfRangeException("num");
			if (num > int.MaxValue - count)
				throw new InvalidOperationException();
			
			if (IsSet)
				return false;
			
			Interlocked.Add(ref count, num);
			return true;
		}
		
		public void Wait()
		{
			SpinWait wait = new SpinWait();
			while (!IsSet) {
				wait.SpinOnce();
			}
		}
		
		public bool Wait(int timeoutMilli)
		{
			if (timeoutMilli == -1) {
				Wait();
				return true;
			}
			
			SpinWait wait = new SpinWait();
			Stopwatch sw = Stopwatch.StartNew();
			
			while (!IsSet) {
				if (sw.ElapsedMilliseconds > (long)timeoutMilli) {
					sw.Stop();
					return false;
				}
				wait.SpinOnce();
			}
			return true;
		}
		
		public bool Wait(TimeSpan span)
		{
			return Wait((int)span.TotalMilliseconds);
		}
		
		public void Reset()
		{
			Reset(initial);
		}
		
		public void Reset(int value)
		{
			Thread.VolatileWrite(ref count, value);
		}
		
		public int CurrentCount {
			get {
				return count;
			}
		}
		
		public int InitialCount {
			get {
				return initial;
			}
		}
			
		public bool IsSet {
			get {
				return count <= 0;
			}
		}
		
		public WaitHandle WaitHandle {
			get {
				return null;
			}
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			//Dispose(true);
		}
		
		/*protected virtual void Dispose(bool managedRes)
		{
			
		}*/
		#endregion 
		
		
		#region ISupportsCancellation implementation 
		
		public void Cancel()
		{
			Thread.VolatileWrite(ref count, 0);
			isCanceled = true;
		}
		
		public bool IsCanceled {
			get {
				return isCanceled;
			}
		}
		
		#endregion 
		
	}
}
#endif
