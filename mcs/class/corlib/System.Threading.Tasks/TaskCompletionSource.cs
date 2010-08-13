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

#if NET_4_0
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
		}
		
		public TaskCompletionSource (object state)
		{
			source = new Task<TResult> (null, state);
		}
		
		public TaskCompletionSource (TaskCreationOptions options)
		{
			source = new Task<TResult> (null, options);
		}
		
		public TaskCompletionSource (object state, TaskCreationOptions options)
		{
			source = new Task<TResult> (null, state, options);
		}
		
		public void SetCanceled ()
		{
			if (!ApplyOperation (TaskStatus.Canceled, source.CancelReal))
				ThrowInvalidException ();
		}
		
		public void SetException (Exception e)
		{
			SetException (new Exception[] { e });
		}
		
		public void SetException (IEnumerable<Exception> e)
		{
			if (!ApplyOperation (TaskStatus.Faulted, () => source.Exception = new AggregateException (e)))
				ThrowInvalidException ();
		}
		
		public void SetResult (TResult result)
		{
			if (!ApplyOperation (TaskStatus.RanToCompletion, () => source.Result = result))
				ThrowInvalidException ();
		}
				
		void ThrowInvalidException ()
		{
			throw new InvalidOperationException ("The underlying Task is already in one of the three final states: RanToCompletion, Faulted, or Canceled.");
		}
		
		public bool TrySetCanceled ()
		{
			return ApplyOperation (TaskStatus.Canceled, source.CancelReal);
		}
		
		public bool TrySetException (Exception e)
		{
			return TrySetException (new Exception[] { e });
		}
		
		
		public bool TrySetException (IEnumerable<Exception> e)
		{
			return ApplyOperation (TaskStatus.Faulted, () => source.Exception = new AggregateException (e));
		}
		
		public bool TrySetResult (TResult result)
		{
			return ApplyOperation (TaskStatus.RanToCompletion, () => source.Result = result);
		}
				
		bool ApplyOperation (TaskStatus newStatus, Action action)
		{
			if (CheckInvalidState ())
				return false;
			
			if (action != null)
				action ();
			source.Status = newStatus;
			
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
