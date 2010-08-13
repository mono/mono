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

#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;

namespace System.Threading
{	
	public class CountdownEvent : IDisposable
	{
		int count;
		readonly int initial;
		ManualResetEventSlim evt = new ManualResetEventSlim (false);
		
		public CountdownEvent (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count is negative");
			this.initial = this.count = count;
		}
		
		public bool Signal ()
		{
			return Signal (1);
		}
		
		public bool Signal (int num)
		{
			if (num <= 0)
				throw new ArgumentOutOfRangeException ("num");
			
			Action<int> check = delegate (int value) {
				if (value < 0)
				throw new InvalidOperationException ("the specified count is larger that CurrentCount");
			};
			
			int newValue;
			if (!ApplyOperation (-num, check, out newValue))
				throw new InvalidOperationException ("The event is already set");
			
			if (newValue == 0) {
				evt.Set ();
				return true;
			}
			
			return false;
		}
		
		public void AddCount ()
		{
			AddCount (1);
		}
		
		public void AddCount (int num)
		{
			if (num < 0)
				throw new ArgumentOutOfRangeException ("num");
			
			if (!TryAddCount (num))
				throw new InvalidOperationException ("The event is already set");
		}
		
		public bool TryAddCount ()
		{
			return TryAddCount (1);
		}
		
		public bool TryAddCount (int num)
		{	
			if (num < 0)
				throw new ArgumentOutOfRangeException ("num");
			
			return ApplyOperation (num, null);
		}
		
		bool ApplyOperation (int num, Action<int> doCheck)
		{
			int temp;
			return ApplyOperation (num, doCheck, out temp);
		}
			
		bool ApplyOperation (int num, Action<int> doCheck, out int newValue)
		{
			int oldCount;
			newValue = 0;
			
			do {
				oldCount = count;
				if (oldCount == 0)
					return false;
				
				newValue = oldCount + num;
				
				if (doCheck != null)
					doCheck (newValue);
			} while (Interlocked.CompareExchange (ref count, newValue, oldCount) != oldCount);
			
			return true;
		}
		
		public void Wait ()
		{
			evt.Wait ();
		}
		
		public void Wait (CancellationToken token)
		{
			evt.Wait (token);
		}
		
		public bool Wait (int timeoutMilli)
		{
			return evt.Wait (timeoutMilli);
		}
		
		public bool Wait(TimeSpan span)
		{
			return evt.Wait (span);
		}
		
		public bool Wait (int timeoutMilli, CancellationToken token)
		{
			return evt.Wait (timeoutMilli, token);
		}
		
		public bool Wait(TimeSpan span, CancellationToken token)
		{
			return evt.Wait (span, token);
		}

		public void Reset ()
		{
			Reset (initial);
		}
		
		public void Reset (int value)
		{
			evt.Reset ();
			Interlocked.Exchange (ref count, value);
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
				return count == 0;
			}
		}
		
		public WaitHandle WaitHandle {
			get {
				return evt.WaitHandle;
			}
		}

		#region IDisposable implementation 
		
		public void Dispose ()
		{
			
		}
		
		protected virtual void Dispose (bool managedRes)
		{
			
		}
		#endregion 	
	}
}
#endif
