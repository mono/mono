// 
// TaskFactory.cs
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

#if NET_4_0 || BOOTSTRAP_NET_4_0
using System;
using System.Threading;

namespace System.Threading.Tasks
{
	
	public class TaskFactory
	{
		TaskScheduler scheduler;
		TaskCreationOptions options;
		TaskContinuationOptions contOptions;
		CancellationToken token;
		
		#region ctors
		public TaskFactory ()
			: this (CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Current)
		{
		}

		public TaskFactory (CancellationToken token)
			: this (token, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Current)
		{	
		}

		public TaskFactory (TaskScheduler scheduler)
			: this (CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, scheduler)
		{	
		}
		
		public TaskFactory (TaskCreationOptions options, TaskContinuationOptions contOptions)
			: this (CancellationToken.None, options, contOptions, TaskScheduler.Current)
		{	
		}
		
		public TaskFactory (CancellationToken token, TaskCreationOptions options, TaskContinuationOptions contOptions,
		                    TaskScheduler scheduler)
		{
			this.token = token;
			this.scheduler = scheduler;
			this.options = options;
			this.contOptions = contOptions;
		}
		#endregion
		
		#region StartNew for Task
		public Task StartNew (Action action)
		{
			return StartNew (action, token, options, scheduler);
		}
		
		public Task StartNew (Action action, CancellationToken token)
		{
			return StartNew (action, token, options, scheduler);
		}
		
		public Task StartNew (Action action, TaskCreationOptions options)
		{
			return StartNew (action, token, options, scheduler);
		}
		
		public Task StartNew (Action<object> action, object state)
		{
			return StartNew (action, state, token, options, scheduler);
		}
		
		public Task StartNew (Action<object> action, object state, CancellationToken token)
		{
			return StartNew (action, state, token, options, scheduler);
		}
		
		public Task StartNew (Action<object> action, object state, TaskCreationOptions options)
		{
			return StartNew (action, state, token, options, scheduler);
		}
		
		public Task StartNew (Action action, CancellationToken token, TaskCreationOptions options, TaskScheduler scheduler)
		{
			return StartNew ((o) => action (), null, token, options, scheduler);
		}
		
		public Task StartNew (Action<object> action, object state, CancellationToken token, TaskCreationOptions options,
		                      TaskScheduler scheduler)
		{
			Task t = new Task (action, state, token, options);
			t.Start (scheduler);
			
			return t;
		}
		#endregion
		
		#region StartNew for Task<TResult>	
		public Task<TResult> StartNew<TResult> (Func<TResult> function)
		{
			return StartNew<TResult> (function, token, options, scheduler);
		}
		
		public Task<TResult> StartNew<TResult> (Func<TResult> function, TaskCreationOptions options)
		{
			return StartNew<TResult> (function, token, options, scheduler);

		}
		
		public Task<TResult> StartNew<TResult> (Func<TResult> function, CancellationToken token)
		{
			return StartNew<TResult> (function, token, options, scheduler);
		}
		
		public Task<TResult> StartNew<TResult> (Func<TResult> function,
		                                        CancellationToken token,
		                                        TaskCreationOptions options,
		                                        TaskScheduler scheduler)
		{
			return StartNew<TResult> ((o) => function (), null, token, options, scheduler);
		}
		
		public Task<TResult> StartNew<TResult> (Func<object, TResult> function, object state)
		{
			return StartNew<TResult> (function, state, token, options, scheduler);
		}
		
		public Task<TResult> StartNew<TResult> (Func<object, TResult> function, object state, CancellationToken token)
		{
			return StartNew<TResult> (function, state, token, options, scheduler);
		}
		
		public Task<TResult> StartNew<TResult> (Func<object, TResult> function, object state, TaskCreationOptions options)
		{
			return StartNew<TResult> (function, state, token, options, scheduler);
		}
		
		public Task<TResult> StartNew<TResult> (Func<object, TResult> function, object state,
		                                        CancellationToken token,
		                                        TaskCreationOptions options,
		                                        TaskScheduler scheduler)
		{
			Task<TResult> t = new Task<TResult> (function, state, token, options);
			t.Start (scheduler);
			
			return t;
		}
		#endregion
		
		#region Continue
		
		public Task ContinueWhenAny (Task[] tasks, Action<Task> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAny (Task[] tasks, Action<Task> continuationAction, CancellationToken token)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAny (Task[] tasks, Action<Task> continuationAction, TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, token, continuationOptions, scheduler);
		}

