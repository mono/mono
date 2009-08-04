#if NET_4_0
// SpinLock.cs
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
using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	public struct SpinLock
	{
		const int isFree = 0;
		const int isOwned = 1;
		int lockState;
		readonly SpinWait sw;
		int threadWhoTookLock;
		readonly bool isThreadOwnerTrackingEnabled;
		
		public bool IsThreadOwnerTrackingEnabled {
			get {
				return isThreadOwnerTrackingEnabled;
			}
		}
		
		public bool IsHeld {
			get {
				return lockState == isOwned;
			}
		}
		
		public bool IsHeldByCurrentThread {
			get {
				if (isThreadOwnerTrackingEnabled)
					return lockState == isOwned && Thread.CurrentThread.ManagedThreadId == threadWhoTookLock;
				else
					return lockState == isOwned;
			}
		}

		public SpinLock (bool trackId)
		{
			this.isThreadOwnerTrackingEnabled = trackId;
			this.threadWhoTookLock = 0;
			this.lockState = isFree;
			this.sw = new SpinWait();
		}
		
		void CheckAndSetThreadId ()
		{
			if (threadWhoTookLock == Thread.CurrentThread.ManagedThreadId)
				throw new LockRecursionException("The current thread has already acquired this lock.");
			threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
		}
		
		public void Enter (ref bool lockTaken)
		{
			try {
				Enter ();
				lockTaken = lockState == isOwned && Thread.CurrentThread.ManagedThreadId == threadWhoTookLock;
			} catch {
				lockTaken = false;
			}
		}
		
		internal void Enter () 
		{
			int result = Interlocked.Exchange (ref lockState, isOwned);
			
			//Thread.BeginCriticalRegion();
			while (result == isOwned) {
				// If resource available, set it to in-use and return
				if (result == isFree) {
					result = Interlocked.Exchange (ref lockState, isOwned);
					if (result == isFree)
						break;
				}
				
				// Efficiently spin, until the resource looks like it might 
				// be free. NOTE: Just reading here (as compared to repeatedly 
				// calling Exchange) improves performance because writing 
				// forces all CPUs to update this value
				//while (Thread.VolatileRead (ref lockState) == isOwned) {
					sw.SpinOnce ();
				//}
			}
			
			CheckAndSetThreadId ();
		}
		
		bool TryEnter ()
		{
			//Thread.BeginCriticalRegion();

			// If resource available, set it to in-use and return
			if (Interlocked.Exchange (ref lockState, isOwned) == isFree) {
				CheckAndSetThreadId ();
				return true;
			}
			return false;
		}
		
		public void TryEnter (ref bool lockTaken)
		{
			try {
				lockTaken = TryEnter ();
			} catch {
				lockTaken = false;
			}
		}
		
		public void TryEnter (TimeSpan timeout, ref bool lockTaken)
		{
			TryEnter ((int)timeout.TotalMilliseconds, ref lockTaken);
		}
		
		public void TryEnter (int milliSeconds, ref bool lockTaken)
		{
			//Thread.BeginCriticalRegion();
			
			Watch sw = Watch.StartNew ();
			
			while (sw.ElapsedMilliseconds < milliSeconds) {
				TryEnter (ref lockTaken);
			}
			sw.Stop ();
		}

		//[ReliabilityContractAttribute]
		public void Exit () 
		{ 
			Exit (false);
		}

		public void Exit (bool flushReleaseWrites) 
		{ 
			threadWhoTookLock = int.MinValue;
			
			// Mark the resource as available
			if (!flushReleaseWrites) {
				lockState = isFree;
			} else {
				Interlocked.Exchange (ref lockState, isFree);
			}
			//Thread.EndCriticalRegion();
		}
	}
	
	// Wraps a SpinLock in a reference when we need to pass
	// around the lock
	internal class SpinLockWrapper
	{
		public readonly SpinLock Lock = new SpinLock (false);
	}
}
#endif
