//
// WebCompletionSource.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace System.Net
{
	class WebCompletionSource<T>
	{
		TaskCompletionSource<Result> completion;
		Result currentResult;

		public WebCompletionSource (bool runAsync = true)
		{
			completion = new TaskCompletionSource<Result> (
				runAsync ?
				TaskCreationOptions.RunContinuationsAsynchronously :
				TaskCreationOptions.None);
		}

		/*
		 * Provide a non-blocking way of getting the current status.
		 * 
		 * We are using `TaskCreationOptions.RunContinuationsAsynchronously`
		 * to prevent any user continuations from being run during the
		 * `TrySet*()` methods - to make these safe to be called while holding
		 * internal locks.
		 * 
		 */
		internal Result CurrentResult => currentResult;

		internal Status CurrentStatus => currentResult?.Status ?? Status.Running;

		internal Task Task => completion.Task;

		public bool TrySetCompleted (T argument)
		{
			var result = new Result (argument);
			if (Interlocked.CompareExchange (ref currentResult, result, null) != null)
				return false;
			return completion.TrySetResult (result);
		}

		public bool TrySetCompleted ()
		{
			var result = new Result (Status.Completed, null);
			if (Interlocked.CompareExchange (ref currentResult, result, null) != null)
				return false;
			return completion.TrySetResult (result);
		}

		public bool TrySetCanceled ()
		{
			return TrySetCanceled (new OperationCanceledException ());
		}

		public bool TrySetCanceled (OperationCanceledException error)
		{
			var result = new Result (Status.Canceled, ExceptionDispatchInfo.Capture (error));
			if (Interlocked.CompareExchange (ref currentResult, result, null) != null)
				return false;
			return completion.TrySetResult (result);
		}

		public bool TrySetException (Exception error)
		{
			var result = new Result (Status.Faulted, ExceptionDispatchInfo.Capture (error));
			if (Interlocked.CompareExchange (ref currentResult, result, null) != null)
				return false;
			return completion.TrySetResult (result);
		}

		public void ThrowOnError ()
		{
			if (!completion.Task.IsCompleted)
				return;
			completion.Task.Result.Error?.Throw ();
		}

		public async Task<T> WaitForCompletion ()
		{
			var result = await completion.Task.ConfigureAwait (false);
			if (result.Status == Status.Completed)
				return (T)result.Argument;
			// This will always throw once we get here.
			result.Error.Throw ();
			throw new InvalidOperationException ("Should never happen.");
		}

		internal enum Status : int {
			Running,
			Completed,
			Canceled,
			Faulted
		}

		internal class Result
		{
			public Status Status {
				get;
			}

			public bool Success => Status == Status.Completed;

			public ExceptionDispatchInfo Error {
				get;
			}

			public T Argument {
				get;
			}

			public Result (T argument)
			{
				Status = Status.Completed;
				Argument = argument;
			}

			public Result (Status state, ExceptionDispatchInfo error)
			{
				Status = state;
				Error = error;
			}
		}
	}

	class WebCompletionSource : WebCompletionSource<object>
	{
	}
}
