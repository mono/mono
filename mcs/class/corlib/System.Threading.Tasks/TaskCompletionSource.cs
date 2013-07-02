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

#if NET_4_0
using System;
using System.Collections.Generic;

namespace System.Threading.Tasks
{
	public class TaskCompletionSource<TResult>
	{
		readonly Task<TResult> source;

		public TaskCompletionSource ()
			: this (null, TaskCreationOptions.None)
		{
		}
		
		public TaskCompletionSource (object state)
			: this (state, TaskCreationOptions.None)
		{
		}
		
		public TaskCompletionSource (TaskCreationOptions creationOptions)
			: this (null, creationOptions)
		{
		}
		
		public TaskCompletionSource (object state, TaskCreationOptions creationOptions)
		{
			if ((creationOptions & System.Threading.Tasks.Task.WorkerTaskNotSupportedOptions) != 0)
				throw new ArgumentOutOfRangeException ("creationOptions");

			source = new Task<TResult> (TaskActionInvoker.Empty, state, CancellationToken.None, creationOptions, null);
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
			return source.TrySetCanceled ();
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

			return source.TrySetException (aggregate);
		}
		
		public bool TrySetResult (TResult result)
		{
			return source.TrySetResult (result);
		}

		public Task<TResult> Task {
			get {
				return source;
			}
		}
	}
}
#endif
