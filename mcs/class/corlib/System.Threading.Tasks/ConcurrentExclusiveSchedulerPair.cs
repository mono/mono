// 
// ConcurrentExclusiveSchedulerPair.cs
//
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar <marek.safar@gmail.com>
// 
// Copyright (c) 2011 Jérémie "Garuma" Laval
// Copyright 2012 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_5

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace System.Threading.Tasks
{
	[DebuggerDisplay ("Concurrent={ConcurrentTaskCount}, Exclusive={ExclusiveTaskCount}")]
	[DebuggerTypeProxy (typeof (SchedulerDebuggerView))]
	public class ConcurrentExclusiveSchedulerPair
	{
		sealed class SchedulerDebuggerView
		{
			readonly ConcurrentExclusiveSchedulerPair owner;
			
			public SchedulerDebuggerView (ConcurrentExclusiveSchedulerPair owner)
			{
				this.owner = owner;
			}
			
			public IEnumerable<Task> ScheduledConcurrent {
				get {
					return owner.concurrentTasks;
				}
			}
			
			public IEnumerable<Task> ScheduledExclusive {
				get {
					return owner.exclusiveTasks;
				}
			}
			
			public TaskScheduler TargetScheduler {
				get {
					return owner.target;
				}
			}

		}
		
		readonly int maxConcurrencyLevel;
		readonly int maxItemsPerTask;

		readonly TaskScheduler target;
		readonly TaskFactory factory;
		readonly Action taskHandler;

		readonly ConcurrentQueue<Task> concurrentTasks = new ConcurrentQueue<Task> ();
		readonly ConcurrentQueue<Task> exclusiveTasks = new ConcurrentQueue<Task> ();

		//readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim ();
		readonly TaskCompletionSource<object> completion = new TaskCompletionSource<object> ();
		readonly InnerTaskScheduler concurrent;
		readonly InnerTaskScheduler exclusive;

		int numTask;

		class InnerTaskScheduler : TaskScheduler
		{
			readonly ConcurrentExclusiveSchedulerPair scheduler;
			readonly ConcurrentQueue<Task> queue;

			public InnerTaskScheduler (ConcurrentExclusiveSchedulerPair scheduler,
			                               ConcurrentQueue<Task> queue)
			{
				this.scheduler = scheduler;
				this.queue = queue;
			}
			
			public int TaskCount {
				get {
					return queue.Count;
				}
			}

			public override int MaximumConcurrencyLevel {
				get {
					return scheduler.maxConcurrencyLevel;
				}
			}

			protected internal override void QueueTask (Task t)
			{
				scheduler.DoQueue (t, queue);
			}

			protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
			{
				if (task.Status != TaskStatus.Created)
					return false;

				task.RunSynchronously (scheduler.target);
				return true;
			}

			public void Execute (Task t)
			{
				TryExecuteTask (t);
			}

			[MonoTODO ("Only useful for debugger support")]
			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				throw new NotImplementedException ();
			}
		}

		public ConcurrentExclusiveSchedulerPair () : this (TaskScheduler.Current)
		{
		}

		public ConcurrentExclusiveSchedulerPair (TaskScheduler taskScheduler) : this (taskScheduler, taskScheduler.MaximumConcurrencyLevel)
		{
		}

		public ConcurrentExclusiveSchedulerPair (TaskScheduler taskScheduler, int maxConcurrencyLevel)
			: this (taskScheduler, maxConcurrencyLevel, -1)
		{
		}

		public ConcurrentExclusiveSchedulerPair (TaskScheduler taskScheduler, int maxConcurrencyLevel, int maxItemsPerTask)
		{
			this.target = taskScheduler;
			this.maxConcurrencyLevel = maxConcurrencyLevel;
			this.maxItemsPerTask = maxItemsPerTask;
			this.factory = new TaskFactory (taskScheduler);
			this.taskHandler = InternalTaskProcesser;
			this.concurrent = new InnerTaskScheduler (this, concurrentTasks);
			this.exclusive = new InnerTaskScheduler (this, exclusiveTasks);
		}

		public void Complete ()
		{
			completion.SetResult (null);
		}

		public TaskScheduler ConcurrentScheduler {
			get {
				return concurrent;
			}
		}
		
		private int ConcurrentTaskCount {
			get {
				return concurrent.TaskCount;
			}
		}

		public TaskScheduler ExclusiveScheduler {
			get {
				return exclusive;
			}
		}

		private int ExclusiveTaskCount {
			get {
				return exclusive.TaskCount;
			}
		}

		public Task Completion {
			get {
				return completion.Task;
			}
		}

		void DoQueue (Task task, ConcurrentQueue<Task> queue)
		{
			queue.Enqueue (task);
			SpinUpTasks ();
		}

		void InternalTaskProcesser ()
		{
			int times = 0;
//			const int lockWaitTime = 2;

			while (!concurrentTasks.IsEmpty || !exclusiveTasks.IsEmpty) {
				if (maxItemsPerTask != -1 && ++times == maxItemsPerTask)
					break;

				throw new NotImplementedException ();
/*
				bool locked = false;

				try {
					if (!concurrentTasks.IsEmpty && rwl.TryEnterReadLock (lockWaitTime)) {
						locked = true;
						Task task;
						while (concurrentTasks.TryDequeue (out task)) {
							RunTask (task);
						}
					}
				} finally {
					if (locked) {
						rwl.ExitReadLock ();
						locked = false;
					}
				}

				try {
					if (!exclusiveTasks.IsEmpty && rwl.TryEnterWriteLock (lockWaitTime)) {
						locked = true;
						Task task;
						while (exclusiveTasks.TryDequeue (out task)) {
							RunTask (task);
						}
					}
				} finally {
					if (locked) {
						rwl.ExitWriteLock ();
					}
				}
*/
			}

			// TODO: there's a race here, task adding + spinup check may be done while here
			Interlocked.Decrement (ref numTask);
		}

		void SpinUpTasks ()
		{
			int currentTaskNumber;
			do {
				currentTaskNumber = numTask;
				if (currentTaskNumber >= maxConcurrencyLevel)
					return;
			} while (Interlocked.CompareExchange (ref numTask, currentTaskNumber + 1, currentTaskNumber) != currentTaskNumber);

			factory.StartNew (taskHandler);
		}

		void RunTask (Task task)
		{
			concurrent.Execute (task);
		}
	}
}

#endif