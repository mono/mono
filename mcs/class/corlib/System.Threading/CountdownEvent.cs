// CountdownEvent.cs
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
	[System.Diagnostics.DebuggerDisplayAttribute ("Initial Count={InitialCount}, Current Count={CurrentCount}")]
	public class CountdownEvent : IDisposable
	{
		int initialCount;
		int initial;
		ManualResetEventSlim evt;
		
		public CountdownEvent (int initialCount)
		{
			if (initialCount < 0)
				throw new ArgumentOutOfRangeException ("initialCount");

			evt = new ManualResetEventSlim (initialCount == 0);
			this.initial = this.initialCount = initialCount;
		}

		public int CurrentCount {
			get {
				return initialCount;
			}
		}
		
		public int InitialCount {
			get {
				return initial;
			}
		}
			
		public bool IsSet {
			get {
				return initialCount == 0;
			}
		}
		
		public WaitHandle WaitHandle {
			get {
				return evt.WaitHandle;
			}
		}
		
		public bool Signal ()
		{
			return Signal (1);
		}
		
		public bool Signal (int signalCount)
		{
			if (signalCount <= 0)
				throw new ArgumentOutOfRangeException ("signalCount");

			CheckDisposed ();

			int newValue;
			if (!ApplyOperation (-signalCount, out newValue))
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
		
		public void AddCount (int signalCount)
		{
			if (!TryAddCount (signalCount))
				throw new InvalidOperationException ("The event is already signaled and cannot be incremented");
		}
		
		public bool TryAddCount ()
		{
			return TryAddCount (1);
		}
		
		public bool TryAddCount (int signalCount)
		{	
			if (signalCount <= 0)
				throw new ArgumentOutOfRangeException ("signalCount");

			CheckDisposed ();

			int temp;
			return ApplyOperation (signalCount, out temp);
		}
		
		bool ApplyOperation (int num, out int newValue)
		{
			int oldCount;
			
			do {
				oldCount = initialCount;
				if (oldCount == 0) {
					newValue = 0;
					return false;
				}
				
				newValue = oldCount + num;

				if (newValue < 0)
					return false;
			} while (Interlocked.CompareExchange (ref initialCount, newValue, oldCount) != oldCount);
			
			return true;
		}
		
		public void Wait ()
		{
			evt.Wait ();
		}
		
		public void Wait (CancellationToken cancellationToken)
		{
			evt.Wait (cancellationToken);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			return evt.Wait (millisecondsTimeout);
		}
		
		public bool Wait(TimeSpan timeout)
		{
			return evt.Wait (timeout);
		}
		
		public bool Wait (int millisecondsTimeout, CancellationToken cancellationToken)
		{
			return evt.Wait (millisecondsTimeout, cancellationToken);
		}
		
		public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
		{
			return evt.Wait (timeout, cancellationToken);
		}

		public void Reset ()
		{
			Reset (initial);
		}
		
		public void Reset (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			CheckDisposed ();

			initialCount = initial = count;
			if (count == 0)
				evt.Set ();
			else
				evt.Reset ();
		}
		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				evt.Dispose ();
		}

		void CheckDisposed ()
		{
			if (evt.disposed.Value)
				throw new ObjectDisposedException ("CountdownEvent");
		}
	}
}
#endif
