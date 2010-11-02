// 
// TaskCompletionSource.cs
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
using System.Collections.Generic;

namespace System.Threading.Tasks
{
	public class TaskCompletionSource<TResult>
	{
		Task<TResult> source;

		public TaskCompletionSource ()
		{
			source = new Task<TResult> (null);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public TaskCompletionSource (object state)
		{
			source = new Task<TResult> (null, state);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public TaskCompletionSource (TaskCreationOptions options)
		{
			source = new Task<TResult> (null, options);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public TaskCompletionSource (object state, TaskCreationOptions options)
		{
			source = new Task<TResult> (null, state, options);
			source.SetupScheduler (TaskScheduler.Current);
		}
		
		public void SetCanceled ()
		{
			if (!ApplyOperation (source.CancelReal))
				ThrowInvalidException ();
		}
		
		public void SetException (Exception e)
		{
			SetException (new Exception[] { e });
		}
		
		public void SetException (IEnumerable<Exception> e)
		{
			if (!ApplyOperation (() => source.HandleGenericException (new AggregateException (e))))
				ThrowInvalidException ();
		}
		
		public void SetResult (TResult result)
		{
			if (!ApplyOperation (() => source.Result = result))
				ThrowInvalidException ();
		}
				
		void ThrowInvalidException ()
		{
			throw new InvalidOperationException ("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
		}
		
		public bool TrySetCanceled ()
		{
			return ApplyOperation (source.CancelReal);
		}
		
		public bool TrySetException (Exception e)
		{
			return TrySetException (new Exception[] { e });
		}
		
		public bool TrySetException (IEnumerable<Exception> e)
		{
			return ApplyOperation (() => source.HandleGenericException (new AggregateException (e)));
		}
		
		public bool TrySetResult (TResult result)
		{
			return ApplyOperation (() => source.Result = result);
		}
				
		bool ApplyOperation (Action action)
		{
			if (CheckInvalidState ())
				return false;
			
			source.Status = TaskStatus.Running;

			if (action != null)
				action ();

			source.Finish ();
			
			return true;
		}
		
		bool CheckInvalidState ()
		{
			return source.Status == TaskStatus.RanToCompletion ||
				   source.Status == TaskStatus.Faulted || 
				   source.Status == TaskStatus.Canceled;
					
		}
		
		public Task<TResult> Task {
			get {
				return source;
			}
		}
	}
}
#endif
