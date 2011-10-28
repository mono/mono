//
// Task.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
	[System.Diagnostics.DebuggerDisplay ("Id = {Id}, Status = {Status}")]
	[System.Diagnostics.DebuggerTypeProxy (typeof (TaskDebuggerView))]
	public class Task : IDisposable, IAsyncResult
	{
		const TaskCreationOptions TaskCreationOptionsContinuation = (TaskCreationOptions) (1 << 10);

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
		
		internal TaskScheduler       scheduler;

		TaskExceptionSlot exSlot;

		TaskStatus          status;

		TaskActionInvoker invoker;
		object         state;
		internal AtomicBooleanValue executing;

		TaskCompletionQueue<IContinuation> continuations;

		CancellationToken token;
		CancellationTokenRegistration? cancellationRegistration;

		internal const TaskCreationOptions WorkerTaskNotSupportedOptions = TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;

		const TaskCreationOptions MaxTaskCreationOptions =
#if NET_4_5
			TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler |
#endif
			TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent;

		public Task (Action action)
			: this (action, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, TaskCreationOptions creationOptions)
			: this (action, CancellationToken.None, creationOptions)
		{
			
		}
		
		public Task (Action action, CancellationToken cancellationToken)
			: this (action, cancellationToken, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
			: this (TaskActionInvoker.Create (action), null, cancellationToken, creationOptions, current)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (creationOptions > MaxTaskCreationOptions || creationOptions < TaskCreationOptions.None)
				throw new ArgumentOutOfRangeException ("creationOptions");
		}
		
		public Task (Action<object> action, object state)
			: this (action, state, TaskCreationOptions.None)
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
			: this (TaskActionInvoker.Create (action), state, cancellationToken, creationOptions, current)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (creationOptions > MaxTaskCreationOptions || creationOptions < TaskCreationOptions.None)
				throw new ArgumentOutOfRangeException ("creationOptions");
		}

		internal Task (TaskActionInvoker invoker, object state, CancellationToken cancellationToken,
		               TaskCreationOptions creationOptions, Task parent)
		{
			this.invoker = invoker;
			this.taskCreationOptions = creationOptions;
			this.state               = state;
			this.taskId              = Interlocked.Increment (ref id);
			this.status              = cancellationToken.IsCancellationRequested ? TaskStatus.Canceled : TaskStatus.Created;
			this.token               = cancellationToken;
			this.parent              = parent;

			// Process taskCreationOptions
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null)
				parent.AddChild ();

			if (token.CanBeCanceled)
				cancellationRegistration = token.Register (l => ((Task) l).CancelReal (), this);
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

			if (IsContinuation)
				throw new InvalidOperationException ("Start may not be called on a continuation task");

			SetupScheduler (scheduler);
			Schedule ();
		}

		internal void SetupScheduler (TaskScheduler scheduler)
		{
			this.scheduler = scheduler;
			Status = TaskStatus.WaitingForActivation;
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
			Status = TaskStatus.WaitingToRun;

			try {
				if (scheduler.RunInline (this))
					return;
			} catch (Exception inner) {
				throw new TaskSchedulerException (inner);
			}

			Status = saveStatus;
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

			return ContinueWith (TaskActionInvoker.Create (continuationAction), cancellationToken, continuationOptions, scheduler);
		}

		internal Task ContinueWith (TaskActionInvoker invoker, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			var continuation = new Task (invoker, null, cancellationToken, GetCreationOptions (continuationOptions), this);
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

			return ContinueWith<TResult> (TaskActionInvoker.Create (continuationFunction), cancellationToken, continuationOptions, scheduler);
		}

		internal Task<TResult> ContinueWith<TResult> (TaskActionInvoker invoker, CancellationToken cancellationToken, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			var continuation = new Task<TResult> (invoker, null, cancellationToken, GetCreationOptions (continuationOptions), this);
			ContinueWithCore (continuation, continuationOptions, scheduler);

			return continuation;
		}
	
		internal void ContinueWithCore (Task continuation, TaskContinuationOptions options, TaskScheduler scheduler)
		{
			const TaskContinuationOptions wrongRan = TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.OnlyOnRanToCompletion;
			const TaskContinuationOptions wrongCanceled = TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.OnlyOnCanceled;
			const TaskContinuationOptions wrongFaulted = TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.OnlyOnFaulted;

			if (((options & wrongRan) == wrongRan) || ((options & wrongCanceled) == wrongCanceled) || ((options & wrongFaulted) == wrongFaulted))
				throw new ArgumentException ("continuationOptions", "Some options are mutually exclusive");

			// Already set the scheduler so that user can call Wait and that sort of stuff
			continuation.scheduler = scheduler;
			continuation.Status = TaskStatus.WaitingForActivation;
			continuation.taskCreationOptions |= TaskCreationOptionsContinuation;

			ContinueWith (new TaskContinuation (continuation, options));
		}
		
		internal void ContinueWith (IContinuation continuation)
		{
			if (IsCompleted) {
				continuation.Execute ();
				return;
			}
			
			continuations.Add (continuation);
			
			// Retry in case completion was achieved but event adding was too late
			if (IsCompleted && continuations.Remove (continuation))
				continuation.Execute ();
		}

		void RemoveContinuation (IContinuation continuation)
		{
			continuations.Remove (continuation);
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
			Status = TaskStatus.WaitingToRun;
			
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

			// Disable CancellationToken direct cancellation
			if (cancellationRegistration != null) {
				cancellationRegistration.Value.Dispose ();
				cancellationRegistration = null;
			}
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

		internal bool TrySetCanceled ()
		{
			if (IsCompleted)
				return false;
			
			if (!executing.TryRelaxedSet ()) {
				var sw = new SpinWait ();
				while (!IsCompleted)
					sw.SpinOnce ();

				return false;
			}
			
			CancelReal ();
			return true;
		}

		internal bool TrySetException (AggregateException aggregate)
		{
			if (IsCompleted)
				return false;
			
			if (!executing.TryRelaxedSet ()) {
				var sw = new SpinWait ();
				while (!IsCompleted)
					sw.SpinOnce ();

				return false;
			}
			
			HandleGenericException (aggregate);
			return true;
		}
/*
		internal void Execute (Action<Task> childAdder)
		{
			childWorkAdder = childAdder;
			Execute ();
		}
*/		
		internal void Execute ()
		{
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
				if (ExceptionSlot.ChildExceptions == null)
					Interlocked.CompareExchange (ref ExceptionSlot.ChildExceptions, new ConcurrentQueue<AggregateException> (), null);
				ExceptionSlot.ChildExceptions.Enqueue (childEx);
			}

			if (childTasks.Signal () && status == TaskStatus.WaitingForChildrenToComplete) {
				Status = TaskStatus.RanToCompletion;
				ProcessChildExceptions ();
				ProcessCompleteDelegates ();
			}
		}

		void InnerInvoke ()
		{
			if (IsContinuation) {
				invoker.Invoke (parent, state, this);
			} else {
				invoker.Invoke (this, state, this);
			}
		}
		
		internal void Finish ()
		{
			// If there was children created and they all finished, we set the countdown
			if (childTasks != null)
				childTasks.Signal ();
			
			// Don't override Canceled or Faulted
			if (status == TaskStatus.Running) {
				if (childTasks == null || childTasks.IsSet)
					Status = TaskStatus.RanToCompletion;
				else
					Status = TaskStatus.WaitingForChildrenToComplete;
			}
		
			// Completions are already processed when task is canceled or faulted
			if (status == TaskStatus.RanToCompletion)
				ProcessCompleteDelegates ();

			// Reset the current thingies
			current = null;
			TaskScheduler.Current = null;

			if (cancellationRegistration.HasValue)
				cancellationRegistration.Value.Dispose ();
			
			// Tell parent that we are finished
			if (CheckTaskOptions (taskCreationOptions, TaskCreationOptions.AttachedToParent) && parent != null) {
				parent.ChildCompleted (this.Exception);
			}
		}

		void ProcessCompleteDelegates ()
		{
			if (continuations.HasElements) {
				IContinuation continuation;
				while (continuations.TryGetNextCompletion (out continuation))
					continuation.Execute ();
			}
		}

		void ProcessChildExceptions ()
		{
			if (exSlot == null || exSlot.ChildExceptions == null)
				return;

			if (ExceptionSlot.Exception == null)
				exSlot.Exception = new AggregateException ();

			AggregateException childEx;
			while (exSlot.ChildExceptions.TryDequeue (out childEx))
				exSlot.Exception.AddChildException (childEx);
		}
		#endregion
		
		#region Cancel and Wait related method
		
		internal void CancelReal ()
		{
			Status = TaskStatus.Canceled;
			ProcessCompleteDelegates ();
		}

		void HandleGenericException (Exception e)
		{
			HandleGenericException (new AggregateException (e));
		}

		void HandleGenericException (AggregateException e)
		{
			ExceptionSlot.Exception = e;
			Thread.MemoryBarrier ();
			Status = TaskStatus.Faulted;
			ProcessCompleteDelegates ();
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

			bool result = true;

			if (!IsCompleted) {
				// If the task is ready to be run and we were supposed to wait on it indefinitely, just run it
				if (Status == TaskStatus.WaitingToRun && millisecondsTimeout == -1 && scheduler != null)
					Execute ();

				if (!IsCompleted) {
					var evt = new ManualResetEventSlim ();
					var slot = new ManualEventSlot (evt);
					try {
						ContinueWith (slot);
						result = evt.Wait (millisecondsTimeout, cancellationToken);
					} finally {
						if (!result)
							RemoveContinuation (slot);
						evt.Dispose ();
					}
				}
			}

			if (IsCanceled)
				throw new AggregateException (new TaskCanceledException (this));

			var exception = Exception;
			if (exception != null)
				throw exception;

			return result;
		}
		
		public static void WaitAll (params Task[] tasks)
		{
			WaitAll (tasks, Timeout.Infinite, CancellationToken.None);
		}

		public static void WaitAll (Task[] tasks, CancellationToken cancellationToken)
		{
			WaitAll (tasks, Timeout.Infinite, cancellationToken);
		}
		
		public static bool WaitAll (Task[] tasks, TimeSpan timeout)
		{
			return WaitAll (tasks, CheckTimeout (timeout), CancellationToken.None);
		}
		
		public static bool WaitAll (Task[] tasks, int millisecondsTimeout)
		{
			return WaitAll (tasks, millisecondsTimeout, CancellationToken.None);
		}
		
		public static bool WaitAll (Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");

			bool result = true;
			foreach (var t in tasks) {
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");

				result &= t.Status == TaskStatus.RanToCompletion;
			}

			if (!result) {
				var evt = new CountdownEvent (tasks.Length);
				var slot = new CountdownEventSlot (evt);
				try {
					foreach (var t in tasks)
						t.ContinueWith (slot);

					result = evt.Wait (millisecondsTimeout, cancellationToken);
				} finally {
					List<Exception> exceptions = null;

					foreach (var t in tasks) {
						if (result) {
							if (t.Status == TaskStatus.RanToCompletion)
								continue;
							if (exceptions == null)
								exceptions = new List<Exception> ();
							if (t.Exception != null)
								exceptions.AddRange (t.Exception.InnerExceptions);
							else
								exceptions.Add (new TaskCanceledException (t));
						} else {
							t.RemoveContinuation (slot);
						}
					}

					evt.Dispose ();

					if (exceptions != null)
						throw new AggregateException (exceptions);
				}
			}

			return result;
		}
		
		public static int WaitAny (params Task[] tasks)
		{
			return WaitAny (tasks, Timeout.Infinite, CancellationToken.None);
		}

		public static int WaitAny (Task[] tasks, TimeSpan timeout)
		{
			return WaitAny (tasks, CheckTimeout (timeout));
		}
		
		public static int WaitAny (Task[] tasks, int millisecondsTimeout)
		{
			return WaitAny (tasks, millisecondsTimeout, CancellationToken.None);
		}

		public static int WaitAny (Task[] tasks, CancellationToken cancellationToken)
		{
			return WaitAny (tasks, Timeout.Infinite, cancellationToken);
		}

		public static int WaitAny (Task[] tasks, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (tasks == null)
				throw new ArgumentNullException ("tasks");
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			CheckForNullTasks (tasks);

			if (tasks.Length > 0) {
				var evt = new ManualResetEventSlim ();
				var slot = new ManualEventSlot (evt);
				bool result = false;
				try {
					for (int i = 0; i < tasks.Length; i++) {
						var t = tasks[i];
						if (t.IsCompleted)
							return i;
						t.ContinueWith (slot);
					}

					if (!(result = evt.Wait (millisecondsTimeout, cancellationToken)))
						return -1;
				} finally {
					if (!result)
						foreach (var t in tasks)
							t.RemoveContinuation (slot);
					evt.Dispose ();
				}
			}

			int firstFinished = -1;
			for (int i = 0; i < tasks.Length; i++) {
				var t = tasks[i];
				if (t.IsCompleted) {
					firstFinished = i;
					break;
				}
			}

			return firstFinished;
		}

		static int CheckTimeout (TimeSpan timeout)
		{
			try {
				return checked ((int)timeout.TotalMilliseconds);
			} catch (System.OverflowException) {
				throw new ArgumentOutOfRangeException ("timeout");
			}
		}

		static void CheckForNullTasks (Task[] tasks)
		{
			foreach (var t in tasks)
				if (t == null)
					throw new ArgumentNullException ("tasks", "the tasks argument contains a null element");
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
				invoker = null;
				state = null;
				if (cancellationRegistration != null)
					cancellationRegistration.Value.Dispose ();
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

			Task continuation = new Task (TaskActionInvoker.Create (continuationAction),
										  state, cancellationToken,
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

			var t = new Task<TResult> (TaskActionInvoker.Create (continuationFunction),
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
				if (exSlot == null)
					return null;
				exSlot.Observed = true;
				return exSlot.Exception;
			}
			internal set {
				ExceptionSlot.Exception = value;
			}
		}
		
		public bool IsCanceled {
			get {
				return status == TaskStatus.Canceled;
			}
		}

		public bool IsCompleted {
			get {
				return status >= TaskStatus.RanToCompletion;
			}
		}
		
		public bool IsFaulted {
			get {
				return status == TaskStatus.Faulted;
			}
		}

		public TaskCreationOptions CreationOptions {
			get {
				return taskCreationOptions & MaxTaskCreationOptions;
			}
		}
		
		public TaskStatus Status {
			get {
				return status;
			}
			internal set {
				status = value;
				Thread.MemoryBarrier ();
			}
		}

		TaskExceptionSlot ExceptionSlot {
			get {
				if (exSlot != null)
					return exSlot;
				Interlocked.CompareExchange (ref exSlot, new TaskExceptionSlot (this), null);
				return exSlot;
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

		bool IsContinuation {
			get {
				return (taskCreationOptions & TaskCreationOptionsContinuation) != 0;
			}
		}

		internal Task Parent {
			get {
				return parent;
			}
		}
		
		internal string DisplayActionMethod {
			get {
				Delegate d = invoker.Action;
				return d == null ? "<none>" : d.Method.ToString ();
			}
		}
		
		#endregion
	}
}
#endif
