// Scheduler.cs
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
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Mono.Threading.Tasks
{
	public class FixedTaskScheduler: TaskScheduler, IMonoTaskScheduler
	{
		readonly IProducerConsumerCollection<Task> workQueue;
		readonly ThreadWorker[]        workers;
		readonly ManualResetEvent      pulseHandle = new ManualResetEvent (false);

		public FixedTaskScheduler ()
			: this (Environment.ProcessorCount, ThreadPriority.Normal)
		{
			
		}
		
		public FixedTaskScheduler (int maxWorker, ThreadPriority priority)
		{
			workQueue = new ConcurrentQueue<Task> ();
			workers = new ThreadWorker [maxWorker];
			
			for (int i = 0; i < maxWorker; i++) {
				workers [i] = new ThreadWorker (workers, i, workQueue, new CyclicDeque<Task> (), priority, pulseHandle);
				workers [i].Pulse ();
			}
		}

		protected override void QueueTask (Task t)
		{
			// Add to the shared work pool
			workQueue.TryAdd (t);
			// Wake up some worker if they were asleep
			PulseAll ();
		}

		public void MonoParticipateUntil (Task task)
		{
			if (task.IsCompleted)
				return;

			ManualResetEventSlim evt = new ManualResetEventSlim (false);
			task.ContinueWith (_ => evt.Set (), TaskContinuationOptions.ExecuteSynchronously);
			if (evt.IsSet || task.IsCompleted)
				return;
			
			ParticipateUntilInternal (task, evt, -1);
		}
		
		public bool MonoParticipateUntil (Task task, ManualResetEventSlim evt, int millisecondsTimeout)
		{
			if (task.IsCompleted)
				return false;

			bool isFromPredicate = true;
			task.ContinueWith (_ => { isFromPredicate = false; evt.Set (); }, TaskContinuationOptions.ExecuteSynchronously);

			ParticipateUntilInternal (task, evt, millisecondsTimeout);

			if (task.IsCompleted)
				return false;

			return isFromPredicate;
		}
		
		public void ParticipateUntilInternal (Task self, ManualResetEventSlim evt, int millisecondsTimeout)
		{
			ThreadWorker.ParticipativeWorkerMethod (self, evt, millisecondsTimeout, workQueue, workers, pulseHandle, (a, b) => true);
		}

		static bool TaskCompletedPredicate (Task self)
		{
			return self.IsCompleted;
		}
		
		public void PulseAll ()
		{
			pulseHandle.Set ();
		}
		
		public void Dispose ()
		{
			foreach (ThreadWorker w in workers)
				w.Dispose ();
		}

		#region Scheduler dummy stubs
		protected override System.Collections.Generic.IEnumerable<Task> GetScheduledTasks ()
		{
			throw new System.NotImplementedException();
		}

		protected override bool TryDequeue (Task task)
		{
			throw new System.NotImplementedException();
		}

		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			task.Execute (null);
			return true;
		}
		
		public override int MaximumConcurrencyLevel {
			get {
				return base.MaximumConcurrencyLevel;
			}
		}
		#endregion
	}
}
#endif
