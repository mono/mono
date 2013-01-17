//
// System.Runtime.Remoting.Channels.RemotingThreadPool.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2005 (C) Copyright, Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Threading;

namespace System.Runtime.Remoting.Channels
{
	internal class RemotingThreadPool
	{
		const int ThreadLimit = 1000;
		const int ThreadWaitTime = 20000; //ms
		const int PoolGrowDelay = 500; // ms
		const int MinThreads = 3;

		int freeThreads;
		int poolUsers;
		Queue workItems = new Queue ();
		AutoResetEvent threadDone = new AutoResetEvent (false);
		ArrayList runningThreads = new ArrayList ();
		 
#if TARGET_JVM
		volatile 
#endif
		bool stopped = false;
		
		static object globalLock = new object ();
		static RemotingThreadPool sharedPool;
		
		public static RemotingThreadPool GetSharedPool ()
		{
			lock (globalLock) {
				if (sharedPool == null)
					sharedPool = new RemotingThreadPool ();
				sharedPool.poolUsers++;
			}
			return sharedPool;
		}
		
		public void Free ()
		{
			lock (globalLock) {
				if (--poolUsers > 0)
					return;
				
				lock (workItems) {
					stopped = true;
					threadDone.Set ();
					workItems.Clear ();
					foreach (Thread t in runningThreads)
#if !TARGET_JVM
						t.Abort ();
#else
						t.Interrupt();
#endif
					runningThreads.Clear ();
				}
				if (this == sharedPool)
					sharedPool = null;
			}
		}
		
		public bool RunThread (ThreadStart threadStart)
		{
			lock (workItems) {
				if (stopped)
					throw new RemotingException ("Server channel stopped.");
					
				if (freeThreads > 0) {
					freeThreads--;
					workItems.Enqueue (threadStart);
					Monitor.Pulse (workItems);
					return true;
				} else if (runningThreads.Count < MinThreads) {
					workItems.Enqueue (threadStart);
					StartPoolThread ();
					return true;
				}
			}
			
			// Try again some ms later, and if there are still no free threads,
			// then create a new one

			threadDone.WaitOne (PoolGrowDelay, false);
			
			lock (workItems) {
				if (stopped)
					throw new RemotingException ("Server channel stopped.");
				
				if (freeThreads > 0) {
					freeThreads--;
					workItems.Enqueue (threadStart);
					Monitor.Pulse (workItems);
				} else {
					if (runningThreads.Count >= ThreadLimit)
						return false;
					workItems.Enqueue (threadStart);
					StartPoolThread ();
				}
			}
			return true;
		}
		
		void StartPoolThread ()
		{
			Thread thread = new Thread (new ThreadStart (PoolThread));
			runningThreads.Add (thread);
			thread.IsBackground = true;
			thread.Start ();
		}
		
		void PoolThread ()
		{
#if !TARGET_JVM
			while (true) {
#else
			while (!stopped) 
			{
#endif
				ThreadStart work = null;
				do {
					lock (workItems) {
						if (workItems.Count > 0)
							work = (ThreadStart) workItems.Dequeue ();
						else {
							freeThreads ++;
							threadDone.Set ();
							if (!Monitor.Wait (workItems, ThreadWaitTime)) {
								// Maybe it timed out when the work was being queued.
								if (workItems.Count > 0) {
									work = (ThreadStart) workItems.Dequeue ();
								} else {
									freeThreads --;
									if (freeThreads == 0) threadDone.Reset ();
									runningThreads.Remove (Thread.CurrentThread);
									return;
								}
							}
						}
					}
				} while (work == null);
				try {
					work ();
				}
				catch (Exception)
				{
#if DEBUG
					Console.WriteLine("The exception was caught during RemotingThreadPool.PoolThread - work: {0}, {1}", ex.GetType(), ex.Message);
#endif
				}
			}
		}
	}
}

