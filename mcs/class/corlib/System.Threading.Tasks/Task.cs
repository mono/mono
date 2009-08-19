#if NET_4_0
// Task.cs
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
	public class Task : IDisposable, IAsyncResult, ICancelableOperation
	{
		// With this attribute each thread has its own value so that it's correct for our Schedule code
		// and for Parent property.
		[System.ThreadStatic]
		static Task         current;
		[System.ThreadStatic]
		static Action<Task> childWorkAdder;
		
		static int          id = -1;
		static TaskFactory  defaultFactory = new TaskFactory ();
		
		CountdownEvent childTasks = new CountdownEvent (1);
		
		Task parent = current;
		
		int                 taskId;
		bool                respectParentCancellation;
		TaskCreationOptions taskCreationOptions;
		
		IScheduler          scheduler;
		TaskScheduler       taskScheduler;
		
		volatile Exception  exception;
		volatile bool       exceptionObserved;
		volatile TaskStatus status;
		
		Action<object> action;
		object         state;
		EventHandler   completed;
		
		CancellationTokenSource src = new CancellationTokenSource ();
			
		
		public Task (Action action) : this (action, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, TaskCreationOptions options) : this ((o) => action (), null, options)
		{
			
		}
		
		public Task (Action<object> action, object state) : this (action, state, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action<object> action, object state, TaskCreationOptions options)
		{
			this.taskCreationOptions = options;
			this.action = action == null ? EmptyFunc : action;
			this.state = state;
			this.taskId = Interlocked.Increment (ref id);
			this.status = TaskStatus.Created;

			// Process taskCreationOptions
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.DetachedFromParent))
				parent = null;
			else if (parent != null)
				parent.AddChild ();

			respectParentCancellation =
				CheckTaskOptions (taskCreationOptions, TaskCreationOptions.RespectParentCancellation);
		}
		
		~Task ()
		{
			if (exception != null && !exceptionObserved)
				throw exception;
		}

		bool CheckTaskOptions (TaskCreationOptions opt, TaskCreationOptions member)
		{
			return (opt & member) == member;
		}

		static void EmptyFunc (object o)
		{
		}
		
		#region Start
		public void Start ()
		{
			Start (TaskScheduler.Current);
		}
		
		public void Start (TaskScheduler tscheduler)
		{
			this.taskScheduler = tscheduler;
			Start (ProxifyScheduler (tscheduler));
		}
		
		void Start (IScheduler scheduler)
		{
			this.scheduler = scheduler;
			status = TaskStatus.WaitingForActivation;
			Schedule ();
		}
		
		IScheduler ProxifyScheduler (TaskScheduler tscheduler)
		{
			IScheduler sched = tscheduler as IScheduler;
			return sched != null ? sched : new SchedulerProxy (tscheduler);
		}
		
		public void RunSynchronously ()
		{
			RunSynchronously (TaskScheduler.Current);
		}
		
		public void RunSynchronously (TaskScheduler tscheduler) 
		{
			// Adopt this scheme for the moment
			ThreadStart ();
		}
		#endregion
		
		#region ContinueWith
		public Task ContinueWith (Action<Task> a)
		{
			return ContinueWith (a, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task> a, TaskContinuationOptions kind)
		{
			return ContinueWith (a, kind, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task> a, TaskScheduler scheduler)
		{
			return ContinueWith (a, TaskContinuationOptions.None, scheduler);
		}
		
		public Task ContinueWith (Action<Task> a, TaskContinuationOptions kind, TaskScheduler scheduler)
		{
			Task continuation = new Task ((o) => a ((Task)o), this, GetCreationOptions (kind));
			ContinueWithCore (continuation, kind, scheduler);
			return continuation;
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a)
		{
			return ContinueWith<TResult> (a, TaskContinuationOptions.None);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, TaskContinuationOptions options)
		{
			return ContinueWith<TResult> (a, options, TaskScheduler.Current);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, TaskScheduler scheduler)
		{
			return ContinueWith<TResult> (a, TaskContinuationOptions.None, scheduler);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, TaskContinuationOptions kind, TaskScheduler scheduler)
		{
			Task<TResult> t = new Task<TResult> ((o) => a ((Task)o), this, GetCreationOptions (kind));
			
			ContinueWithCore (t, kind, scheduler);
			
			return t;
		}
		
		internal void ContinueWithCore (Task continuation, TaskContinuationOptions kind, TaskScheduler scheduler)
		{
			ContinueWithCore (continuation, kind, scheduler, () => true);
		}
		
		internal void ContinueWithCore (Task continuation, TaskContinuationOptions kind,
		                                TaskScheduler scheduler, Func<bool> predicate)
		{
			// Already set the scheduler so that user can call Wait and that sort of stuff
			continuation.taskScheduler = scheduler;
			continuation.scheduler = ProxifyScheduler (scheduler);
			
			AtomicBoolean launched = new AtomicBoolean ();
			EventHandler action = delegate {
				if (!predicate ()) return;
				
				if (!launched.Value && !launched.Exchange (true)) {
					if (!ContinuationStatusCheck (kind)) {
						continuation.Cancel ();
						continuation.CancelReal ();
						continuation.Dispose ();
						
						return;
					}
					
					CheckAndSchedule (continuation, kind, scheduler);
				}
			};
			
			if (IsCompleted) {
				action (this, EventArgs.Empty);
				return;
			}
			
			completed += action;
			
			// Retry in case completion was achieved but event adding was too late
			if (IsCompleted)
				action (this, EventArgs.Empty);
		}
		
		bool ContinuationStatusCheck (TaskContinuationOptions kind)
		{
			if (kind == TaskContinuationOptions.None)
				return true;
			
			int kindCode = (int)kind;
			
			if (kindCode >= ((int)TaskContinuationOptions.NotOnRanToCompletion)) {
				if (status == TaskStatus.Canceled) {
					if (kind == TaskContinuationOptions.NotOnCanceled)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnFaulted)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnRanToCompletion)
						return false;
				} else if (status == TaskStatus.Faulted) {
					if (kind == TaskContinuationOptions.NotOnFaulted)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnCanceled)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnRanToCompletion)
						return false;
				} else if (status == TaskStatus.RanToCompletion) {
					if (kind == TaskContinuationOptions.NotOnRanToCompletion)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnFaulted)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnCanceled)
						return false;
				}
			}
			
			return true;
		}
		
		void CheckAndSchedule (Task continuation, TaskContinuationOptions options, TaskScheduler scheduler)
		{
			if (options == TaskContinuationOptions.None || (options & TaskContinuationOptions.ExecuteSynchronously) > 0) {
				continuation.ThreadStart ();
			} else {
				continuation.Start (scheduler);
			}
		}
		
		static TaskCreationOptions GetCreationOptions (TaskContinuationOptions kind)
		{
			TaskCreationOptions options = TaskCreationOptions.None;
			if ((kind & TaskContinuationOptions.DetachedFromParent) > 0)
				options |= TaskCreationOptions.DetachedFromParent;
			if ((kind & TaskContinuationOptions.RespectParentCancellation) > 0)
				options |= TaskCreationOptions.RespectParentCancellation;
			
			return options;
		}
		#endregion
		
		#region Internal and protected thingies
		internal void Schedule ()
		{	
			status = TaskStatus.WaitingToRun;
			
			// If worker is null it means it is a local one, revert to the old behavior
			if (current == null || childWorkAdder == null || parent == null
			    || CheckTaskOptions (taskCreationOptions, TaskCreationOptions.PreferFairness)) {
				
				scheduler.AddWork (this);
				
			} else {
				/* Like the semantic of the ABP paper describe it, we add ourselves to the bottom 
				 * of our Parent Task's ThreadWorker deque. It's ok to do that since we are in
				 * the correct Thread during the creation
				 */
				childWorkAdder (this);
			}
		}
		
		void ThreadStart ()
		{			
			current = this;
			TaskScheduler.Current = taskScheduler;
			
			if (!src.IsCancellationRequested
			    && (!respectParentCancellation || (respectParentCancellation && parent != null && !parent.IsCanceled))) {
				
				status = TaskStatus.Running;
				
				try {
					InnerInvoke ();
				} catch (Exception e) {
					exception = e;
					status = TaskStatus.Faulted;
				}
			} else {
				AcknowledgeCancellation ();
			}
			
			Finish ();
		}
		
		internal void Execute (Action<Task> childAdder)
		{
			childWorkAdder = childAdder;
			ThreadStart ();
		}
		
		internal void AddChild ()
		{
			childTasks.AddCount ();
		}

		internal void ChildCompleted ()
		{
			childTasks.Signal ();
			if (childTasks.IsSet && status == TaskStatus.WaitingForChildrenToComplete)
				status = TaskStatus.RanToCompletion;
		}

		internal virtual void InnerInvoke ()
		{
			if (action != null)
				action (state);
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in an anonymous method
			action = null;
			state = null;
		}
		
		internal void Finish ()
		{
			// If there wasn't any child created in the task we set the CountdownEvent
			childTasks.Signal ();
			
			// Don't override Canceled or Faulted
			if (status == TaskStatus.Running) {
				if (childTasks.IsSet )
					status = TaskStatus.RanToCompletion;
				else
					status = TaskStatus.WaitingForChildrenToComplete;
			}
			
			// Call the event in the correct style
			EventHandler tempCompleted = completed;
			if (tempCompleted != null) 
				tempCompleted (this, EventArgs.Empty);
			
			// Reset the current thingies
			current = null;
			TaskScheduler.Current = null;
			
			// Tell parent that we are finished
			if (!CheckTaskOptions (taskCreationOptions, TaskCreationOptions.DetachedFromParent) && parent != null){
				parent.ChildCompleted ();
			}
			
			Dispose ();
		}
		#endregion
		
		#region Cancel and Wait related methods
		public void AcknowledgeCancellation ()
		{
			if (this != current)
				throw new InvalidOperationException ("The Task object is different from the currently executing"
				                                     + "task or the current task hasn't been "
				                                     + "marked for cancellation.");
			
			CancelReal ();
		}
		
		internal void CancelReal ()
		{
			exception = new TaskCanceledException (this);
			status = TaskStatus.Canceled;
		}
		
		public void Cancel ()
		{
			src.Cancel ();
		}
		
		public void CancelAndWait ()
		{
			Cancel ();
			Wait ();
		}
		
		public void CancelAndWait (CancellationToken token)
		{
			Cancel ();
			Wait (token);
		}
		
		public bool CancelAndWait (TimeSpan ts)
		{
			Cancel ();
			return Wait (ts);
		}
		
		public bool CancelAndWait (int millisecondsTimeout)
		{
			Cancel ();
			return Wait (millisecondsTimeout);
		}
		
		public bool CancelAndWait (int millisecondsTimeout, CancellationToken token)
		{
			Cancel ();
			return Wait (millisecondsTimeout, token);
		}
		
		public void Wait ()
		{
			if (scheduler == null)
				throw new InvalidOperationException ("The Task hasn't been Started and thus can't be waited on");
			
			scheduler.ParticipateUntil (this);
			if (exception != null && !(exception is TaskCanceledException))
				throw exception;
		}

		public void Wait (CancellationToken token)
		{
			Wait (null, token);
		}
		
		public bool Wait (TimeSpan ts)
		{
			return Wait ((int)ts.TotalMilliseconds);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			Watch sw = Watch.StartNew ();
			return Wait (() => sw.ElapsedMilliseconds >= millisecondsTimeout, null);
		}
		
		public bool Wait (int millisecondsTimeout, CancellationToken token)
		{
			Watch sw = Watch.StartNew ();
			return Wait (() => sw.ElapsedMilliseconds >= millisecondsTimeout, token);
		}

		bool Wait (Func<bool> stopFunc, CancellationToken? token)
		{
			if (scheduler == null)
				throw new InvalidOperationException ("The Task hasn't been Started and thus can't be waited on");
			
			bool result = scheduler.ParticipateUntil (this, delegate { 
				if (token.HasValue && token.Value.IsCancellationRequested)
					throw new OperationCanceledException ("The CancellationToken has had cancellation requested.");
				
				
				return (stopFunc != null) ? stopFunc () : false;
			});

			if (exception != null && !(exception is TaskCanceledException))
				throw exception;
			
			return !result;
		}
		
		public static void WaitAll (params Task[] tasks)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			
			foreach (var t in tasks)
				t.Wait ();
		}

		public static void WaitAll (Task[] tasks, CancellationToken token)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			
			foreach (var t in tasks)
				t.Wait (token);
		}
		
		public static bool WaitAll (Task[] tasks, TimeSpan ts)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait (ts);
			return result;
		}
		
		public static bool WaitAll (Task[] tasks, int millisecondsTimeout)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait (millisecondsTimeout);
			return result;
		}
		
		public static bool WaitAll (Task[] tasks, int millisecondsTimeout, CancellationToken token)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait (millisecondsTimeout, token);
			return result;
		}
		
		public static int WaitAny (params Task[] tasks)
		{
			return WaitAny (tasks, null, null);
		}
		
		static int WaitAny (Task[] tasks, Func<bool> stopFunc, CancellationToken? token)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			
			int numFinished = 0;
			int indexFirstFinished = -1;
			int index = 0;
			
			foreach (Task t in tasks) {
				t.ContinueWith (delegate {
					int indexResult = index;
					int result = Interlocked.Increment (ref numFinished);
					// Check if we are the first to have finished
					if (result == 1)
						indexFirstFinished = indexResult;
				});	
				index++;
			}
			
			// One task already finished
			if (indexFirstFinished != -1)
				return indexFirstFinished;
			
			// All tasks are supposed to use the same TaskManager
			tasks[0].scheduler.ParticipateUntil (delegate {
				if (stopFunc != null && stopFunc ())
					return true;
				
				if (token.HasValue && token.Value.IsCancellationRequested)
					throw new OperationCanceledException ("The CancellationToken has had cancellation requested.");
				
				return numFinished >= 1;
			});
			
			return indexFirstFinished;
		}
		
		public static int WaitAny (Task[] tasks, TimeSpan ts)
		{
			return WaitAny (tasks, (int)ts.TotalMilliseconds);
		}
		
		public static int WaitAny (Task[] tasks, int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			
			if (millisecondsTimeout == -1)
				return WaitAny (tasks);
			
			Watch sw = Watch.StartNew ();
			return WaitAny (tasks, () => sw.ElapsedMilliseconds > millisecondsTimeout, null);
		}

		public static int WaitAny (Task[] tasks, int millisecondsTimeout, CancellationToken token)
		{			
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			
			if (millisecondsTimeout == -1)
				return WaitAny (tasks);
			
			Watch sw = Watch.StartNew ();
			return WaitAny (tasks, () => sw.ElapsedMilliseconds > millisecondsTimeout, token);
		}
		
		public static int WaitAny (Task[] tasks, CancellationToken token)
		{			
			return WaitAny (tasks, null, token);
		}
		#endregion
		
		#region Dispose
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposeManagedRes)
		{
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in a anonymous method
			if (disposeManagedRes) {
				action = null;
				completed = null;
				state = null;
			}
		}
		#endregion
		
		#region Properties
		public static TaskFactory Factory {
			get {
				return defaultFactory;
			}
		}
		
		public static Task Current {
			get {
				return current;
			}
		}
		
		public CancellationToken CancellationToken {
			get {
				return src.Token;
			}
		}
		
		public Exception Exception {
			get {
				exceptionObserved = true;
				
				return exception;	
			}
			internal set {
				exception = value;
			}
		}
		
		public bool IsCanceled {
			get {
				return status == TaskStatus.Canceled;
			}
		}

		public bool IsCancellationRequested {
			get {
				return src.IsCancellationRequested;
			}
		}

		public bool IsCompleted {
			get {
				return status == TaskStatus.RanToCompletion ||
					status == TaskStatus.Canceled || status == TaskStatus.Faulted;
			}
		}
		
		public bool IsFaulted {
			get {
				return status == TaskStatus.Faulted;
			}
		}

		public Task Parent {
			get {
				return parent;
			}
		}

		public TaskCreationOptions CreationOptions {
			get {
				return taskCreationOptions;
			}
		}
		
		public TaskStatus Status {
			get {
				return status;
			}
			internal set {
				status = value;
			}
		}

		public object AsyncState {
			get {
				return state;
			}
		}
		
		bool IAsyncResult.CompletedSynchronously {
			get {
				return true;
			}
		}

		WaitHandle IAsyncResult.AsyncWaitHandle {
			get {
				return null;
			}
		}
		
		public int Id {
			get {
				return taskId;
			}
		}
		#endregion
	}
}
#endif
