// 
// TaskScheduler.cs
//  
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar <marek.safar@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

#if NET_4_0

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Threading.Tasks
{
	[DebuggerDisplay ("Id={Id}")]
	[DebuggerTypeProxy (typeof (TaskSchedulerDebuggerView))]
	public abstract class TaskScheduler
	{
		sealed class TaskSchedulerDebuggerView
		{
			readonly TaskScheduler scheduler;

			public TaskSchedulerDebuggerView (TaskScheduler scheduler)
			{
				this.scheduler = scheduler;
			}

			public IEnumerable<Task> ScheduledTasks {
				get {
					return scheduler.GetScheduledTasks ();
				}
			}
		}

		static readonly TaskScheduler defaultScheduler = new TpScheduler ();
		
		[ThreadStatic]
		static TaskScheduler currentScheduler;
		
		int id;
		static int lastId = int.MinValue;
		
		public static event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;
		
		protected TaskScheduler ()
		{
			this.id = Interlocked.Increment (ref lastId);
		}
		
		public static TaskScheduler FromCurrentSynchronizationContext ()
		{
			var syncCtx = SynchronizationContext.Current;
			if (syncCtx == null)
				throw new InvalidOperationException ("The current SynchronizationContext is null and cannot be used as a TaskScheduler");

			return new SynchronizationContextScheduler (syncCtx);
		}
		
		public static TaskScheduler Default  {
			get {
				return defaultScheduler;
			}
		}
		
		public static TaskScheduler Current  {
			get {
				if (currentScheduler != null)
					return currentScheduler;
				
				return defaultScheduler;
			}
			internal set {
				currentScheduler = value;
			}
		}
		
		public int Id {
			get {
				return id;
			}
		}
		
		public virtual int MaximumConcurrencyLevel {
			get {
				return int.MaxValue;
			}
		}

		protected abstract IEnumerable<Task> GetScheduledTasks ();
		protected internal abstract void QueueTask (Task task);

		protected internal virtual bool TryDequeue (Task task)
		{
			return false;
		}

		internal protected bool TryExecuteTask (Task task)
		{
			if (task.IsCompleted)
				return false;

			if (task.Status == TaskStatus.WaitingToRun) {
				task.Execute ();
				if (task.WaitOnChildren ())
					task.Wait ();

				return true;
			}

			return false;
		}

		protected abstract bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued);

		internal bool RunInline (Task task, bool taskWasPreviouslyQueued)
		{
			if (!TryExecuteTaskInline (task, taskWasPreviouslyQueued))
				return false;

			if (!task.IsCompleted)
				throw new InvalidOperationException ("The TryExecuteTaskInline call to the underlying scheduler succeeded, but the task body was not invoked");

			return true;
		}

		internal static UnobservedTaskExceptionEventArgs FireUnobservedEvent (Task task, AggregateException e)
		{
			UnobservedTaskExceptionEventArgs args = new UnobservedTaskExceptionEventArgs (e);
			
			EventHandler<UnobservedTaskExceptionEventArgs> temp = UnobservedTaskException;
			if (temp == null)
				return args;
			
			temp (task, args);
			
			return args;
		}
	}
}
#endif
