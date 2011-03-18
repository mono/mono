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

#if NET_4_0 || MOBILE

using System;
using System.Threading;
using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
	[System.Diagnostics.DebuggerDisplay ("Id = {Id}, Status = {Status}, Method = {DebuggerDisplayMethodDescription}")]
	[System.Diagnostics.DebuggerTypeProxy ("System.Threading.Tasks.SystemThreadingTasks_TaskDebugView")]
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
		
		TaskScheduler       scheduler;

		ManualResetEventSlim schedWait = new ManualResetEventSlim (false);
		
		volatile AggregateException  exception;
		volatile bool                exceptionObserved;

		TaskStatus          status;
		
		Action<object> action;
		Action         simpleAction;
		object         state;
		AtomicBooleanValue executing;

		ConcurrentQueue<EventHandler> completed;

		CancellationToken token;

		public Task (Action action) : this (action, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, TaskCreationOptions creationOptions) : this (action, CancellationToken.None, creationOptions)
		{
			
		}
		
		public Task (Action action, CancellationToken cancellationToken) : this (action, cancellationToken, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
			: this (null, null, cancellationToken, creationOptions)
		{
			this.simpleAction = action;
		}
		
		public Task (Action<object> action, object state) : this (action, state, TaskCreationOptions.None)
		{	
		}
		
		public Task (Action<object> action, object state, TaskCreationOptions creationOptions)
			: this (action, state, CancellationToken.None, creationOptions)
		{
		}
		
		public Task (Action<object> action, object state, CancellationToken cancellationToken)
			: this (action, state, cancellationToken, TaskCreationOptions.None)
		{	
		}

		public Task (Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
			: this (action, state, cancellationToken, creationOptions, current)
		{

		}

		internal Task (Action<object> action,
		               object state,
		               CancellationToken cancellationToken,
		               TaskCreationOptions creationOptions,
		               Task parent)
		{
			this.taskCreationOptions = creationOptions;
			this.action              = action;
			this.state               = state;
			this.taskId              = Interlocked.Increment (ref id);
			this.status              = TaskStatus.Created;
			this.token               = cancellationToken;
			this.parent              = parent;

			// Process taskCreationOptions
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null)
				parent.AddChild ();
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

		#region Start
		public void Start ()
		{
			Start (TaskScheduler.Current);
		}
		
		public void Start (TaskScheduler scheduler)
		{
			SetupScheduler (scheduler);
			Schedule ();
		}

		internal void SetupScheduler (TaskScheduler scheduler)
		{
			this.scheduler = scheduler;
			status = TaskStatus.WaitingForActivation;
			schedWait.Set ();
		}
		
		public void RunSynchronously ()
		{
			RunSynchronously (TaskScheduler.Current);
		}
		
		public void RunSynchronously (TaskScheduler scheduler)
		{
			if (this.Status != TaskStatus.Created)
				throw new InvalidOperationException ("The task is not in a valid state to be started");
			if (scheduler.TryExecuteTask (this))
				return;

			Start (scheduler);
			Wait ();
		}
		#endregion
		
		#region ContinueWith
		public Task ContinueWith (Action<Task> continuationAction)
		{
			return ContinueWith (continuationAction, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith (continuationAction, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task> continuationAction, CancellationToken cancellationToken)
		{
			return ContinueWith (continuationAction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task> continuationAction, TaskScheduler scheduler)
		{
			return ContinueWith (continuationAction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}
		
		public Task ContinueWith (Action<Task> continuationAction, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			Task continuation = new Task ((o) => continuationAction ((Task)o),
			                              this,
			                              cancellationToken,
			                              GetCreationOptions (continuationOptions),
			                              this);
			ContinueWithCore (continuation, continuationOptions, scheduler);

			return continuation;
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> continuationFunction)
		{
			return ContinueWith<TResult> (continuationFunction, TaskContinuationOptions.None);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> continuationFunction, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith<TResult> (continuationFunction, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> continuationFunction, CancellationToken cancellationToken)
		{
			return ContinueWith<TResult> (continuationFunction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> continuationFunction, TaskScheduler scheduler)
		{
			return ContinueWith<TResult> (continuationFunction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}
		
		public Task<TResult> ContinueWith<TResult> (Func<Task, TResult> continuationFunction, CancellationToken cancellationToken,
		                                            TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			if (continuationFunction == null)
				throw new ArgumentNullException ("continuationFunction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			Task<TResult> t = new Task<TResult> ((o) => continuationFunction ((Task)o),
			                                     this,
			                                     cancellationToken,
			                                     GetCreationOptions (continuationOptions),
			                                     this);
			
			ContinueWithCore (t, continuationOptions, scheduler);
			
			return t;
		}
		
		internal void ContinueWithCore (Task continuation, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			ContinueWithCore (continuation, continuationOptions, scheduler, () => true);
		}
		
		internal void ContinueWithCore (Task continuation, TaskContinuationOptions kind,
		                                TaskScheduler scheduler, Func<bool> predicate)
		{
			// Already set the scheduler so that user can call Wait and that sort of stuff
			continuation.scheduler = scheduler;
			continuation.schedWait.Set ();
			continuation.status = TaskStatus.WaitingForActivation;
			
			AtomicBoolean launched = new AtomicBoolean ();
			EventHandler action = delegate (object sender, EventArgs e) {
				if (launched.TryRelaxedSet ()) {
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
			
			if (completed == null)
				Interlocked.CompareExchange (ref completed, new ConcurrentQueue<EventHandler> (), null);
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
					if ((kind & TaskContinuationOptions.NotOnCanceled) > 0)
						return false;
					if ((kind & TaskContinuationOptions.OnlyOnFaulted) > 0)
						return false;
					if ((kind & TaskContinuationOptions.OnlyOnRanToCompletion) & 0)
						return false;
				} else if (status == TaskStatus.Faulted) {
					if ((kind & TaskContinuationOptions.NotOnFaulted) > 0)
						return false;
					if ((kind & TaskContinuationOptions.OnlyOnCanceled) > 0)
						return false;
					if ((kind & TaskContinuationOptions.OnlyOnRanToCompletion) > 0)
						return false;
				} else if (status == TaskStatus.RanToCompletion) {
					if ((kind & TaskContinuationOptions.NotOnRanToCompletion) > 0)
						return false;
					if ((kind & TaskContinuationOptions.OnlyOnFaulted) > 0)
						return false;
					if ((kind & TaskContinuationOptions.OnlyOnCanceled) > 0)
						return false;
				}
			}
			
			return true;
		}
		
		void CheckAndSchedule (Task continuation, TaskContinuationOptions options, TaskScheduler scheduler, bool fromCaller)
		{
			if ((options & TaskContinuationOptions.ExecuteSynchronously) > 0)
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
			// If TaskScheduler.Current is not being used, the scheduler was explicitly provided, so we must use that
			if (scheduler != TaskScheduler.Current || childWorkAdder == null || CheckTaskOptions (taskCreationOptions, TaskCreationOptions.PreferFairness)) {
				scheduler.QueueTask (this);
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
			/* Allow scheduler to break fairness of deque ordering without
			 * breaking its semantic (the task can be executed twice but the
			 * second time it will return immediately
			 */
			if (!executing.TryRelaxedSet ())
				return;

			current = this;
			TaskScheduler.Current = scheduler;
			
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
			if (childTasks.Signal () && status == TaskStatus.WaitingForChildrenToComplete) {
				status = TaskStatus.RanToCompletion;
				
				ProcessCompleteDelegates ();
			}
		}

		internal virtual void InnerInvoke ()
		{
			if (action == null && simpleAction != null)
				simpleAction ();
			else if (action != null)
				action (state);
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in an anonymous method
			action = null;
			simpleAction = null;
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
		}

		void ProcessCompleteDelegates ()
		{
			if (completed == null)
				return;

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
			if (scheduler != null && scheduler.FireUnobservedEvent (exception).Observed)
				exceptionObserved = true;
		}
		
		public void Wait ()
		{
			if (scheduler == null)
				schedWait.Wait ();
			
			if (!IsCompleted)
				scheduler.ParticipateUntil (this);
			if (exception != null)
				throw exception;
			if (IsCanceled)
				throw new AggregateException (new TaskCanceledException (this));
		}

		public void Wait (CancellationToken cancellationToken)
		{
			Wait (-1, cancellationToken);
		}
		
		public bool Wait (TimeSpan timeout)
		{
			return Wait (CheckTimeout (timeout), CancellationToken.None);
		}
		
		public bool Wait (int millisecondsTimeout)
		{
			return Wait (millisecondsTimeout, CancellationToken.None);
		}

		public bool Wait (int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			if (millisecondsTimeout == -1 && token == CancellationToken.None) {
				Wait ();
				return true;
			}

			Watch watch = Watch.StartNew ();

			if (scheduler == null) {
				schedWait.Wait (millisecondsTimeout, cancellationToken);
				millisecondsTimeout = ComputeTimeout (millisecondsTimeout, watch);
			}

			ManualResetEventSlim predicateEvt = new ManualResetEventSlim (false);
			if (cancellationToken != CancellationToken.None) {
				cancellationToken.Register (predicateEvt.Set);
				cancellationToken.ThrowIfCancellationRequested ();
			}

			bool result = scheduler.ParticipateUntil (this, predicateEvt, millisecondsTimeout);

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
			
			foreach (var t in tasks) {
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");
				t.Wait ();
			}
		}

		public static void WaitAll (Task[] tasks, CancellationToken cancellationToken)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			
			foreach (var t in tasks) {
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");

				t.Wait (cancellationToken);
			}
		}
		
		public static bool WaitAll (Task[] tasks, TimeSpan timeout)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			
			bool result = true;
			foreach (var t in tasks) {
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");

				result &= t.Wait (timeout);
			}
			return result;
		}
		
		public static bool WaitAll (Task[] tasks, int millisecondsTimeout)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			
			bool result = true;
			foreach (var t in tasks) {
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");

				result &= t.Wait (millisecondsTimeout);
			}
			return result;
		}
		
		public static bool WaitAll (Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			
			bool result = true;
			foreach (var t in tasks) {
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");

				result &= t.Wait (millisecondsTimeout, cancellationToken);
			}
			return result;
		}
		
		public static int WaitAny (params Task[] tasks)
		{
			return WaitAny (tasks, -1, CancellationToken.None);
		}

		public static int WaitAny (Task[] tasks, TimeSpan timeout)
		{
			return WaitAny (tasks, CheckTimeout (timeout));
		}
		
		public static int WaitAny (Task[] tasks, int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			if (millisecondsTimeout == -1)
				return WaitAny (tasks);

			return WaitAny (tasks, millisecondsTimeout, CancellationToken.None);
		}

		public static int WaitAny (Task[] tasks, CancellationToken cancellationToken)
		{
			return WaitAny (tasks, -1, cancellationToken);
		}

		public static int WaitAny (Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException ("tasks is empty", "tasks");
			if (tasks.Length == 1) {
				tasks[0].Wait (millisecondsTimeout, cancellationToken);
				return 0;
			}
			
			int numFinished = 0;
			int indexFirstFinished = -1;
			int index = 0;
			TaskScheduler sched = null;
			Task task = null;
			Watch watch = Watch.StartNew ();
			ManualResetEventSlim predicateEvt = new ManualResetEventSlim (false);

			foreach (Task t in tasks) {
				int indexResult = index++;
				t.ContinueWith (delegate {
					if (numFinished >= 1)
						return;
					int result = Interlocked.Increment (ref numFinished);

					// Check if we are the first to have finished
					if (result == 1)
						indexFirstFinished = indexResult;

					// Stop waiting
					predicateEvt.Set ();
				}, TaskContinuationOptions.ExecuteSynchronously);

				if (sched == null && t.scheduler != null) {
					task = t;
					sched = t.scheduler;
				}
			}

			// If none of task have a scheduler we are forced to wait for at least one to start
			if (sched == null) {
				var handles = Array.ConvertAll (tasks, t => t.schedWait.WaitHandle);
				int shandle = -1;
				if ((shandle = WaitHandle.WaitAny (handles, millisecondsTimeout)) == WaitHandle.WaitTimeout)
					return -1;
				sched = tasks[shandle].scheduler;
				task = tasks[shandle];
				millisecondsTimeout = ComputeTimeout (millisecondsTimeout, watch);
			}

			// One task already finished
			if (indexFirstFinished != -1)
				return indexFirstFinished;

			if (cancellationToken != CancellationToken.None) {
				cancellationToken.Register (predicateEvt.Set);
				cancellationToken.ThrowIfCancellationRequested ();
			}

			sched.ParticipateUntil (task, predicateEvt, millisecondsTimeout);

			// Index update is still not done
			if (indexFirstFinished == -1) {
				SpinWait wait = new SpinWait ();
				while (indexFirstFinished == -1)
					wait.SpinOnce ();
			}

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
		
		protected virtual void Dispose (bool disposing)
		{
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in a anonymous method
			if (disposing) {
				action = null;
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

		internal Task Parent {
			get {
				return parent;
			}
		}
		#endregion
	}
}
#endif
