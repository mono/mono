//
// Task_T.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc.
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

using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
	[System.Diagnostics.DebuggerDisplay ("Id = {Id}, Status = {Status}, Result = {ResultAsString}")]
	[System.Diagnostics.DebuggerTypeProxy (typeof (TaskDebuggerView))]
	public class Task<TResult> : Task
	{
		static readonly TaskFactory<TResult> factory = new TaskFactory<TResult> ();

		TResult value;
		
		[System.Diagnostics.DebuggerBrowsable (System.Diagnostics.DebuggerBrowsableState.Never)]
		public TResult Result {
			get {
				if (!IsCompleted)
					Wait ();
				if (IsCanceled)
					throw new AggregateException (new TaskCanceledException (this));
				if (Exception != null)
					throw Exception;
				return value;
			}
			internal set {
				this.value = value;
			}
		}

		string ResultAsString {
			get {
				if ((Status & (TaskStatus.RanToCompletion)) != 0)
					return "" + value;
				
				return "<value not available>";
			}
		}
		
		public static new TaskFactory<TResult> Factory {
			get {
				return factory;
			}
		}
		
		public Task (Func<TResult> function)
			: this (function, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<TResult> function, CancellationToken cancellationToken)
			: this (function, cancellationToken, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<TResult> function, TaskCreationOptions creationOptions)
			: this (function, CancellationToken.None, creationOptions)
		{
			
		}
		
		public Task (Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
			: base (TaskActionInvoker.Create (function), null, cancellationToken, creationOptions)
		{
			if (function == null)
				throw new ArgumentNullException ("function");
		}
		
		public Task (Func<object, TResult> function, object state)
			: this (function, state, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<object, TResult> function, object state, CancellationToken cancellationToken)
			: this (function, state, cancellationToken, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
			: this (function, state, CancellationToken.None, creationOptions)
		{
			
		}

		public Task (Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
			: base (TaskActionInvoker.Create (function), state, cancellationToken, creationOptions)
		{
			if (function == null)
				throw new ArgumentNullException ("function");
		}

		internal Task (TaskActionInvoker invoker, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, Task parent, Task contAncestor = null, bool ignoreCancellation = false)
			: base (invoker, state, cancellationToken, creationOptions, parent, contAncestor, ignoreCancellation)
		{
		}

		public Task ContinueWith (Action<Task<TResult>> continuationAction)
		{
			return ContinueWith (continuationAction, TaskContinuationOptions.None);
		}
		
		public Task ContinueWith (Action<Task<TResult>> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith (continuationAction, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task<TResult>> continuationAction, CancellationToken cancellationToken)
		{
			return ContinueWith (continuationAction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task ContinueWith (Action<Task<TResult>> continuationAction, TaskScheduler scheduler)
		{
			return ContinueWith (continuationAction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}
		
		public Task ContinueWith (Action<Task<TResult>> continuationAction, CancellationToken cancellationToken,
		                          TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			if (continuationAction == null)
				throw new ArgumentNullException ("continuationAction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			Task t = new Task (TaskActionInvoker.Create (continuationAction),
			                   null,
			                   cancellationToken,
			                   GetCreationOptions (continuationOptions),
			                   null,
			                   this);
			ContinueWithCore (t, continuationOptions, scheduler);
			
			return t;
		}

		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, TNewResult> continuationFunction)
		{
			return ContinueWith<TNewResult> (continuationFunction, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, TNewResult> continuationFunction, CancellationToken cancellationToken)
		{
			return ContinueWith<TNewResult> (continuationFunction, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}
		
		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, TNewResult> continuationFunction, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith<TNewResult> (continuationFunction, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}
		
		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, TNewResult> continuationFunction, TaskScheduler scheduler)
		{
			return ContinueWith<TNewResult> (continuationFunction, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}
		
		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, TNewResult> continuationFunction,
		                                                  CancellationToken cancellationToken,
		                                                  TaskContinuationOptions continuationOptions,
		                                                  TaskScheduler scheduler)
		{
			if (continuationFunction == null)
				throw new ArgumentNullException ("continuationFunction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			var t = new Task<TNewResult> (TaskActionInvoker.Create (continuationFunction),
			                              null,
			                              cancellationToken,
			                              GetCreationOptions (continuationOptions),
			                              null,
			                              this);
			ContinueWithCore (t, continuationOptions, scheduler);
			
			return t;
		}

		internal bool TrySetResult (TResult result)
		{
			if (IsCompleted)
				return false;
			
			if (!executing.TryRelaxedSet ()) {
				var sw = new SpinWait ();
				while (!IsCompleted)
					sw.SpinOnce ();

				return false;
			}
			
			Status = TaskStatus.Running;

			this.value = result;
			Thread.MemoryBarrier ();

			Finish ();

			return true;
		}

#if NET_4_5
		public
#else
		internal
#endif
		Task ContinueWith (Action<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken,
								  TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			if (continuationAction == null)
				throw new ArgumentNullException ("continuationAction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			var t = new Task (TaskActionInvoker.Create (continuationAction),
			                  state,
			                  cancellationToken,
			                  GetCreationOptions (continuationOptions),
			                  null,
			                  this);

			ContinueWithCore (t, continuationOptions, scheduler);

			return t;
		}
		
#if NET_4_5

		public Task ContinueWith (Action<Task<TResult>, object> continuationAction, object state)
		{
			return ContinueWith (continuationAction, state, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task ContinueWith (Action<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken)
		{
			return ContinueWith (continuationAction, state, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task ContinueWith (Action<Task<TResult>, object> continuationAction, object state, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith (continuationAction, state, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}

		public Task ContinueWith (Action<Task<TResult>, object> continuationAction, object state, TaskScheduler scheduler)
		{
			return ContinueWith (continuationAction, state, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}

		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, object, TNewResult> continuationFunction, object state)
		{
			return ContinueWith<TNewResult> (continuationFunction, state, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, object, TNewResult> continuationFunction, object state, CancellationToken cancellationToken)
		{
			return ContinueWith<TNewResult> (continuationFunction, state, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
		}

		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskContinuationOptions continuationOptions)
		{
			return ContinueWith<TNewResult> (continuationFunction, state, CancellationToken.None, continuationOptions, TaskScheduler.Current);
		}

		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, object, TNewResult> continuationFunction, object state, TaskScheduler scheduler)
		{
			return ContinueWith<TNewResult> (continuationFunction, state, CancellationToken.None, TaskContinuationOptions.None, scheduler);
		}

		public Task<TNewResult> ContinueWith<TNewResult> (Func<Task<TResult>, object, TNewResult> continuationFunction, object state,
														  CancellationToken cancellationToken,
														  TaskContinuationOptions continuationOptions,
														  TaskScheduler scheduler)
		{
			if (continuationFunction == null)
				throw new ArgumentNullException ("continuationFunction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			var t = new Task<TNewResult> (TaskActionInvoker.Create (continuationFunction),
			                              state,
			                              cancellationToken,
			                              GetCreationOptions (continuationOptions),
			                              null,
			                              this);

			ContinueWithCore (t, continuationOptions, scheduler);

			return t;
		}

		public new ConfiguredTaskAwaitable<TResult> ConfigureAwait (bool continueOnCapturedContext)
		{
			return new ConfiguredTaskAwaitable<TResult> (this, continueOnCapturedContext);
		}

		public new TaskAwaiter<TResult> GetAwaiter ()
		{
			return new TaskAwaiter<TResult> (this);
		}

		internal static Task<TResult> FromException (Exception ex)
		{
			var tcs = new TaskCompletionSource<TResult>();
			tcs.TrySetException (ex);
			return tcs.Task;
		}
#endif
	}
}
#endif
