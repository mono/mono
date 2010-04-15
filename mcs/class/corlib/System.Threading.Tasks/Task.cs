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

#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;
using System.Threading;
using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
	public class Task : IDisposable, IAsyncResult
	{
		// With this attribute each thread has its own value so that it's correct for our Schedule code
		// and for Parent property.
		[System.ThreadStatic]
		static Task         current;
		[System.ThreadStatic]
		static Action<Task> childWorkAdder;
		
		Task parent;
		
		static int          id = -1;
		static TaskFactory  defaultFactory = new TaskFactory ();
		
		CountdownEvent childTasks = new CountdownEvent (1);
		
		int                 taskId;
		TaskCreationOptions taskCreationOptions;
		
		IScheduler          scheduler;
		TaskScheduler       taskScheduler;
		
		volatile AggregateException  exception;
		volatile bool                exceptionObserved;
		volatile TaskStatus          status;
		
		Action<object> action;
		object         state;
		EventHandler   completed;
		
		CancellationToken token;			
		
		public Task (Action action) : this (action, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, TaskCreationOptions options) : this (action, CancellationToken.None, options)
		{
			
		}
		
		public Task (Action action, CancellationToken token) : this (action, token, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, CancellationToken token, TaskCreationOptions options)
			: this ((o) => { if (action != null) action (); }, null, token, options)
		{
		}
		
		public Task (Action<object> action, object state) : this (action, state, TaskCreationOptions.None)
		{	
		}
		
		public Task (Action<object> action, object state, TaskCreationOptions options)
			: this (action, state, CancellationToken.None, options)
		{
		}
		
		public Task (Action<object> action, object state, CancellationToken token)
			: this (action, state, token, TaskCreationOptions.None)
		{	
		}
		
		public Task (Action<object> action, object state, CancellationToken token, TaskCreationOptions options)
		{
			this.taskCreationOptions = options;
			this.action              = action == null ? EmptyFunc : action;
			this.state               = state;
			this.taskId              = Interlocked.Increment (ref id);
			this.status              = TaskStatus.Created;
			this.token               = token;

			// Process taskCreationOptions
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent)) {
				parent = current;
				if (parent != null)
					parent.AddChild ();
			}
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
			return ContinueWith (a, CancellationToken.None, kind, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task> a, CancellationToken token)
		{
			return ContinueWith (a, token, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task> a, TaskScheduler scheduler)
		{
			return ContinueWith (a, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}
		
		public Task ContinueWith (Action<Task> a, CancellationToken token, TaskContinuationOptions kind, TaskScheduler scheduler)
		{
			Task continuation = new Task ((o) => a ((Task)o), this, token, GetCreationOptions (kind));
			ContinueWithCore (continuation, kind, scheduler);
			return continuation;
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a)
		{
			return ContinueWith<TResult> (a, TaskContinuationOptions.None);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, TaskContinuationOptions options)
		{
			return ContinueWith<TResult> (a, CancellationToken.None, options, TaskScheduler.Current);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, CancellationToken token)
		{
			return ContinueWith<TResult> (a, token, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, TaskScheduler scheduler)
		{
			return ContinueWith<TResult> (a, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> a, CancellationToken token,
		                                            TaskContinuationOptions kind, TaskScheduler scheduler)
		{
			Task<TResult> t = new Task<TResult> ((o) => a ((Task)o), this, token, GetCreationOptions (kind));
			
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
			EventHandler action = delegate (object sender, EventArgs e) {
				if (!predicate ()) return;
				
				if (!launched.Value && launched.TrySet ()) {
					if (!ContinuationStatusCheck (kind)) {
						continuation.CancelReal ();
						continuation.Dispose ();
						
						return;
					}
					
					CheckAndSchedule (continuation, kind, scheduler, sender == null);
				}
			};
			
			if (IsCompleted) {
				action (null, EventArgs.Empty);
				return;
			}
			
			completed += action;
			
			// Retry in case completion was achieved but event adding was too late
			if (IsCompleted)
				action (null, EventArgs.Empty);
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
		
		void CheckAndSchedule (Task continuation, TaskContinuationOptions options, TaskScheduler scheduler, bool fromCaller)
		{
			if (!fromCaller 
			    && (options == TaskContinuationOptions.None || (options & TaskContinuationOptions.ExecuteSynchronously) > 0))
				continuation.ThreadStart ();
			else
				continuation.Start (scheduler);
		}
		
		internal TaskCreationOptions GetCreationOptions (TaskContinuationOptions kind)
		{
			TaskCreationOptions options = TaskCreationOptions.None;
			if ((kind & TaskContinuationOptions.AttachedToParent) > 0)
				options |= TaskCreationOptions.AttachedToParent;
			if ((kind & TaskContinuationOptions.PreferFairness) > 0)
				options |= TaskCreationOptions.PreferFairness;
			if ((kind & TaskContinuationOptions.LongRunning) > 0)
				options |= TaskCreationOptions.LongRunning;
			
			return options;
		}
		#endregion
		
		#region Internal and protected thingies
		internal void Schedule ()
		{	
			status = TaskStatus.WaitingToRun;
			
			// If worker is null it means it is a local one, revert to the old behavior
			if (childWorkAdder == null || CheckTaskOptions (taskCreationOptions, TaskCreationOptions.PreferFairness)) {
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
			
			if (!token.IsCancellationRequested) {
				
				status = TaskStatus.Running;
				
				try {
					InnerInvoke ();
				} catch (Exception e) {
					exception = new AggregateException (e);
					status = TaskStatus.Faulted;
					if (taskScheduler.FireUnobservedEvent (exception).Observed)
						exceptionObserved = true;
				}
			} else {
				CancelReal ();
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
			if (childTasks.IsSet && status == TaskStatus.WaitingForChildrenToComplete) {
				status = TaskStatus.RanToCompletion;
				
				// Let continuation creation process
				EventHandler tempCompleted = completed;
				if (tempCompleted != null) 
					tempCompleted (this, EventArgs.Empty);
			}
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
				if (childTasks.IsSet)
					status = TaskStatus.RanToCompletion;
				else
					status = TaskStatus.WaitingForChildrenToComplete;
			}
		
			if (status != TaskStatus.WaitingForChildrenToComplete) {
				// Let continuation creation process
				EventHandler tempCompleted = completed;
				if (tempCompleted != null)
					tempCompleted (this, EventArgs.Empty);
			}
			
			// Reset the current thingies
			current = null;
			TaskScheduler.Current = null;
			
			// Tell parent that we are finished
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null){
				parent.ChildCompleted ();
			}
			
			Dispose ();
		}
		#endregion
		
		#region Cancel and Wait related method
		
		internal void CancelReal ()
		{
			exception = new AggregateException (new TaskCanceledException (this));
			status = TaskStatus.Canceled;
		}
		
		public void Wait ()
		{
			if (scheduler == null)
				throw new InvalidOperationException ("The Task hasn't been Started and thus can't be waited on");
			
			scheduler.ParticipateUntil (this);
			if (exception != null)
				throw exception;
		}

		public void Wait (CancellationToken token)
		{
			Wait (null, token);
		}
		
		public bool Wait (TimeSpan ts)
		{
			return Wait ((int)ts.TotalMilliseconds, CancellationToken.None);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			return Wait (millisecondsTimeout, CancellationToken.None);
		}
		
		public bool Wait (int millisecondsTimeout, CancellationToken token)
		{
			Watch sw = Watch.StartNew ();
			return Wait (() => sw.ElapsedMilliseconds >= millisecondsTimeout, token);
		}

		bool Wait (Func<bool> stopFunc, CancellationToken token)
		{
			if (scheduler == null)
				throw new InvalidOperationException ("The Task hasn't been Started and thus can't be waited on");
			
			bool result = scheduler.ParticipateUntil (this, delegate { 
				if (token.IsCancellationRequested)
					throw new OperationCanceledException ("The CancellationToken has had cancellation requested.");
				
				return (stopFunc != null) ? stopFunc () : false;
			});

			if (exception != null)
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
		
		public static int? CurrentId {
			get {
				Task t = current;
				return t == null ? (int?)null : t.Id;
			}
		}
		
		public AggregateException Exception {
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
