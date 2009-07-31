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
		const int defaultSpinCount = 20;
		const int isSet    = 1;
		const int isNotSet = 0;
		readonly int spinCount;
		readonly SpinWait sw = new SpinWait ();

		int state;

		
		public ManualResetEventSlim () : this(false, 20)
		{
		}
		
		public ManualResetEventSlim (bool initState) : this (initState, 20)
		{
		}
		
		public ManualResetEventSlim (bool initState, int spinCount)
		{
			this.state = initState ? isSet : isNotSet;
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
			Interlocked.Exchange (ref state, isNotSet);
		}
		
		public void Set ()
		{
			Interlocked.Exchange (ref state, isSet);
		}
		
		public void Wait ()
		{
			// First spin
			for (int i = 0; i < spinCount && state == isNotSet; i++)
				sw.SpinOnce ();
			
			// Then, fallback to classic Sleep's yielding
			while (state == isNotSet)
				Thread.Sleep (0);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout",
				                                       "millisecondsTimeout is a negative number other than -1");
			
			if (millisecondsTimeout == -1) {
				Wait ();
				return true;
			}
			
			Watch s = Watch.StartNew ();
			
			// First spin
			for (int i = 0; i < spinCount && state == isNotSet; i++) {
				if (s.ElapsedMilliseconds >= millisecondsTimeout)
					return false;
				
				sw.SpinOnce ();
			}
			
			// Then, fallback to classic Sleep's yielding
			while (state == isNotSet) {
				if (s.ElapsedMilliseconds >= millisecondsTimeout)
					return false;
				
				sw.SpinOnce ();
			}
			
			return true;
		}
		
		public bool Wait (TimeSpan ts)
		{
			return Wait ((int)ts.TotalMilliseconds);
		}
		
		[MonoTODO]
		public WaitHandle WaitHandle {
			get {
				return null;
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
