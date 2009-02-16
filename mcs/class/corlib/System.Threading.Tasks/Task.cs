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
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	public class Task: TaskBase, IDisposable, IAsyncResult
	{
		// With this attribute each thread has its own value so that it's correct for our Schedule code
		// and for Parent and Creator properties. Though it may not be the value that Current should yield.
		[System.ThreadStatic]
		static Task         current;
		[System.ThreadStatic]
		static Action<Task> childWorkAdder;
		static int          id = 0;
		
		CountdownEvent childTasks = new CountdownEvent(1);
		Task parent  = current;
		Task creator = current;
		
		int                 taskId;
		Exception           exception;
		AtomicBoolean       isCanceled;
		bool                respectParentCancellation;
		bool                isCompleted;
		TaskCreationOptions taskCreationOptions;
		
		// Ugly coding style because of initial API
		protected readonly TaskManager m_taskManager;
		protected readonly object      m_stateObject;
		
		Action<object> action;
		EventHandler   completed;
			
		internal Task(TaskManager tm, Action<object> action, object state, TaskCreationOptions taskCreationOptions)
		{
			this.taskCreationOptions = taskCreationOptions;
			this.m_taskManager = TaskManager.Current = tm;
			this.action = action == null ? EmptyFunc : action;
			this.m_stateObject = state;
			this.taskId = Interlocked.Increment(ref id);

			// Process taskCreationOptions
			if (CheckTaskOptions(taskCreationOptions, TaskCreationOptions.Detached))
				parent = null;
			else if (parent != null)
				parent.AddChild();

			respectParentCancellation = CheckTaskOptions(taskCreationOptions, TaskCreationOptions.RespectCreatorCancellation);
			
			if (CheckTaskOptions(taskCreationOptions, TaskCreationOptions.SelfReplicating))
			  ContinueWith(_ => action(state), TaskContinuationKind.OnAny, taskCreationOptions);
		}

		bool CheckTaskOptions(TaskCreationOptions opt, TaskCreationOptions member)
		{
			return (opt & member) == member;
		}
		
		// TODO: Not useful in our implementation of Task
		// but may break other's API
		/*~Task() {
			Dispose(false);
		}*/

		static void EmptyFunc(object o)
		{
		}
		
		#region Create and ContinueWith
		public static Task StartNew(Action<object> action)
		{
			return StartNew(action, null, TaskManager.Default, TaskCreationOptions.None);
		}
		
		public static Task StartNew(Action<object> action, object state)
		{
			return StartNew(action, state, TaskManager.Current, TaskCreationOptions.None);
		}
		
		public static Task StartNew(Action<object> action, TaskManager tm)
		{
			return StartNew(action, null, tm, TaskCreationOptions.None);
		}
		
		public static Task StartNew(Action<object> action, TaskCreationOptions options)
		{
			return StartNew(action, null, TaskManager.Current, options);
		}
		
		
		public static Task StartNew(Action<object> action, TaskManager tm, TaskCreationOptions options)
		{
			return StartNew(action, null, tm, options);
		}
		
		public static Task StartNew(Action<object> action, object state, TaskManager tm, TaskCreationOptions options)
		{
			Task result = new Task(tm, action, state, options);
			result.Schedule();
			return result;
		}
		
		public Task ContinueWith(Action<Task> a)
		{
			return ContinueWith(a, TaskContinuationKind.OnAny, TaskCreationOptions.None);
		}
		
		public Task ContinueWith(Action<Task> a, TaskContinuationKind kind)
		{
			return ContinueWith(a, kind, TaskCreationOptions.None);
		}
		
		public Task ContinueWith(Action<Task> a, TaskContinuationKind kind, TaskCreationOptions option)
		{
			return ContinueWith(a, kind, option, false);
		}
		
		public Task ContinueWith(Action<Task> a, TaskContinuationKind kind, TaskCreationOptions option, bool executeSync)
		{
			Task continuation = new Task(TaskManager.Current, delegate { a(this); }, null, option);
			ContinueWithCore(continuation, kind, false);
			return continuation;
		}
		
		protected void ContinueWithCore(Task continuation, TaskContinuationKind kind, bool executeSync)
		{
			if (IsCompleted) {
				CheckAndSchedule(executeSync, continuation);
				return;
			}
				
			completed += delegate {
				switch (kind) {
					case TaskContinuationKind.OnAny:
						CheckAndSchedule(executeSync, continuation);
						break;
					case TaskContinuationKind.OnAborted:
						if (isCanceled.Value)
							CheckAndSchedule(executeSync, continuation);
						break;
					case TaskContinuationKind.OnFailed:
						if (exception != null)
							CheckAndSchedule(executeSync, continuation);
						break;
					case TaskContinuationKind.OnCompletedSuccessfully:
						if (exception == null && !isCanceled.Value)
							CheckAndSchedule(executeSync, continuation);
						break;
				}
			};
		}
		
		void CheckAndSchedule(bool executeSync, Task continuation)
		{
			if (executeSync)
				continuation.ThreadStart();
			else
				continuation.Schedule();
		}
		#endregion
		
		#region Internal and protected thingies
		internal protected void Schedule()
		{			
			// If worker is null it means it is a local one, revert to the old behavior
			if (current == null || childWorkAdder == null) {
				m_taskManager.AddWork(this);
			} else {
				/* Like the semantic of the ABP paper describe it, we add ourselves to the bottom 
				 * of our Parent Task's ThreadWorker deque. It's ok to do that since we are in
				 * the correct Thread during the creation
				 */
				childWorkAdder(this);
			}
		}
		
		void ThreadStart()
		{
			if (!isCanceled.Value
			    && (!respectParentCancellation || (respectParentCancellation && parent != null && !parent.IsCanceled))) {
				current = this;
				try {
					InnerInvoke();
				} catch (Exception e) {
					exception = e;
				} finally {
					current = null;
				}
			} else {
				this.exception = new TaskCanceledException(this);
			}
			
			isCompleted = true;
			
			// Call the event in the correct style
			EventHandler tempCompleted = completed;
			if (tempCompleted != null) tempCompleted(this, EventArgs.Empty);
			
			Finish();
		}
		
		internal void Execute(Action<Task> childAdder)
		{
			childWorkAdder = childAdder;
			ThreadStart();
		}
		
		internal void AddChild()
		{
			childTasks.Increment();
		}

		internal void ChildCompleted()
		{
			childTasks.Decrement();
		}

		protected virtual void InnerInvoke()
		{
			action(m_stateObject);
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in an anonymous method
			action = null;
		}
		
		protected void Finish()
		{
			// If there wasn't any child created in the task we set the CountdownEvent
			childTasks.Decrement();
			// Tell parent that we are finished
			if (!CheckTaskOptions(taskCreationOptions, TaskCreationOptions.Detached) && parent != null)
				parent.ChildCompleted();
			Dispose();
		}
		
		internal bool ChildTasks {
			get {
				return childTasks.IsSet;
			}
		}
		#endregion
		
		#region Cancel and Wait related methods
		public void Cancel()
		{
			// Mark the Task as canceled so that the worker function will return immediately
			isCanceled.Value = true;
		}
		
		public void CancelAndWait()
		{
			Cancel();
			Wait();
		}
		
		public bool CancelAndWait(TimeSpan ts)
		{
			Cancel();
			return Wait(ts);
		}
		
		public bool CancelAndWait(int millisecondsTimeout)
		{
			Cancel();
			return Wait(millisecondsTimeout);
		}
		
		public void Wait()
		{
			m_taskManager.WaitForTask(this);
		}
		
		public bool Wait(TimeSpan ts)
		{
			return Wait((int)ts.TotalMilliseconds);
		}
		
		public bool Wait(int millisecondsTimeout)
		{
			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			bool result = m_taskManager.WaitForTaskWithPredicate(this, delegate { return sw.ElapsedMilliseconds >= millisecondsTimeout; });
			sw.Stop();
			return !result;
		}
		
		public static void WaitAll(params Task[] tasks)
		{
			if (tasks == null)
				throw new ArgumentNullException("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException("tasks is empty", "tasks");
			
			foreach (var t in tasks)
				t.Wait();
		}
		
		public static bool WaitAll(Task[] tasks, TimeSpan ts)
		{
			if (tasks == null)
				throw new ArgumentNullException("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException("tasks is empty", "tasks");
			
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait(ts);
			return result;
		}
		
		public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
		{
			if (tasks == null)
				throw new ArgumentNullException("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException("tasks is empty", "tasks");
			
			bool result = true;
			foreach (var t in tasks)
				result &= t.Wait(millisecondsTimeout);
			return result;
		}
		
		// predicate for WaitAny would be numFinished == 1 and for WaitAll numFinished == count
		public static int WaitAny(params Task[] tasks)
		{
			if (tasks == null)
				throw new ArgumentNullException("tasks");
			if (tasks.Length == 0)
				throw new ArgumentException("tasks is empty", "tasks");
			
			int numFinished = 0;
			int indexFirstFinished = -1;
			int index = 0;
			
			foreach (Task t in tasks) {
				if (t.IsCompleted) {
					return index;
				}
				t.completed += delegate (object sender, EventArgs e) {
					int result = Interlocked.Increment(ref numFinished);
					// Check if we are the first to have finished
					if (result == 1) {
						Task target = (Task)sender;
						indexFirstFinished = Array.FindIndex(tasks, (elem) => elem == target);
					}
				};	
				index++;
			}
			
			// All tasks are supposed to use the same TaskManager
			tasks[0].m_taskManager.WaitForTasksUntil(delegate {
				return numFinished >= 1;
			});
			
			return indexFirstFinished;
		}
		
		public static int WaitAny(Task[] tasks, TimeSpan ts)
		{
			return WaitAny(tasks, (int)ts.TotalMilliseconds);
		}
		
		public static int WaitAny(Task[] tasks, int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException("millisecondsTimeout");
			if (tasks == null)
				throw new ArgumentNullException("tasks");
			
			if (millisecondsTimeout == -1)
				return WaitAny(tasks);
			
			int numFinished = 0;
			int indexFirstFinished = -1;
			
			foreach (Task t in tasks) {
				t.completed += delegate (object sender, EventArgs e) { 
					int result = Interlocked.Increment(ref numFinished);
					if (result == 1) {
						Task target = (Task)sender;
						indexFirstFinished = Array.FindIndex(tasks, (elem) => elem == target);
					}
				};	
			}
			
			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			tasks[0].m_taskManager.WaitForTasksUntil(delegate {
				if (sw.ElapsedMilliseconds > millisecondsTimeout)
					return true;
				return numFinished >= 1;
			});
			sw.Stop();
			
			return indexFirstFinished;
		}
		#endregion
		
		#region Dispose
		public void Dispose()
		{
			Dispose(true);
		}
		
		protected virtual void Dispose(bool disposeManagedRes)
		{
			// Set action to null so that the GC can collect the delegate and thus
			// any big object references that the user might have captured in a anonymous method
			if (disposeManagedRes) {
				action = null;
				completed = null;
			}
		}
		#endregion
		
		#region Properties
		public static Task Current {
			get {
				return current;
			}
		}
		
		public Exception Exception {
			get {
				return exception;	
			}
		}
		
		public bool IsCanceled {
			get {
				return isCanceled.Value && isCompleted;
			}
		}

		public bool IsCancellationRequested {
			get {
				return isCanceled.Value;
			}
		}

		public bool IsCompleted {
			get {
				return isCompleted;
			}
		}

		public Task Parent {
			get {
				return parent;
			}
		}
		
		public Task Creator {
			get {
				return creator;	
			}
		}

		public TaskCreationOptions TaskCreationOptions {
			get {
				return taskCreationOptions;
			}
		}

		object IAsyncResult.AsyncState {
			get {
				return m_stateObject;
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
		
		public override string ToString ()
		{
			return Id.ToString();
		}
		#endregion
	}
}
#endif
