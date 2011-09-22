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

#if NET_4_0 || MOBILE
using System;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
	[System.Diagnostics.DebuggerDisplay ("Id = {Id}, Status = {Status}, Result = {ResultAsString}")]
	[System.Diagnostics.DebuggerTypeProxy (typeof (TaskDebuggerView))]
	public class Task<TResult> : Task
	{
		internal static new readonly Task<TResult> Canceled = new Task<TResult> (TaskStatus.Canceled);
		static readonly TaskFactory<TResult> factory = new TaskFactory<TResult> ();

		TResult value;
		internal Func<object, TResult> function;
		object state;
		
		[System.Diagnostics.DebuggerBrowsable (System.Diagnostics.DebuggerBrowsableState.Never)]
		public TResult Result {
			get {
				if (function != null)
					Wait ();
				else if (Exception != null)
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
		
		public Task (Func<TResult> function) : this (function, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<TResult> function, CancellationToken cancellationToken)
			: this (function == null ? (Func<object, TResult>)null : (o) => function(), null, cancellationToken, TaskCreationOptions.None)
		{
			
		}
		
		public Task (Func<TResult> function, TaskCreationOptions creationOptions)
			: this (function == null ? (Func<object, TResult>)null : (o) => function(), null, CancellationToken.None, creationOptions)
		{
			
		}
		
		public Task (Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions)
			: this (function == null ? (Func<object, TResult>)null : (o) => function(), null, cancellationToken, creationOptions)
		{
			
		}
		
		public Task (Func<object, TResult> function, object state) : this (function, state, TaskCreationOptions.None)
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
			: base (delegate { }, state, cancellationToken, creationOptions)
		{
			this.function = function;
			this.state = state;
		}

		internal Task (Func<object, TResult> function,
		               object state,
		               CancellationToken cancellationToken,
		               TaskCreationOptions creationOptions,
		               Task parent)
			: base (null, state, cancellationToken, creationOptions, parent)
		{
			this.function = function;
			this.state = state;
		}


		internal Task (TaskStatus status)
			: base (status)
		{
		}
		
		internal override void InnerInvoke ()
		{
			if (function != null)
				value = function (state);
			
			function = null;
			state = null;
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

			Task t = new Task (l => continuationAction ((Task<TResult>)l),
			                   this,
			                   cancellationToken,
			                   GetCreationOptions (continuationOptions),
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

			Task<TNewResult> t = new Task<TNewResult> ((o) => continuationFunction ((Task<TResult>)o),
			                                           this,
			                                           cancellationToken,
			                                           GetCreationOptions (continuationOptions),
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

		public Task ContinueWith (Action<Task<TResult>, object> continuationAction, object state, CancellationToken cancellationToken,
								  TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			if (continuationAction == null)
				throw new ArgumentNullException ("continuationAction");
			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			var t = new Task (l => continuationAction (this, l),
							   state,
							   cancellationToken,
							   GetCreationOptions (continuationOptions),
							   this);

			ContinueWithCore (t, continuationOptions, scheduler);

			return t;
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

			var t = new Task<TNewResult> (l => continuationFunction (this, l),
							   state,
							   cancellationToken,
							   GetCreationOptions (continuationOptions),
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
#endif
	}
}
#endif
