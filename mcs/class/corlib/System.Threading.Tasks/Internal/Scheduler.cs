#if NET_4_0
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

using System;
using System.Linq;
//using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	internal class Scheduler: IScheduler
	{
		ConcurrentStack<Task> workQueue;
		//ConcurrentStack<Task>[] workBuffers;
		ThreadWorker[]        workers;
		//ThreadWorker          participant;
		bool                  isPulsable = true;
		
		public Scheduler(int maxWorker, int maxStackSize, ThreadPriority priority)
		{
			//workQueue = new OptimizedStack<Task>(maxWorker);
			workQueue = new ConcurrentStack<Task>();
			workers = new ThreadWorker[maxWorker];
			
			for (int i = 0; i < maxWorker; i++) {
				workers[i] = new ThreadWorker(this, workers, workQueue, maxStackSize, priority);
			}
		}
		
		public void AddWork(Task t)
		{
			// Add to the shared work pool
			workQueue.Push(t);
			// Wake up some worker if they were asleep
			PulseAll();
		}
		
		public void ParticipateUntil(Task task)
		{
			if (AreTasksFinished(task))
				return;
			
			ParticipateUntil(delegate {
				return AreTasksFinished(task);
			});
		}
		
		public bool ParticipateUntil(Task task, Func<bool> predicate)
		{
			if (AreTasksFinished(task))
				return false;
			
			bool isFromPredicate = false;
			
			ParticipateUntil(delegate {
				if (predicate()) {
					isFromPredicate = true;
					return true;
				}
				return AreTasksFinished(task);	
			});
				
			return isFromPredicate;
		}
		
		// Called with Task.WaitAll(someTasks) or Task.WaitAny(someTasks) so that we can remove ourselves
		// also when our wait condition is ok
		public void ParticipateUntil(Func<bool> predicate)
		{	
			ThreadWorker.WorkerMethod(predicate, workQueue, workers);
		}
		
		public void PulseAll()
		{
			if (isPulsable) {
				foreach (ThreadWorker worker in workers) {
					if (worker != null)
						worker.Pulse();	
				}
			}
		}
		
		public void InhibitPulse()
		{
			isPulsable = false;
		}
		
		public void UnInhibitPulse() 
		{
			isPulsable = true;
		}

		public void Dispose()
		{
			foreach (ThreadWorker w in workers) {
				w.Dispose();
			}
		}
		
		bool AreTasksFinished(Task parent)
		{
			if (!parent.IsCompleted)
				return false;
			
			return parent.ChildTasks;
		}
	}
}
#endif
