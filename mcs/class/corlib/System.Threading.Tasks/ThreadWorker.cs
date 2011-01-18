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

#if NET_4_0
using System;
using System.Threading;
using System.Collections.Concurrent;

#if INSIDE_MONO_PARALLEL
using System.Threading.Tasks;
using Watch = System.Diagnostics.Stopwatch;

namespace Mono.Threading.Tasks
#else
namespace System.Threading.Tasks
#endif
{
#if INSIDE_MONO_PARALLEL
	public
#endif
	class ThreadWorker : IDisposable
	{
		Thread workerThread;

		/* This field is used when a TheadWorker have to call Task.Wait
		 * which bring him back here with the static WorkerMethod although
		 * it's more optimized for him to continue calling its own WorkerMethod
		 */
		[ThreadStatic]
		static ThreadWorker autoReference;
		
		readonly IConcurrentDeque<Task> dDeque;
		readonly ThreadWorker[]         others;
		readonly ManualResetEvent       waitHandle;
		readonly IProducerConsumerCollection<Task> sharedWorkQueue;
		readonly ThreadPriority         threadPriority;

		// Flag to tell if workerThread is running
		int started = 0; 
		
		readonly int  workerLength;
		readonly int  workerPosition;
		const    int  maxRetry = 5;
		
		const int sleepThreshold = 100;
		int deepSleepTime = 8;
		
		public ThreadWorker (ThreadWorker[] others,
		                     int workerPosition,
		                     IProducerConsumerCollection<Task> sharedWorkQueue,
		                     IConcurrentDeque<Task> dDeque,
		                     ThreadPriority priority,
		                     ManualResetEvent handle)
		{
			this.others          = others;
			this.dDeque          = dDeque;
			this.sharedWorkQueue = sharedWorkQueue;
			this.workerLength    = others.Length;
			this.workerPosition  = workerPosition;
			this.waitHandle      = handle;
			this.threadPriority  = priority;

			InitializeUnderlyingThread ();
		}

#if INSIDE_MONO_PARALLEL
		protected virtual
#endif
		void InitializeUnderlyingThread ()
		{
			this.workerThread = new Thread (WorkerMethodWrapper);
	
			this.workerThread.IsBackground = true;
			this.workerThread.Priority = threadPriority;
			this.workerThread.Name = "ParallelFxThreadWorker";
		}

#if INSIDE_MONO_PARALLEL
		virtual
#endif
		public void Dispose ()
		{
			Stop ();
			if (workerThread.ThreadState != ThreadState.Stopped)
				workerThread.Abort ();
		}

#if INSIDE_MONO_PARALLEL
		virtual
#endif
		public void Pulse ()
		{
			if (started == 1)
				return;

			// If the thread was stopped then set it in use and restart it
			int result = Interlocked.Exchange (ref started, 1);
			if (result != 0)
				return;

			if (this.workerThread.ThreadState != ThreadState.Unstarted) {
				InitializeUnderlyingThread ();
			}

			workerThread.Start ();
		}

#if INSIDE_MONO_PARALLEL
		virtual
#endif
		public void Stop ()
		{
			// Set the flag to stop so that the while in the thread will stop
			// doing its infinite loop.
			started = 0;
		}
		
#if INSIDE_MONO_PARALLEL
		protected virtual
#endif
		// This is the actual method called in the Thread
		void WorkerMethodWrapper ()
		{
			int sleepTime = 0;
			autoReference = this;
			
			// Main loop
			while (started == 1) {
				bool result = false;

				result = WorkerMethod ();
				if (!result)
					waitHandle.Reset ();

				Thread.Yield ();

				if (result) {
					deepSleepTime = 8;
					sleepTime = 0;
					continue;
				}

				// If we are spinning too much, have a deeper sleep
				if (++sleepTime > sleepThreshold && sharedWorkQueue.Count == 0) {
					waitHandle.WaitOne ((deepSleepTime =  deepSleepTime >= 0x4000 ? 0x4000 : deepSleepTime << 1));
				}
			}

			started = 0;
		}

#if INSIDE_MONO_PARALLEL
		protected virtual
#endif
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
						waitHandle.Set ();
					}

					// Now we process our work
					while (dDeque.PopBottom (out value) == PopResult.Succeed) {
						waitHandle.Set ();
						if (value != null) {
							value.Execute (ChildWorkAdder);
							result = true;
						}
					}
				}

				// When we have finished, steal from other worker
				ThreadWorker other;
				
				// Repeat the operation a little so that we can let other things process.
				for (int j = 0; j < maxRetry; ++j) {
					int len = workerLength + workerPosition;
					// Start stealing with the ThreadWorker at our right to minimize contention
					for (int it = workerPosition + 1; it < len; ++it) {
						int i = it % workerLength;
						if ((other = others [i]) == null || other == this)
							continue;
						
						// Maybe make this steal more than one item at a time, see TODO.
						if (other.dDeque.PopTop (out value) == PopResult.Succeed) {
							waitHandle.Set ();
							hasStolenFromOther = true;
							if (value != null) {
								value.Execute (ChildWorkAdder);
								result = true;
							}
						}
					}
				}
			} while (sharedWorkQueue.Count > 0 || hasStolenFromOther);
			
			return result;
		}

