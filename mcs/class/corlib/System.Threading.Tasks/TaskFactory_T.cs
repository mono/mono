// 
// TaskFactory_T.cs
//  
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar <marek.safar@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_0 || MOBILE

using System;
using System.Threading;

namespace System.Threading.Tasks
{
	public class TaskFactory<TResult>
	{
		readonly TaskScheduler scheduler;
		TaskCreationOptions creationOptions;
		TaskContinuationOptions continuationOptions;
		CancellationToken cancellationToken;
		
		TaskFactory parent;

		const TaskCreationOptions FromAsyncOptionsNotSupported = TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
		
		public TaskFactory ()
			: this (CancellationToken.None)
		{	
		}
		
		public TaskFactory (TaskScheduler scheduler)
			: this (CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler)
		{	
		}
		
		public TaskFactory (CancellationToken cancellationToken)
			: this (cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, null)
		{	
		}
		
		public TaskFactory (TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions)
			: this (CancellationToken.None, creationOptions, continuationOptions, null)
		{	
		}
		
		public TaskFactory (CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			this.cancellationToken = cancellationToken;
			this.scheduler = scheduler;
			this.creationOptions = creationOptions;
			this.continuationOptions = continuationOptions;

			TaskFactory.CheckContinuationOptions (continuationOptions);
			
			this.parent = new TaskFactory (cancellationToken, creationOptions, continuationOptions, scheduler);
		}

		public TaskScheduler Scheduler {
			get {
				return scheduler;
			}
		}
		
		public TaskContinuationOptions ContinuationOptions {
			get {
				return continuationOptions;
			}
		}
		
		public TaskCreationOptions CreationOptions {
			get {
				return creationOptions;
			}
		}
		
		public CancellationToken CancellationToken {
			get {
				return cancellationToken;
			}
		}
		
		#region StartNew for Task<TResult>	
		public Task<TResult> StartNew (Func<TResult> function)
		{
			return StartNew (function, cancellationToken, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> StartNew (Func<TResult> function, TaskCreationOptions creationOptions)
		{
			return StartNew (function, cancellationToken, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> StartNew (Func<TResult> function, CancellationToken cancellationToken)
		{
			return StartNew (function, cancellationToken, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> StartNew (Func<TResult> function, 
		                               CancellationToken cancellationToken,
		                               TaskCreationOptions creationOptions,
		                               TaskScheduler scheduler)
		{
			return StartNew ((o) => function (), null, cancellationToken, creationOptions, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state)
		{
			return StartNew (function, state, cancellationToken, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
		{
			return StartNew (function, state, cancellationToken, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, CancellationToken cancellationToken)
		{
			return StartNew (function, state, cancellationToken, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, 
		                               CancellationToken cancellationToken,
		                               TaskCreationOptions creationOptions,
		                               TaskScheduler scheduler)
		{
			return parent.StartNew<TResult> (function, state, cancellationToken, creationOptions, scheduler);
		}
		#endregion
		
		#region Continue

		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationFunction)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}

		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationFunction,
		                                      CancellationToken cancellationToken)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}

		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationFunction,
		                                      TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}

		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationFunction,
		                                      CancellationToken cancellationToken,
		                                      TaskContinuationOptions continuationOptions,
		                                      TaskScheduler scheduler)
		{
			return parent.ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction,
		                                                         CancellationToken cancellationToken)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction,
		                                                         TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction,
		                                                         CancellationToken cancellationToken,
		                                                         TaskContinuationOptions continuationOptions,
		                                                         TaskScheduler scheduler)
		{
			return parent.ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks, Func<Task[], TResult> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      CancellationToken cancellationToken)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      CancellationToken cancellationToken,
		                                      TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			return parent.ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         CancellationToken cancellationToken)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, GetScheduler ());
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         CancellationToken cancellationToken,
		                                                         TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			return parent.ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		#endregion
		
		#region FromAsync
		
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
		{
			return FromAsync (asyncResult, endMethod, creationOptions);
		}
		
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions)
		{
			return FromAsync (asyncResult, endMethod, creationOptions, GetScheduler ());
		}
		
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
		{
			return FromIAsyncResult (asyncResult, endMethod, creationOptions, scheduler);
		}

		internal static Task<TResult> FromIAsyncResult (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod, TaskCreationOptions creationOptions, TaskScheduler scheduler)
		{
			if (asyncResult == null)
			    throw new ArgumentNullException ("asyncResult");

			if (endMethod == null)
			    throw new ArgumentNullException ("endMethod");

			if (scheduler == null)
				throw new ArgumentNullException ("scheduler");

			if ((creationOptions & FromAsyncOptionsNotSupported) != 0)
				throw new ArgumentOutOfRangeException ("creationOptions");

			var tcs = new TaskCompletionSource<TResult> (creationOptions);
			tcs.Task.function = l => endMethod (asyncResult);

			var task = tcs.Task;

			// Take quick path for completed operations
			if (asyncResult.IsCompleted) {
				try {
					task.RunSynchronously (scheduler);
				} catch (Exception e) {
					tcs.TrySetException (e);
				}
			} else {
				ThreadPool.RegisterWaitForSingleObject (asyncResult.AsyncWaitHandle,
					delegate {
						try {
							task.RunSynchronously (scheduler);
						} catch (Exception e) {
							tcs.TrySetException (e);
						}
					}, null, Timeout.Infinite, true);
			}

			return task;
		}

		public Task<TResult> FromAsync (Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                object state)
		{
			return FromAsync (beginMethod, endMethod, state, creationOptions);
		}

		public Task<TResult> FromAsync (Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                object state, TaskCreationOptions creationOptions)
		{
			return FromAsyncBeginEnd (beginMethod, endMethod, state, creationOptions);
		}

		internal static Task<TResult> FromAsyncBeginEnd (Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
										  object state, TaskCreationOptions creationOptions)
		{
			if (beginMethod == null)
				throw new ArgumentNullException ("beginMethod");

			if (endMethod == null)
				throw new ArgumentNullException ("endMethod");

			if ((creationOptions & FromAsyncOptionsNotSupported) != 0)
				throw new ArgumentOutOfRangeException ("creationOptions");

			var tcs = new TaskCompletionSource<TResult> (state, creationOptions);
			beginMethod (l => {
				try {
					tcs.SetResult (endMethod (l));
				} catch (Exception e) {
					tcs.SetException (e);
				}
			}, state);

			return tcs.Task;
		}

		public Task<TResult> FromAsync<TArg1> (Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                       TArg1 arg1, object state)
		{
			return FromAsync (beginMethod, endMethod, arg1, state, creationOptions);
		}

		public Task<TResult> FromAsync<TArg1> (Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                       TArg1 arg1, object state, TaskCreationOptions creationOptions)
		{
			return FromAsyncBeginEnd (beginMethod, endMethod, arg1, state, creationOptions);
		}

		internal static Task<TResult> FromAsyncBeginEnd<TArg1> (Func<TArg1, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
										  TArg1 arg1, object state, TaskCreationOptions creationOptions)
		{
			if (beginMethod == null)
				throw new ArgumentNullException ("beginMethod");

			if (endMethod == null)
				throw new ArgumentNullException ("endMethod");

			if ((creationOptions & FromAsyncOptionsNotSupported) != 0)
				throw new ArgumentOutOfRangeException ("creationOptions");

			var tcs = new TaskCompletionSource<TResult> (state, creationOptions);
			beginMethod (arg1, l => {
				try {
					tcs.SetResult (endMethod (l));
				} catch (Exception e) {
					tcs.SetException (e);
				}
			}, state);

			return tcs.Task;
		}

		public Task<TResult> FromAsync<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                              TArg1 arg1, TArg2 arg2, object state)
		{
			return FromAsync (beginMethod, endMethod, arg1, arg2, state, creationOptions);
		}

