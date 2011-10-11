//
// Task.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
	[System.Diagnostics.DebuggerDisplay ("Id = {Id}, Status = {Status}")]
	[System.Diagnostics.DebuggerTypeProxy (typeof (TaskDebuggerView))]
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
		static readonly TaskFactory defaultFactory = new TaskFactory ();
		
		CountdownEvent childTasks;
		
		int                 taskId;
		TaskCreationOptions taskCreationOptions;
		
		TaskScheduler       scheduler;

		ManualResetEventSlim schedWait = new ManualResetEventSlim (false);
		
		volatile AggregateException  exception;
		volatile bool                exceptionObserved;
		ConcurrentQueue<AggregateException> childExceptions;

		TaskStatus          status;
		
		Action<object> action;
		Action         simpleAction;
		object         state;
		AtomicBooleanValue executing;

		CompletionContainer completed;
		// If this task is a continuation, this stuff gets filled
		CompletionSlot Slot;

		CancellationToken token;
		CancellationTokenRegistration? cancelation_registration;

		const TaskCreationOptions MaxTaskCreationOptions =
#if NET_4_5
			TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler |
#endif
			TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent;

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
			: this (null, null, cancellationToken, creationOptions, current)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (creationOptions > MaxTaskCreationOptions || creationOptions < TaskCreationOptions.None)
				throw new ArgumentOutOfRangeException ("creationOptions");
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
			if (action == null)
				throw new ArgumentNullException ("action");
			if (creationOptions > MaxTaskCreationOptions || creationOptions < TaskCreationOptions.None)
				throw new ArgumentOutOfRangeException ("creationOptions");
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
			this.status              = cancellationToken.IsCancellationRequested ? TaskStatus.Canceled : TaskStatus.Created;
			this.token               = cancellationToken;
			this.parent              = parent;

			// Process taskCreationOptions
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null)
				parent.AddChild ();

			if (token.CanBeCanceled) {
				cancelation_registration = token.Register (l => ((Task) l).CancelReal (), this);
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

		#region Start
		public void Start ()
		{
			Start (TaskScheduler.Current);
		}
		
		public void Start (TaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			if (status >= TaskStatus.WaitingToRun)
				throw new InvalidOperationException ("The Task is not in a valid state to be started.");

			if (Slot.Initialized)
				throw new InvalidOperationException ("Start may not be called on a continuation task");

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
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			if (Status > TaskStatus.WaitingForActivation)
				throw new InvalidOperationException ("The task is not in a valid state to be started");

			SetupScheduler (scheduler);
			var saveStatus = status;
			status = TaskStatus.WaitingToRun;

			try {
				if (scheduler.RunInline (this))
					return;
			} catch (Exception inner) {
				throw new TaskSchedulerException (inner);
			}

			status = saveStatus;
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
			if (continuationAction == null)
				throw new ArgumentNullException ("continuationAction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			Task continuation = new Task (l => continuationAction ((Task)l),
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
			ContinueWithCore (continuation, continuationOptions, scheduler, null);
		}
		
		internal void ContinueWithCore (Task continuation, TaskContinuationOptions kind,
		                                TaskScheduler scheduler, Func<bool> predicate)
		{
			// Already set the scheduler so that user can call Wait and that sort of stuff
			continuation.scheduler = scheduler;
			continuation.schedWait.Set ();
			continuation.status = TaskStatus.WaitingForActivation;
			continuation.Slot = new CompletionSlot (kind, predicate);

			if (IsCompleted) {
				CompletionExecutor (continuation);
				return;
			}
			
			completed.Add (continuation);
			
			// Retry in case completion was achieved but event adding was too late
			if (IsCompleted)
				CompletionExecutor (continuation);
		}

		
		bool ContinuationStatusCheck (TaskContinuationOptions kind)
		{
			if (kind == TaskContinuationOptions.None)
				return true;
			
			int kindCode = (int)kind;
			
			if (kindCode >= ((int)TaskContinuationOptions.NotOnRanToCompletion)) {
				// Remove other options
				kind &= ~(TaskContinuationOptions.PreferFairness
				          | TaskContinuationOptions.LongRunning
				          | TaskContinuationOptions.AttachedToParent
				          | TaskContinuationOptions.ExecuteSynchronously);

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
			
		static internal TaskCreationOptions GetCreationOptions (TaskContinuationOptions kind)
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
					if (token != CancellationToken.None && oce.CancellationToken == token)
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
			if (childTasks == null)
				Interlocked.CompareExchange (ref childTasks, new CountdownEvent (1), null);
			childTasks.AddCount ();
		}

		internal void ChildCompleted (AggregateException childEx)
		{
			if (childEx != null) {
				if (childExceptions == null)
					Interlocked.CompareExchange (ref childExceptions, new ConcurrentQueue<AggregateException> (), null);
				childExceptions.Enqueue (childEx);
			}

			if (childTasks.Signal () && status == TaskStatus.WaitingForChildrenToComplete) {
				status = TaskStatus.RanToCompletion;
				ProcessChildExceptions ();
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
			// If there was children created and they all finished, we set the countdown
			if (childTasks != null)
				childTasks.Signal ();
			
			// Don't override Canceled or Faulted
			if (status == TaskStatus.Running) {
				if (childTasks == null || childTasks.IsSet)
					status = TaskStatus.RanToCompletion;
				else
					status = TaskStatus.WaitingForChildrenToComplete;
			}
		
			if (status != TaskStatus.WaitingForChildrenToComplete)
				ProcessCompleteDelegates ();

			// Reset the current thingies
			current = null;
			TaskScheduler.Current = null;

			if (cancelation_registration.HasValue)
				cancelation_registration.Value.Dispose ();
			
			// Tell parent that we are finished
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null) {
				parent.ChildCompleted (this.Exception);
			}
		}

		void CompletionExecutor (Task cont)
		{
			if (cont.Slot.Predicate != null && !cont.Slot.Predicate ())
				return;

			if (!cont.Slot.Launched.TryRelaxedSet ())
				return;

			if (!ContinuationStatusCheck (cont.Slot.Kind)) {
				cont.CancelReal ();
				cont.Dispose ();

				return;
			}
			
			if ((cont.Slot.Kind & TaskContinuationOptions.ExecuteSynchronously) != 0)
				cont.RunSynchronously (cont.scheduler);
			else
				cont.Schedule ();
		}

		void ProcessCompleteDelegates ()
		{
			if (!completed.HasElements)
				return;

			Task continuation;
			while (completed.TryGetNextCompletion (out continuation))
				CompletionExecutor (continuation);
		}

		void ProcessChildExceptions ()
		{
			if (childExceptions == null)
				return;

			if (exception == null)
				exception = new AggregateException ();

			AggregateException childEx;
			while (childExceptions.TryDequeue (out childEx))
				exception.AddChildException (childEx);
		}
		#endregion
		
		#region Cancel and Wait related method
		
		internal void CancelReal ()
		{
			status = TaskStatus.Canceled;
			ProcessCompleteDelegates ();
		}

		internal void HandleGenericException (Exception e)
		{
			HandleGenericException (new AggregateException (e));
		}

		internal void HandleGenericException (AggregateException e)
		{
			exception = e;
			Thread.MemoryBarrier ();
			status = TaskStatus.Faulted;
			if (scheduler != null && scheduler.FireUnobservedEvent (exception).Observed)
				exceptionObserved = true;
		}
		
		public void Wait ()
		{
			Wait (Timeout.Infinite, CancellationToken.None);
		}

		public void Wait (CancellationToken cancellationToken)
		{
			Wait (Timeout.Infinite, cancellationToken);
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

			bool result = IsCompleted;
			if (!result) {
				if (scheduler == null) {
					Watch watch = Watch.StartNew ();

					schedWait.Wait (millisecondsTimeout, cancellationToken);
					millisecondsTimeout = ComputeTimeout (millisecondsTimeout, watch);
				}

				var wait_event = new ManualResetEventSlim (false);
				CancellationTokenRegistration? registration = null;

				try {
					if (cancellationToken.CanBeCanceled) {
						registration = cancellationToken.Register (wait_event.Set);
					}

					// FIXME: The implementation is wrong and slow
					// It adds a continuation to the task which is then
					// returned to parent causing all sort of problems when
					// timeout is reached before task is finished
					result = !scheduler.ParticipateUntil (this, wait_event, millisecondsTimeout);
				} finally {
					if (registration.HasValue)
						registration.Value.Dispose ();
				}
			}

			if (IsCanceled)
				exception = new AggregateException (new TaskCanceledException (this));

			if (exception != null)
				throw exception;

			return result;
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
			if (!IsCompleted)
				throw new InvalidOperationException ("A task may only be disposed if it is in a completion state");

			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in a anonymous method
			if (disposing) {
				action = null;
				state = null;
			}
		}
		#endregion
		
#if NET_4_5

		public ConfiguredTaskAwaitable ConfigureAwait (bool continueOnCapturedContext)
		{
			return new ConfiguredTaskAwaitable (this, continueOnCapturedContext);
		}

		public Task ContinueWith (Action<Task, object> continuationAction, object state)
		{
			return ContinueWith (continuationAction, state, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task ContinueWith (Action<Task, object> continuationAction, object state, CancellationToken cancellationToken)
		{
			return ContinueWith (continuationAction, state, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task ContinueWith (Action<Task, object> continuationAction, object state, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith (continuationAction, state, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}

		public Task ContinueWith (Action<Task, object> continuationAction, object state, TaskScheduler scheduler)
		{
			return ContinueWith (continuationAction, state, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}

		public Task ContinueWith (Action<Task, object> continuationAction, object state, CancellationToken cancellationToken,
								  TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			if (continuationAction == null)
				throw new ArgumentNullException ("continuationAction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			Task continuation = new Task (l => continuationAction (this, l), state,
										  cancellationToken,
										  GetCreationOptions (continuationOptions),
										  this);
			ContinueWithCore (continuation, continuationOptions, scheduler);

			return continuation;
		}

		public Task<TResult> ContinueWith<TResult> (Func<Task, object, TResult> continuationFunction, object state)
		{
			return ContinueWith<TResult> (continuationFunction, state, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task<TResult> ContinueWith<TResult> (Func<Task, object, TResult> continuationFunction, object state, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith<TResult> (continuationFunction, state, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}

		public Task<TResult> ContinueWith<TResult> (Func<Task, object, TResult> continuationFunction, object state, CancellationToken cancellationToken)
		{
			return ContinueWith<TResult> (continuationFunction, state, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task<TResult> ContinueWith<TResult> (Func<Task, object, TResult> continuationFunction, object state, TaskScheduler scheduler)
		{
			return ContinueWith<TResult> (continuationFunction, state, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}

		public Task<TResult> ContinueWith<TResult> (Func<Task, object, TResult> continuationFunction, object state, CancellationToken cancellationToken,
													TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			if (continuationFunction == null)
				throw new ArgumentNullException ("continuationFunction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			var t = new Task<TResult> (l => continuationFunction (this, l),
												 state,
												 cancellationToken,
												 GetCreationOptions (continuationOptions),
												 this);

			ContinueWithCore (t, continuationOptions, scheduler);

			return t;
		}

		public static Task<TResult> FromResult<TResult> (TResult result)
		{
			var tcs = new TaskCompletionSource<TResult> ();
			tcs.SetResult (result);
			return tcs.Task;
		}

		public TaskAwaiter GetAwaiter ()
		{
			return new TaskAwaiter (this);
		}

		public static Task Run (Action action)
		{
			return Run (action, CancellationToken.None);
		}

		public static Task Run (Action action, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return TaskConstants.Canceled;

			var t = new Task (action, cancellationToken, TaskCreationOptions.DenyChildAttach);
			t.Start ();
			return t;
		}

		public static Task Run (Func<Task> function)
		{
			return Run (function, CancellationToken.None);
		}

		public static Task Run (Func<Task> function, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return TaskConstants.Canceled;

			var t = new Task<Task> (function, cancellationToken);
			t.Start ();
			return t;
		}

		public static Task<TResult> Run<TResult> (Func<TResult> function)
		{
			return Run (function, CancellationToken.None);
		}

		public static Task<TResult> Run<TResult> (Func<TResult> function, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return TaskConstants<TResult>.Canceled;

			var t = new Task<TResult> (function, cancellationToken, TaskCreationOptions.DenyChildAttach);
			t.Start ();
			return t;
		}

		public static Task<TResult> Run<TResult> (Func<Task<TResult>> function)
		{
			return Run (function, CancellationToken.None);
		}

		public static Task<TResult> Run<TResult> (Func<Task<TResult>> function, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return TaskConstants<TResult>.Canceled;

			var t = Task<Task<TResult>>.Factory.StartNew (function, cancellationToken, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
			return GetTaskResult (t);
		}

		async static Task<TResult> GetTaskResult<TResult> (Task<Task<TResult>> task)
		{
			var r = await task.ConfigureAwait (false);
			return r.Result;
		}
		
		public static YieldAwaitable Yield ()
		{
			return new YieldAwaitable ();
		}
#endif

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
		
		internal string DisplayActionMethod {
			get {
				Delegate d = simpleAction ?? (Delegate) action;
				return d == null ? "<none>" : d.Method.ToString ();
			}
		}
		
		#endregion
	}
}
#endif
