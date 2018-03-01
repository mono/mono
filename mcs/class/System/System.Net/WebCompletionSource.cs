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

		public WebCompletionSource ()
		{
			completion = new TaskCompletionSource<Result> ();
		}

		public bool TrySetCompleted (T argument)
		{
			return completion.TrySetResult (new Result (argument));
		}

		public bool TrySetCompleted ()
		{
			return completion.TrySetResult (new Result (State.Completed, null));
		}

		public bool TrySetCanceled ()
		{
			var error = new OperationCanceledException ();
			var result = new Result (State.Canceled, ExceptionDispatchInfo.Capture (error));
			return completion.TrySetResult (result);
		}

		public bool TrySetException (Exception error)
		{
			var result = new Result (State.Faulted, ExceptionDispatchInfo.Capture (error));
			return completion.TrySetResult (result);
		}

		public bool IsCompleted => completion.Task.IsCompleted;

		public void ThrowOnError ()
		{
			if (!completion.Task.IsCompleted)
				return;
			completion.Task.Result.Error?.Throw ();
		}

		public async Task<(bool success, object result)> WaitForCompletion (bool throwOnError)
		{
			var result = await completion.Task.ConfigureAwait (false);
			if (result.State == State.Completed)
				return (true, result.Argument);
			if (throwOnError)
				result.Error.Throw ();
			return (false, null);
		}

		public async Task<T> WaitForCompletion ()
		{
			var (result, argument) = await WaitForCompletion (true);
			return (T)argument;
		}

		enum State : int {
			Running,
			Completed,
			Canceled,
			Faulted
		}

		class Result
		{
			public State State {
				get;
			}

			public ExceptionDispatchInfo Error {
				get;
			}

			public T Argument {
				get;
			}

			public Result (T argument)
			{
				State = State.Completed;
				Argument = argument;
			}

			public Result (State state, ExceptionDispatchInfo error)
			{
				State = state;
				Error = error;
			}
		}
	}

	class WebCompletionSource : WebCompletionSource<object>
	{
	}
}