		public Task<TResult> FromAsync<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                              TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
		{
			return FromAsyncBeginEnd (beginMethod, endMethod, arg1, arg2, state, creationOptions);
		}

		internal static Task<TResult> FromAsyncBeginEnd<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
										  TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
		{
			if (beginMethod == null)
				throw new ArgumentNullException ("beginMethod");

			if (endMethod == null)
				throw new ArgumentNullException ("endMethod");

			if ((creationOptions & FromAsyncOptionsNotSupported) != 0)
				throw new ArgumentOutOfRangeException ("creationOptions");

			var tcs = new TaskCompletionSource<TResult> (state, creationOptions);
			beginMethod (arg1, arg2, l => {
				try {
					tcs.SetResult (endMethod (l));
				} catch (Exception e) {
					tcs.SetException (e);
				}
			}, state);

			return tcs.Task;
		}

		public Task<TResult> FromAsync<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                                     TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
		{
			return FromAsync (beginMethod, endMethod, arg1, arg2, arg3, state, creationOptions);
		}

		public Task<TResult> FromAsync<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
		                                                     TArg1 arg1, TArg2 arg2, TArg3 arg3, object state,
		                                                     TaskCreationOptions creationOptions)
		{
			return FromAsyncBeginEnd (beginMethod, endMethod, arg1, arg2, arg3, state, creationOptions);
		}

		internal static Task<TResult> FromAsyncBeginEnd<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod,
										  TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
		{
			if (beginMethod == null)
				throw new ArgumentNullException ("beginMethod");

			if (endMethod == null)
				throw new ArgumentNullException ("endMethod");

			if ((creationOptions & FromAsyncOptionsNotSupported) != 0)
				throw new ArgumentOutOfRangeException ("creationOptions");

			var tcs = new TaskCompletionSource<TResult> (state, creationOptions);
			beginMethod (arg1, arg2, arg3, l => {
				try {
					tcs.SetResult (endMethod (l));
				} catch (Exception e) {
					tcs.SetException (e);
				}
			}, state);

			return tcs.Task;
		}

		#endregion

		TaskScheduler GetScheduler ()
		{
			return scheduler ?? TaskScheduler.Current;
		}
	}
}
#endif