		public Task ContinueWhenAny (Task[] tasks, Action<Task> continuationAction, CancellationToken token, 
		                             TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			AtomicBoolean trigger = new AtomicBoolean ();
			Task commonContinuation = new Task (null);
			
			foreach (Task t in tasks) {
				Task cont = new Task ((o) => { continuationAction ((Task)o); }, t, token, options);
				t.ContinueWithCore (cont, continuationOptions, scheduler, trigger.TrySet);
				cont.ContinueWithCore (commonContinuation, TaskContinuationOptions.None, scheduler);
			}
			
			return commonContinuation;
		}
		
		public Task ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction,
		                                                CancellationToken token)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction,
		                                                TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, token, continuationOptions, scheduler);
		}

		public Task ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>> continuationAction,
		                                                CancellationToken token, TaskContinuationOptions continuationOptions,
		                                                TaskScheduler scheduler)
		{
			return ContinueWhenAny ((Task[]) tasks, (o) => continuationAction ((Task<TAntecedentResult>)o),
			                        token, continuationOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TResult> (Task[] tasks, Func<Task, TResult> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TResult> (Task[] tasks, Func<Task, TResult> continuationAction,
		                                               CancellationToken token)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TResult> (Task[] tasks, Func<Task, TResult> continuationAction,
		                                               TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, token, continuationOptions, scheduler);
		}

		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TResult> (Task[] tasks, Func<Task, TResult> continuationAction,
		                                               CancellationToken token,
		                                               TaskContinuationOptions continuationOptions,
		                                               TaskScheduler scheduler)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks,
		                                                                  Func<Task<TAntecedentResult>, TResult> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}

		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks,
		                                                                  Func<Task<TAntecedentResult>, TResult> continuationAction,
		                                                                  CancellationToken token)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks,
		                                                                  Func<Task<TAntecedentResult>, TResult> continuationAction,
		                                                                  TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, token, continuationOptions, scheduler);
		}

		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks,
		                                                                  Func<Task<TAntecedentResult>, TResult> continuationAction,
		                                                                  CancellationToken token,
		                                                                  TaskContinuationOptions continuationOptions,
		                                                                  TaskScheduler scheduler)
		{
			return ContinueWhenAny<TResult> ((Task[])tasks, (t) => continuationAction((Task<TAntecedentResult>)t), token, continuationOptions, scheduler);
		}
		
		public Task ContinueWhenAll (Task[] tasks, Action<Task[]> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAll (Task[] tasks, Action<Task[]> continuationFunction, CancellationToken token)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAll (Task[] tasks, Action<Task[]> continuationFunction,
		                             TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, continuationOptions, scheduler);
		}
		
		public Task ContinueWhenAll (Task[] tasks, Action<Task[]> continuationFunction, CancellationToken token,
		                             TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			CountdownEvent evt = new CountdownEvent (tasks.Length);
			Task cont = new Task ((o) => continuationFunction ((Task[])o), tasks, token, options);
			
			foreach (Task t in tasks)
				t.ContinueWithCore (cont, continuationOptions, scheduler, evt.Signal);
			
			return cont;
		}
		
		public Task ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                Action<Task<TAntecedentResult>[]> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                Action<Task<TAntecedentResult>[]> continuationFunction, CancellationToken token)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks, Action<Task<TAntecedentResult>[]> continuationFunction,
		                                                TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, continuationOptions, scheduler);
		}
		
		public Task ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks, 
		                                                Action<Task<TAntecedentResult>[]> continuationFunction,
		                                                CancellationToken token, TaskContinuationOptions continuationOptions,
		                                                TaskScheduler scheduler)
		{
			return ContinueWhenAll ((Task[]) tasks, (o) => continuationFunction (tasks), token,
			                        continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TResult> (Task[] tasks, Func<Task[], TResult> continuationFunction)
		{
			return ContinueWhenAll<TResult> (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TResult> (Task[] tasks, Func<Task[], TResult> continuationFunction,
		                                               TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll<TResult> (tasks, continuationFunction, token, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TResult> (Task[] tasks, Func<Task[], TResult> continuationFunction,
		                                               CancellationToken token)
		{
			return ContinueWhenAll<TResult> (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TResult> (Task[] tasks, Func<Task[], TResult> continuationFunction,
		                                               CancellationToken token,
		                                               TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			CountdownEvent evt = new CountdownEvent (tasks.Length);
			Task<TResult> cont = new Task<TResult> ((o) => continuationFunction ((Task[])o), tasks, token, options);
			
			foreach (Task t in tasks)
				t.ContinueWithCore (cont, continuationOptions, scheduler, evt.Signal);
			
			return cont;
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks,
		                                                                  Func<Task<TAntecedentResult>[], TResult> continuationFunction)
		{
			return ContinueWhenAll<TAntecedentResult, TResult> (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks, 
		                                                                  Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                                  TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll<TAntecedentResult, TResult> (tasks, continuationFunction, token, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks,
		                                                                  Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                                  CancellationToken token)
		{
			return ContinueWhenAll<TAntecedentResult, TResult> (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult, TResult> (Task<TAntecedentResult>[] tasks, 
		                                                                  Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                                  CancellationToken token,
		                                                                  TaskContinuationOptions continuationOptions,
		                                                                  TaskScheduler scheduler)
		{
			return ContinueWhenAll<TResult> ((Task[]) tasks,
			                                 (o) => continuationFunction (tasks),
			                                 token,
			                                 continuationOptions, scheduler);
		}

		#endregion
		
		#region FromAsync
		// For these methods to work we first have to convert the ThreadPool to use Tasks as it
		// is doing in 4.0, then all that is remaining is to identify the Task on which is 
		// run the async operation (probably with some additional state in a IAsyncResult subclass)
		// and call its ContinueWith method accordingly
		
		const string errorMsg = "Mono's thread pool doesn't support this operation yet";
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync (IAsyncResult asyncResult, Action<IAsyncResult> endMethod)
		{
			return FromAsync (asyncResult, endMethod, options);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync (IAsyncResult asyncResult, Action<IAsyncResult> endMethod,
		                       TaskCreationOptions creationOptions)
		{
			return FromAsync (asyncResult, endMethod, creationOptions);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync (IAsyncResult asyncResult, Action<IAsyncResult> endMethod,
		                       TaskCreationOptions creationOptions, TaskScheduler scheduler)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TResult> (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
		{
			return FromAsync<TResult> (asyncResult, endMethod, options);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TResult> (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod,
		                                         TaskCreationOptions creationOptions)
		{
			return FromAsync<TResult> (asyncResult, endMethod, creationOptions);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TResult> (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod,
		                                         TaskCreationOptions creationOptions, TaskScheduler scheduler)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync (Func<AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                       object state)
		{
			return FromAsync<object> ((a, c, o) => beginMethod (c, o), endMethod, state, options);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync (Func<AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                       object state, TaskCreationOptions creationOptions)
		{
			return FromAsync<object> ((a, c, o) => beginMethod (c, o), endMethod, state, creationOptions);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync<TArg1> (Func<TArg1, AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                              TArg1 arg1, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync<TArg1> (Func<TArg1, AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                              TArg1 arg1, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                                     TArg1 arg1, TArg2 arg2, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync<TArg1, TArg2> (Func<TArg1, TArg2, AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                                     TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                                            TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task FromAsync<TArg1, TArg2, TArg3> (Func<TArg1, TArg2, TArg3, AsyncCallback, Object, IAsyncResult> beginMethod, Action<IAsyncResult> endMethod,
		                                            TArg1 arg1, TArg2 arg2, TArg3 arg3, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}		
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TResult> (Func<AsyncCallback, Object, IAsyncResult> beginMethod,
		                                         Func<IAsyncResult, TResult> endMethod,
		                                         object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TResult> (Func<AsyncCallback, Object, IAsyncResult> beginMethod,
		                                         Func<IAsyncResult, TResult> endMethod,
		                       object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TResult> (Func<TArg1, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                                Func<IAsyncResult, TResult> endMethod,
		                                                TArg1 arg1, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TResult> (Func<TArg1, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                             Func<IAsyncResult, TResult> endMethod,
		                                             TArg1 arg1, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2, TResult> (Func<TArg1, TArg2, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                                       Func<IAsyncResult, TResult> endMethod,
		                                                       TArg1 arg1, TArg2 arg2, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2, TResult> (Func<TArg1, TArg2, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                                       Func<IAsyncResult, TResult> endMethod,
		                                                       TArg1 arg1, TArg2 arg2, object state, TaskCreationOptions creationOptions)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult> (Func<TArg1, TArg2, TArg3, AsyncCallback, Object, IAsyncResult> beginMethod,
		                                                              Func<IAsyncResult, TResult> endMethod,
		                                                              TArg1 arg1, TArg2 arg2, TArg3 arg3, object state)
		{
			throw new NotSupportedException (errorMsg);
		}
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync<TArg1, TArg2, TArg3, TResult> (Func<TArg1, TArg2, TArg3, AsyncCallback, Object, IAsyncResult> beginMethod,
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
				return contOptions;
			}
		}
		
		public TaskCreationOptions CreationOptions {
			get {
				return options;
			}
		}
		
		public CancellationToken CancellationToken {
			get {
				return token;
			}
		}
	}
	
	public class TaskFactory<TResult>
	{
		TaskScheduler scheduler;
		TaskCreationOptions options;
		TaskContinuationOptions contOptions;
		CancellationToken token;
		
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
		
		public TaskFactory (CancellationToken token)
			: this (token, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Current)
		{	
		}
		
		public TaskFactory (TaskCreationOptions options, TaskContinuationOptions contOptions)
			: this (CancellationToken.None, options, contOptions, TaskScheduler.Current)
		{	
		}
		
		public TaskFactory (CancellationToken token, TaskCreationOptions options, TaskContinuationOptions contOptions,
		                    TaskScheduler scheduler)
		{
			this.token = token;
			this.scheduler = scheduler;
			this.options = options;
			this.contOptions = contOptions;
			
			this.parent = new TaskFactory (token, options, contOptions, scheduler);
		}
		
		#endregion
		
		#region StartNew for Task<TResult>	
		public Task<TResult> StartNew (Func<TResult> function)
		{
			return StartNew (function, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<TResult> function, TaskCreationOptions options)
		{
			return StartNew (function, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<TResult> function, CancellationToken token)
		{
			return StartNew (function, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<TResult> function, 
		                               CancellationToken token,
		                               TaskCreationOptions options,
		                               TaskScheduler scheduler)
		{
			return StartNew ((o) => function (), null, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state)
		{
			return StartNew (function, state, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, TaskCreationOptions options)
		{
			return StartNew (function, state, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, CancellationToken token)
		{
			return StartNew (function, state, token, options, scheduler);
		}
		
		public Task<TResult> StartNew (Func<object, TResult> function, object state, 
		                               CancellationToken token,
		                               TaskCreationOptions options,
		                               TaskScheduler scheduler)
		{
			return parent.StartNew<TResult> (function, state, token, options, scheduler);
		}
		#endregion
		
		#region Continue
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationAction,
		                                      CancellationToken token)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationAction,
		                                      TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, token, continuationOptions, scheduler);
		}

		[MonoTODO]
		public Task<TResult> ContinueWhenAny (Task[] tasks,
		                                      Func<Task, TResult> continuationAction,
		                                      CancellationToken token,
		                                      TaskContinuationOptions continuationOptions,
		                                      TaskScheduler scheduler)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationAction)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationAction,
		                                                         CancellationToken token)
		{
			return ContinueWhenAny (tasks, continuationAction, token, contOptions, scheduler);
		}
		
		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationAction,
		                                                         TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAny (tasks, continuationAction, token, continuationOptions, scheduler);
		}

		[MonoTODO]
		public Task<TResult> ContinueWhenAny<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>, TResult> continuationAction,
		                                                         CancellationToken token,
		                                                         TaskContinuationOptions continuationOptions,
		                                                         TaskScheduler scheduler)
		{
			throw new NotImplementedException ();
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      CancellationToken token)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll (Task[] tasks,
		                                      Func<Task[], TResult> continuationFunction,
		                                      CancellationToken token,
		                                      TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			CountdownEvent evt = new CountdownEvent (tasks.Length);
			Task<TResult> cont = new Task<TResult> ((o) => continuationFunction (tasks), tasks, token, options);
			
			foreach (Task t in tasks)
				t.ContinueWithCore (cont, continuationOptions, scheduler, evt.Signal);
			
			return cont;
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         TaskContinuationOptions continuationOptions)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, continuationOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         CancellationToken token)
		{
			return ContinueWhenAll (tasks, continuationFunction, token, contOptions, scheduler);
		}
		
		public Task<TResult> ContinueWhenAll<TAntecedentResult> (Task<TAntecedentResult>[] tasks,
		                                                         Func<Task<TAntecedentResult>[], TResult> continuationFunction,
		                                                         CancellationToken token,
		                                                         TaskContinuationOptions continuationOptions, TaskScheduler scheduler)
		{
			CountdownEvent evt = new CountdownEvent (tasks.Length);
			Task<TResult> cont = new Task<TResult> ((o) => continuationFunction (tasks), tasks, token, options);
			
			foreach (Task t in tasks)
				t.ContinueWithCore (cont, continuationOptions, scheduler, evt.Signal);
			
			return cont;
		}

		#endregion
		
		#region FromAsync
		const string errorMsg = "Mono's thread pool doesn't support this operation yet";
		
		[MonoLimitation(errorMsg)]
		public Task<TResult> FromAsync (IAsyncResult asyncResult, Func<IAsyncResult, TResult> endMethod)
		{
			return FromAsync (asyncResult, endMethod, options);
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
				return contOptions;
			}
		}
		
		public TaskCreationOptions CreationOptions {
			get {
				return options;
			}
		}
		
		public CancellationToken CancellationToken {
			get {
				return token;
			}
		}
	}
}
#endif
