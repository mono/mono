//
// TaskContinuation.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//    Marek Safar  <marek.safar@gmail.com>
//
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

#if NET_4_0

using System.Collections.Generic;

namespace System.Threading.Tasks
{
	interface IContinuation
	{
		void Execute ();
	}

	class TaskContinuation : IContinuation
	{
		readonly Task task;
		readonly TaskContinuationOptions continuationOptions;

		public TaskContinuation (Task task, TaskContinuationOptions continuationOptions)
		{
			this.task = task;
			this.continuationOptions = continuationOptions;
		}

		bool ContinuationStatusCheck (TaskContinuationOptions kind)
		{
			if (kind == TaskContinuationOptions.None)
				return true;

			int kindCode = (int) kind;
			var status = task.ContinuationAncestor.Status;

			if (kindCode >= ((int) TaskContinuationOptions.NotOnRanToCompletion)) {
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

		public void Execute ()
		{
			if (!ContinuationStatusCheck (continuationOptions)) {
				task.CancelReal ();
				task.Dispose ();
				return;
			}

			// The task may have been canceled externally
			if (task.IsCompleted)
				return;

			if ((continuationOptions & TaskContinuationOptions.ExecuteSynchronously) != 0)
				task.RunSynchronouslyCore (task.scheduler);
			else
				task.Schedule ();
		}
	}

	class ActionContinuation : IContinuation
	{
		readonly Action action;

		public ActionContinuation (Action action)
		{
			this.action = action;
		}

		public void Execute ()
		{
			action ();
		}
	}

	class SynchronizationContextContinuation : IContinuation
	{
		readonly Action action;
		readonly SynchronizationContext ctx;

		public SynchronizationContextContinuation (Action action, SynchronizationContext ctx)
		{
			this.action = action;
			this.ctx = ctx;
		}

		public void Execute ()
		{
			ctx.Post (l => ((Action) l) (), action);
		}
	}

	sealed class WhenAllContinuation : IContinuation
	{
		readonly Task owner;
		readonly IList<Task> tasks;
		int counter;

		public WhenAllContinuation (Task owner, IList<Task> tasks)
		{
			this.owner = owner;
			this.counter = tasks.Count;
			this.tasks = tasks;
		}

		public void Execute ()
		{
			if (Interlocked.Decrement (ref counter) != 0)
				return;

			owner.Status = TaskStatus.Running;

			bool canceled = false;
			List<Exception> exceptions = null;
			foreach (var task in tasks) {
				if (task.IsFaulted) {
					if (exceptions == null)
						exceptions = new List<Exception> ();

					exceptions.AddRange (task.Exception.InnerExceptions);
					continue;
				}

				if (task.IsCanceled) {
					canceled = true;
				}
			}

			if (exceptions != null) {
				owner.TrySetException (new AggregateException (exceptions));
				return;
			}

			if (canceled) {
				owner.CancelReal ();
				return;
			}

			owner.Finish ();
		}
	}

	sealed class WhenAllContinuation<TResult> : IContinuation
	{
		readonly Task<TResult[]> owner;
		readonly IList<Task<TResult>> tasks;
		int counter;

		public WhenAllContinuation (Task<TResult[]> owner, IList<Task<TResult>> tasks)
		{
			this.owner = owner;
			this.counter = tasks.Count;
			this.tasks = tasks;
		}

		public void Execute ()
		{
			if (Interlocked.Decrement (ref counter) != 0)
				return;

			bool canceled = false;
			List<Exception> exceptions = null;
			TResult[] results = null;
			for (int i = 0; i < tasks.Count; ++i) {
				var task = tasks [i];
				if (task.IsFaulted) {
					if (exceptions == null)
						exceptions = new List<Exception> ();

					exceptions.AddRange (task.Exception.InnerExceptions);
					continue;
				}

				if (task.IsCanceled) {
					canceled = true;
					continue;
				}

				if (results == null) {
					if (canceled || exceptions != null)
						continue;

					results = new TResult[tasks.Count];
				}

				results[i] = task.Result;
			}

			if (exceptions != null) {
				owner.TrySetException (new AggregateException (exceptions));
				return;
			}

			if (canceled) {
				owner.CancelReal ();
				return;
			}

			owner.TrySetResult (results);
		}
	}

	sealed class WhenAnyContinuation<T> : IContinuation where T : Task
	{
		readonly Task<T> owner;
		readonly IList<T> tasks;
		AtomicBooleanValue executed = new AtomicBooleanValue ();

		public WhenAnyContinuation (Task<T> owner, IList<T> tasks)
		{
			this.owner = owner;
			this.tasks = tasks;
		}

		public void Execute ()
		{
			if (!executed.TryRelaxedSet ())
				return;

			for (int i = 0; i < tasks.Count; ++i) {
				var task = tasks[i];
				if (!task.IsCompleted)
					continue;

				owner.TrySetResult (task);
				return;
			}
		}
	}

	sealed class ManualResetContinuation : IContinuation, IDisposable
	{
		readonly ManualResetEventSlim evt;

		public ManualResetContinuation ()
		{
			this.evt = new ManualResetEventSlim ();
		}

		public ManualResetEventSlim Event {
			get {
				return evt;
			}
		}

		public void Dispose ()
		{
			evt.Dispose ();
		}

		public void Execute ()
		{
			evt.Set ();
		}
	}

	sealed class CountdownContinuation : IContinuation, IDisposable
	{
		readonly CountdownEvent evt;

		public CountdownContinuation (int initialCount)
		{
			this.evt = new CountdownEvent (initialCount);
		}

		public CountdownEvent Event {
			get {
				return evt;
			}
		}

		public void Dispose ()
		{
			evt.Dispose ();
		}

		public void Execute ()
		{
			evt.Signal ();
		}
	}
}

#endif
