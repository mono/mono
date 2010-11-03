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

		ManualResetEventSlim schedWait = new ManualResetEventSlim (false);
		
		volatile AggregateException  exception;
		volatile bool                exceptionObserved;
		volatile TaskStatus          status;
		
		Action<object> action;
		object         state;

		ConcurrentQueue<EventHandler> completed = new ConcurrentQueue<EventHandler> ();

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
			Start (ProxifyScheduler (tscheduler));
		}
		
		void Start (IScheduler scheduler)
		{
			SetupScheduler (scheduler);
			Schedule ();
		}

		internal void SetupScheduler (TaskScheduler tscheduler)
		{
			SetupScheduler (ProxifyScheduler (tscheduler));
		}

		internal void SetupScheduler (IScheduler scheduler)
		{
			this.scheduler = scheduler;
			status = TaskStatus.WaitingForActivation;
			schedWait.Set ();
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
			// TODO
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
			continuation.schedWait.Set ();
			
			AtomicBoolean launched = new AtomicBoolean ();
			EventHandler action = delegate (object sender, EventArgs e) {
				if (!launched.Value && launched.TrySet ()) {
					if (!predicate ())
						return;

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
			
			completed.Enqueue (action);
			
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
				} catch (OperationCanceledException oce) {
					if (oce.CancellationToken == token)
						CancelReal ();
					else
						HandleGenericException (oce);
				} catch (Exception e) {
					HandleGenericException (e);
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
				
				ProcessCompleteDelegates ();
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
		
			if (status != TaskStatus.WaitingForChildrenToComplete)
				ProcessCompleteDelegates ();
			
			// Reset the current thingies
			current = null;
			TaskScheduler.Current = null;
			
			// Tell parent that we are finished
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null){
				parent.ChildCompleted ();
			}
			
			Dispose ();
		}

		void ProcessCompleteDelegates ()
		{
			EventHandler handler;
			while (completed.TryDequeue (out handler))
				handler (this, EventArgs.Empty);
		}
		#endregion
		
		#region Cancel and Wait related method
		
		internal void CancelReal ()
		{
			status = TaskStatus.Canceled;
		}

		internal void HandleGenericException (Exception e)
		{
			HandleGenericException (new AggregateException (e));
		}

		internal void HandleGenericException (AggregateException e)
		{
			exception = e;
			status = TaskStatus.Faulted;
			if (taskScheduler != null && taskScheduler.FireUnobservedEvent (exception).Observed)
				exceptionObserved = true;
		}
		
		public void Wait ()
		{
			if (scheduler == null)
				schedWait.Wait ();
			
			scheduler.ParticipateUntil (this);
			if (exception != null)
				throw exception;
			if (IsCanceled)
				throw new AggregateException (new TaskCanceledException (this));
		}

		public void Wait (CancellationToken token)
		{
			Wait (-1, token);
		}
		
		public bool Wait (TimeSpan ts)
		{
			return Wait (CheckTimeout (ts), CancellationToken.None);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			return Wait (millisecondsTimeout, CancellationToken.None);
		}

		public bool Wait (int millisecondsTimeout, CancellationToken token)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			if (millisecondsTimeout == -1 && token == CancellationToken.None) {
				Wait ();
				return true;
			}

			Watch watch = Watch.StartNew ();

			if (scheduler == null) {
				schedWait.Wait (millisecondsTimeout, token);
				millisecondsTimeout = ComputeTimeout (millisecondsTimeout, watch);
			}

			Func<bool> stopFunc
				= delegate { token.ThrowIfCancellationRequested (); return watch.ElapsedMilliseconds > millisecondsTimeout; };
			bool result = scheduler.ParticipateUntil (this, stopFunc);

			if (exception != null)
				throw exception;
			if (IsCanceled)
				throw new AggregateException (new TaskCanceledException (this));
			
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
			return WaitAny (tasks, -1, CancellationToken.None);
		}

		public static int WaitAny (Task[] tasks, TimeSpan ts)
		{
			return WaitAny (tasks, CheckTimeout (ts));
		}
		
		public static int WaitAny (Task[] tasks, int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			if (millisecondsTimeout == -1)
				return WaitAny (tasks);

			return WaitAny (tasks, millisecondsTimeout, CancellationToken.None);
		}

		public static int WaitAny (Task[] tasks, CancellationToken token)
		{
			return WaitAny (tasks, -1, token);
		}

		public static int WaitAny (Task[] tasks, int millisecondsTimeout, CancellationToken token)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			if (tasks.Length == 1) {
				tasks[0].Wait (millisecondsTimeout, token);
				return 0;
			}
			
			int numFinished = 0;
			int indexFirstFinished = -1;
			int index = 0;
			IScheduler sched = null;
			Watch watch = Watch.StartNew ();

			foreach (Task t in tasks) {
				int indexResult = index++;
				t.ContinueWith (delegate {
					if (numFinished >= 1)
						return;
					int result = Interlocked.Increment (ref numFinished);
					// Check if we are the first to have finished
					if (result == 1)
						indexFirstFinished = indexResult;
				}, TaskContinuationOptions.ExecuteSynchronously);
				if (sched == null && t.scheduler != null)
					sched = t.scheduler;
			}

			// If none of task have a scheduler we are forced to wait for at least one to start
			if (sched == null) {
				var handles = Array.ConvertAll (tasks, t => t.schedWait.WaitHandle);
				int shandle = -1;
				if ((shandle = WaitHandle.WaitAny (handles, millisecondsTimeout)) == WaitHandle.WaitTimeout)
					return -1;
				sched = tasks[shandle].scheduler;
				millisecondsTimeout = ComputeTimeout (millisecondsTimeout, watch);
			}
			
			// One task already finished
			if (indexFirstFinished != -1)
				return indexFirstFinished;
			
			// All tasks are supposed to use the same TaskScheduler
			sched.ParticipateUntil (delegate {
				if (millisecondsTimeout != 1 && watch.ElapsedMilliseconds > millisecondsTimeout)
					return true;

				token.ThrowIfCancellationRequested ();

				return numFinished >= 1;
			});
			
			return indexFirstFinished;
		}

		static int CheckTimeout (TimeSpan timeout)
		{
			try {
				return checked ((int)timeout.TotalMilliseconds);
			} catch (System.OverflowException) {
				throw new ArgumentOutOfRangeException ("timeout");
			}
		}

		static int ComputeTimeout (int millisecondsTimeout, Watch watch)
		{
			return millisecondsTimeout == -1 ? -1 : (int)Math.Max (watch.ElapsedMilliseconds - millisecondsTimeout, 1);
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
				completed.Clear ();
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
