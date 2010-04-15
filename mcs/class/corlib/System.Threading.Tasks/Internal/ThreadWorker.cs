#if NET_4_0 || BOOTSTRAP_NET_4_0
// ThreadWorker.cs
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
using System.Threading;
using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
	internal class ThreadWorker : IDisposable
	{
		static Random r = new Random ();
		
		Thread workerThread;
		
		readonly          ThreadWorker[]        others;
		internal readonly IDequeOperations<Task>    dDeque;
		readonly          IProducerConsumerCollection<Task> sharedWorkQueue;
		readonly          Action<Task>          childWorkAdder;
		
		// Flag to tell if workerThread is running
		int started = 0; 
		
		readonly bool isLocal;
		readonly int  workerLength;
		readonly int  stealingStart;
		const    int  maxRetry = 5;
		
		#region Sleep related fields
		readonly SpinWait wait = new SpinWait ();
		const int sleepThreshold = 100000;
		#endregion
		
		Action threadInitializer;
		
		public ThreadWorker (IScheduler sched, ThreadWorker[] others, IProducerConsumerCollection<Task> sharedWorkQueue,
		                     int maxStackSize, ThreadPriority priority)
			: this (sched, others, sharedWorkQueue, true, maxStackSize, priority)
		{
		}
		
		public ThreadWorker (IScheduler sched, ThreadWorker[] others, IProducerConsumerCollection<Task> sharedWorkQueue,
		                     bool createThread, int maxStackSize, ThreadPriority priority)
		{
			this.others          = others;

			this.dDeque = new CyclicDeque<Task> ();
			
			this.sharedWorkQueue = sharedWorkQueue;
			this.workerLength    = others.Length;
			this.isLocal         = !createThread;
			
			this.childWorkAdder = delegate (Task t) { 
				dDeque.PushBottom (t);
				sched.PulseAll ();
			};
			
			// Find the stealing start index randomly (then the traversal
			// will be done in Round-Robin fashion)
			do {
				this.stealingStart = r.Next(0, workerLength);
			} while (others[stealingStart] == this);
			
			InitializeUnderlyingThread (maxStackSize, priority);
		}
		
		void InitializeUnderlyingThread (int maxStackSize, ThreadPriority priority)
		{
			threadInitializer = delegate {
				// Special case of the participant ThreadWorker
				if (isLocal) {			
					this.workerThread = Thread.CurrentThread;
					return;
				}
				
				this.workerThread = (maxStackSize == 0) ? new Thread (WorkerMethodWrapper) :
					new Thread (WorkerMethodWrapper, maxStackSize);
	
				this.workerThread.IsBackground = true;
				this.workerThread.Priority = priority;
				this.workerThread.Name = "ParallelFxThreadWorker";
			};
			threadInitializer ();
		}

		public void Dispose ()
		{
			Stop ();
			if (!isLocal && workerThread.ThreadState != ThreadState.Stopped)
				workerThread.Abort ();
		}
		
		public void Pulse ()
		{
			// If the thread was stopped then set it in use and restart it
			int result = Interlocked.Exchange (ref started, 1);
			if (result != 0)
				return;
			if (!isLocal) {
				if (this.workerThread.ThreadState != ThreadState.Unstarted) {
					threadInitializer ();
				}
				workerThread.Start ();
			}
		}
		
		public void Stop ()
		{
			// Set the flag to stop so that the while in the thread will stop
			// doing its infinite loop.
			started = 0;
		}
		
		// This is the actual method called in the Thread
		void WorkerMethodWrapper ()
		{
			int sleepTime = 0;
			
			// Main loop
			while (started == 1) {
				bool result = false;
				try {
					result = WorkerMethod ();
				} catch (Exception e) {
					Console.WriteLine (e.ToString ());
				}
				
				// Wait a little and if the Thread has been more sleeping than working shut it down
				wait.SpinOnce ();
				if (result)
					sleepTime = 0;
				if (sleepTime++ > sleepThreshold) 
					break;
			}

			started = 0;
		}
		
		// Main method, used to do all the logic of retrieving, processing and stealing work.
		bool WorkerMethod ()
		{		
			bool result = false;
			bool hasStolenFromOther;
			do {
				hasStolenFromOther = false;
				
				Task value;
				
				// We fill up our work deque concurrently with other ThreadWorker
				while (sharedWorkQueue.Count > 0) {
					while (sharedWorkQueue.TryTake (out value)) {
						dDeque.PushBottom (value);
					}
					
					// Now we process our work
					while (dDeque.PopBottom (out value) == PopResult.Succeed) {
						if (value != null) {
							value.Execute (childWorkAdder);
							result = true;
						}
					}
				}
				
				// When we have finished, steal from other worker
				ThreadWorker other;
				
				// Repeat the operation a little so that we can let other things process.
				for (int j = 0; j < maxRetry; j++) {
					// Start stealing with the ThreadWorker at our right to minimize contention
					for (int it = stealingStart; it < stealingStart + workerLength; it++) {
						int i = it % workerLength;
						if ((other = others [i]) == null || other == this)
							continue;
						
						// Maybe make this steal more than one item at a time, see TODO.
						if (other.dDeque.PopTop (out value) == PopResult.Succeed) {
							hasStolenFromOther = true;
							if (value != null) {
								value.Execute (childWorkAdder);
								result = true;
							}
						}
					}
				}
			} while (sharedWorkQueue.Count > 0 || hasStolenFromOther);
			
			return result;
		}
		
		// Almost same as above but with an added predicate and treating one item at a time. 
		// It's used by Scheduler Participate(...) method for special waiting case like
		// Task.WaitAll(someTasks) or Task.WaitAny(someTasks)
		// Predicate should be really fast and not blocking as it is called a good deal of time
		// Also, the method skip tasks that are LongRunning to avoid blocking (Task are not LongRunning by default)
		public static void WorkerMethod (Func<bool> predicate, IProducerConsumerCollection<Task> sharedWorkQueue,
		                                 ThreadWorker[] others)
		{
			while (!predicate ()) {
				Task value;
				
				// Dequeue only one item as we have restriction
				if (sharedWorkQueue.TryTake (out value)) {
					if (value != null) {
						if (CheckTaskFitness (value))
							value.Execute (null);
						else
							sharedWorkQueue.TryAdd (value);
					}
				}
				
				// First check to see if we comply to predicate
				if (predicate ()) {
					return;
				}
				
				// Try to complete other work by stealing since our desired tasks may be in other worker
				ThreadWorker other;
				for (int i = 0; i < others.Length; i++) {
					if ((other = others [i]) == null)
						continue;
					
					if (other.dDeque.PopTop (out value) == PopResult.Succeed) {
						if (value != null) {
							if (CheckTaskFitness (value))
								value.Execute (null);
							else
								sharedWorkQueue.TryAdd (value);
						}
					}
					
					if (predicate ()) {
						return;
					}
				}
			}
		}
		
		static bool CheckTaskFitness (Task t)
		{
			return (t.CreationOptions | TaskCreationOptions.LongRunning) > 0;
		}
		
		public bool Finished {
			get {
				return started == 0;
			}
		}
		
		public bool IsLocal {
			get {
				return isLocal;
			}
		}
		
		public int Id {
			get {
				return workerThread.ManagedThreadId;
			}
		}
		
		public bool Equals (ThreadWorker other)
		{
			return (other == null) ? false : object.ReferenceEquals (this.dDeque, other.dDeque);	
		}
		
		public override bool Equals (object obj)
		{
			ThreadWorker temp = obj as ThreadWorker;
			return temp == null ? false : Equals (temp);
		}
		
		public override int GetHashCode ()
		{
			return workerThread.ManagedThreadId.GetHashCode ();
		}
	}
}
#endif
