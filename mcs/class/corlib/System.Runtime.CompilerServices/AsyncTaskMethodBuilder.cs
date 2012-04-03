//
// AsyncTaskMethodBuilder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_4_5

using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
	public struct AsyncTaskMethodBuilder
	{
		readonly Task<object> task;
		IAsyncStateMachine stateMachine;

		private AsyncTaskMethodBuilder (Task<object> task)
		{
			this.task = task;
			this.stateMachine = null;
		}

		public Task Task {
			get {
				return task;
			}
		}
		
		public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : INotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			var action = new Action (stateMachine.MoveNext);
			awaiter.OnCompleted (action);
		}
		
		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : ICriticalNotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			var action = new Action (stateMachine.MoveNext);
			awaiter.UnsafeOnCompleted (action);
		}
		
		public static AsyncTaskMethodBuilder Create ()
		{
			var task = new Task<object> (TaskActionInvoker.Empty, null, CancellationToken.None, TaskCreationOptions.None, null);
			task.SetupScheduler (TaskScheduler.Current);
			return new AsyncTaskMethodBuilder (task);
		}
		
		public void SetException (Exception exception)
		{
			if (exception is OperationCanceledException) {
				if (Task.TrySetCanceled ())
					return;
			} else {
				if (Task.TrySetException (new AggregateException (exception)))
					return;
			}

			throw new InvalidOperationException ("The task has already completed");
		}

		public void SetStateMachine (IAsyncStateMachine stateMachine)
		{
			if (stateMachine == null)
				throw new ArgumentNullException ("stateMachine");
			
			if (this.stateMachine != null)
				throw new InvalidOperationException ("The state machine was previously set");
			
			this.stateMachine = stateMachine;
		}

		public void SetResult ()
		{
			if (!task.TrySetResult (null))
				throw new InvalidOperationException ("The task has already completed");
		}

		public void Start<TStateMachine> (ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
		{
			if (stateMachine == null)
				throw new ArgumentNullException ("stateMachine");
			
			stateMachine.MoveNext ();
		}
	}
}

#endif