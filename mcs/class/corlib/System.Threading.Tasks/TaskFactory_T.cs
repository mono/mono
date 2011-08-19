// 
// TaskFactory_T.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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
		TaskScheduler scheduler;
		TaskCreationOptions creationOptions;
		TaskContinuationOptions continuationOptions;
		CancellationToken cancellationToken;
		
		TaskFactory parent;
		
		#region ctors
		public TaskFactory ()
			: this (CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Current)
		{	
		}
		
		public TaskFactory (TaskScheduler scheduler)
			: this (CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler)
		{	
		}
		
		public TaskFactory (CancellationToken cancellationToken)
			: this (cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Current)
		{	
		}
		
		public TaskFactory (TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions)
			: this (CancellationToken.None, creationOptions, continuationOptions, TaskScheduler.Current)
		{	
		}
		
		public TaskFactory (CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskContinuationOptions continuationOptions,
		                    TaskScheduler scheduler)
		{
			this.cancellationToken = cancellationToken;
			this.scheduler = scheduler;
			this.creationOptions = creationOptions;
			this.continuationOptions = continuationOptions;
			
			this.parent = new TaskFactory (cancellationToken, creationOptions, continuationOptions, scheduler);
		}
		
		#endregion
		
		#region StartNew for Task<TResult>	
		public Task<TResult> StartNew (Func<TResult> function)
		{
			return StartNew (function, cancellationToken, creationOptions, scheduler);
		}
		
		public Task<TResult> StartNew (Func<TResult> function, TaskCreationOptions creationOptions)
		{
			return StartNew (function, cancellationToken, creationOptions, scheduler);
		}
		
		public Task<TResult> StartNew (Func<TResult> function, CancellationToken cancellationToken)
		{
			return StartNew (function, cancellationToken, creationOptions, scheduler);
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
			return StartNew (function, state, cancellationToken, creationOptions, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
		{
			return StartNew (function, state, cancellationToken, creationOptions, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, CancellationToken cancellationToken)
		{
			return StartNew (function, state, cancellationToken, creationOptions, scheduler);
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
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationFunction,
		                                      CancellationToken cancellationToken)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationFunction,
		                                      TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
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
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction,
		                                                         CancellationToken cancellationToken)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction,
		                                                         TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}

		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationFunction,
		                                                         CancellationToken cancellationToken,
		                                                         TaskContinuationOptions continuationOptions,
		                                                         TaskScheduler scheduler)
		{
			return parent.ContinueWhenAny (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      CancellationToken cancellationToken)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
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
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         CancellationToken cancellationToken)
		{
			return ContinueWhenAll (tasks, continuationFunction, cancellationToken, continuationOptions, scheduler);
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
		const string errorMsg = "Mono's thread pool doesn't support this operation yet";
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
		{
			return FromAsync (asyncResult, endMethod, creationOptions);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod,
		                                TaskCreationOptions creationOptions)
		{
			return FromAsync (asyncResult, endMethod, creationOptions);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod,
		                                TaskCreationOptions creationOptions, TaskScheduler scheduler)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync (Func<AsyncCallback, Object, IAsyncResult> beginMethod,
		                                Func<IAsyncResult, TResult> endMethod,
		                                object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync (Func<AsyncCallback, Object, IAsyncResult> beginMethod,
		                                Func<IAsyncResult, TResult> endMethod,
		                                object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1> (Func<TArg1, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                       Func<IAsyncResult, TResult> endMethod,
		                                       TArg1 arg1, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1> (Func<TArg1, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                       Func<IAsyncResult, TResult> endMethod,
		                                       TArg1 arg1, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                              Func<IAsyncResult, TResult> endMethod,
		                                              TArg1 arg1, TArg2 arg2, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                              Func<IAsyncResult, TResult> endMethod,
		                                              TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                                     Func<IAsyncResult, TResult> endMethod,
		                                                     TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                                     Func<IAsyncResult, TResult> endMethod,
		                                                     TArg1 arg1, TArg2 arg2, TArg3 arg3, object state,
		                                                     TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		#endregion
		
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
	}
}
#endif
