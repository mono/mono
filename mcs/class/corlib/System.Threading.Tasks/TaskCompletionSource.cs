// 
// TaskCompletionSource.cs
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
using System.Collections.Generic;

namespace System.Threading.Tasks
{
	public class TaskCompletionSource<TResult>
	{
		static readonly Func<TResult> emptyFunction = () => default (TResult);
		static readonly Func<object, TResult> emptyParamFunction = (_) => default (TResult);

		static readonly Action<Task<TResult>, TResult> setResultAction = SetResultAction;
		static readonly Action<Task<TResult>, AggregateException> setExceptionAction = SetExceptionAction;
		static readonly Action<Task<TResult>, object> setCanceledAction = SetCanceledAction;

		readonly Task<TResult> source;
		SpinLock opLock = new SpinLock (false);

		public TaskCompletionSource ()
		{
			source = new Task<TResult> (emptyFunction);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public TaskCompletionSource (object state)
		{
			source = new Task<TResult> (emptyParamFunction, state);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public TaskCompletionSource (TaskCreationOptions creationOptions)
		{
			source = new Task<TResult> (emptyFunction, creationOptions);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public TaskCompletionSource (object state, TaskCreationOptions creationOptions)
		{
			source = new Task<TResult> (emptyParamFunction, state, creationOptions);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public void SetCanceled ()
		{
			if (!TrySetCanceled ())
				ThrowInvalidException ();
		}
		
		public void SetException (Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException ("exception");
			
			SetException (new Exception[] { exception });
		}
		
		public void SetException (IEnumerable<Exception> exceptions)
		{
			if (!TrySetException (exceptions))
				ThrowInvalidException ();
		}
		
		public void SetResult (TResult result)
		{
			if (!TrySetResult (result))
				ThrowInvalidException ();
		}
				
		static void ThrowInvalidException ()
		{
			throw new InvalidOperationException ("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
		}
		
		public bool TrySetCanceled ()
		{
			return ApplyOperation (setCanceledAction, null);
		}
		
		public bool TrySetException (Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException ("exception");
			
			return TrySetException (new Exception[] { exception });
		}
		
		public bool TrySetException (IEnumerable<Exception> exceptions)
		{
			if (exceptions == null)
				throw new ArgumentNullException ("exceptions");

			var aggregate = new AggregateException (exceptions);
			if (aggregate.InnerExceptions.Count == 0)
				throw new ArgumentNullException ("exceptions");
			
			return ApplyOperation (setExceptionAction, aggregate);
		}
		
		public bool TrySetResult (TResult result)
		{
			return ApplyOperation (setResultAction, result);
		}
				
		bool ApplyOperation<TState> (Action<Task<TResult>, TState> action, TState state)
		{
			bool taken = false;
			try {
				opLock.Enter (ref taken);
				if (CheckInvalidState ())
					return false;
			
				source.Status = TaskStatus.Running;

				if (action != null)
					action (source, state);

				source.Finish ();
			
				return true;
			} finally {
				if (taken)
					opLock.Exit ();
			}
		}
		
		bool CheckInvalidState ()
		{
			return source.Status == TaskStatus.RanToCompletion ||
				   source.Status == TaskStatus.Faulted || 
				   source.Status == TaskStatus.Canceled;
		}

		static void SetResultAction (Task<TResult> source, TResult result)
		{
			source.Result = result;
		}

		static void SetExceptionAction (Task<TResult> source, AggregateException aggregate)
		{
			source.HandleGenericException (aggregate);
		}

		static void SetCanceledAction (Task<TResult> source, object unused)
		{
			source.CancelReal ();
		}

		public Task<TResult> Task {
			get {
				return source;
			}
		}
	}
}
#endif