#if !INSIDE_MONO_PARALLEL
		// Almost same as above but with an added predicate and treating one item at a time. 
		// It's used by Scheduler Participate(...) method for special waiting case like
		// Task.WaitAll(someTasks) or Task.WaitAny(someTasks)
		// Predicate should be really fast and not blocking as it is called a good deal of time
		// Also, the method skip tasks that are LongRunning to avoid blocking (Task are not LongRunning by default)
		public static void ParticipativeWorkerMethod (Task self,
		                                              ManualResetEventSlim predicateEvt,
		                                              int millisecondsTimeout,
		                                              IProducerConsumerCollection<Task> sharedWorkQueue,
		                                              ThreadWorker[] others,
		                                              ManualResetEvent evt)
		{
			const int stage1 = 5, stage2 = 0;
			int tries = 8;
			WaitHandle[] handles = null;
			Watch watch = Watch.StartNew ();

			while (!predicateEvt.IsSet && watch.ElapsedMilliseconds < millisecondsTimeout) {
				Task value;
				
				// If we are in fact a normal ThreadWorker, use our own deque
				if (autoReference != null) {
					while (autoReference.dDeque.PopBottom (out value) == PopResult.Succeed && value != null) {
						evt.Set ();
						if (CheckTaskFitness (self, value))
							value.Execute (autoReference.ChildWorkAdder);
						else {
							sharedWorkQueue.TryAdd (value);
							evt.Set ();
						}

						if (predicateEvt.IsSet || watch.ElapsedMilliseconds > millisecondsTimeout)
							return;
					}
				}

				int count = sharedWorkQueue.Count;

				// Dequeue only one item as we have restriction
				while (--count >= 0 && sharedWorkQueue.TryTake (out value) && value != null) {
					evt.Set ();
					if (CheckTaskFitness (self, value))
						value.Execute (null);
					else {
						if (autoReference == null)
							sharedWorkQueue.TryAdd (value);
						else
							autoReference.dDeque.PushBottom (value);
						evt.Set ();
					}

					if (predicateEvt.IsSet || watch.ElapsedMilliseconds > millisecondsTimeout)
						return;
				}

				// First check to see if we comply to predicate
				if (predicateEvt.IsSet || watch.ElapsedMilliseconds > millisecondsTimeout)
					return;
				
				// Try to complete other work by stealing since our desired tasks may be in other worker
				ThreadWorker other;
				for (int i = 0; i < others.Length; i++) {
					if ((other = others [i]) == autoReference || other == null)
						continue;

					if (other.dDeque.PopTop (out value) == PopResult.Succeed && value != null) {
						evt.Set ();
						if (CheckTaskFitness (self, value))
							value.Execute (null);
						else {
							if (autoReference == null)
								sharedWorkQueue.TryAdd (value);
							else
								autoReference.dDeque.PushBottom (value);
							evt.Set ();
						}
					}

					if (predicateEvt.IsSet || watch.ElapsedMilliseconds > millisecondsTimeout)
						return;
				}

				if (--tries > stage1)
					Thread.Yield ();
				else if (tries >= stage2)
					predicateEvt.Wait (ComputeTimeout (100, millisecondsTimeout, watch));
				else {
					if (tries == stage2 - 1)
						handles = new [] { predicateEvt.WaitHandle, evt };
					WaitHandle.WaitAny (handles, ComputeTimeout (1000, millisecondsTimeout, watch));
				}
			}
		}

		static bool CheckTaskFitness (Task self, Task t)
		{
			return ((t.CreationOptions & TaskCreationOptions.LongRunning) == 0 && t.Id < self.Id) || t.Parent == self || t == self;
		}
#else
		public static ThreadWorker AutoReference {
			get {
				return autoReference;
			}
			set {
				autoReference = value;
			}
		}
#endif

#if INSIDE_MONO_PARALLEL
		protected virtual
#endif
		internal void ChildWorkAdder (Task t)
		{
			dDeque.PushBottom (t);
			waitHandle.Set ();
		}

		static int ComputeTimeout (int proposed, int timeout, Watch watch)
		{
			return timeout == -1 ? proposed : System.Math.Min (proposed, System.Math.Max (0, (int)(timeout - watch.ElapsedMilliseconds)));
		}
		
		public bool Finished {
			get {
				return started == 0;
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
