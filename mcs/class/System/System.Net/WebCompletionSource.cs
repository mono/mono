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
	class WebCompletionSource
	{
		TaskCompletionSource<Result> completion;

		public WebCompletionSource ()
		{
			completion = new TaskCompletionSource<Result> ();
		}

		public bool SetCompleted ()
		{
			var result = new Result (1, null);
			return completion.TrySetResult (new Result (1, null));
		}

		public bool SetCanceled ()
		{
			var error = new OperationCanceledException ();
			var result = new Result (2, ExceptionDispatchInfo.Capture (error));
			return completion.TrySetResult (result);
		}

		public bool SetException (Exception error)
		{
			var result = new Result (3, ExceptionDispatchInfo.Capture (error));
			return completion.TrySetResult (result);
		}

		public bool IsCompleted => completion.Task.IsCompleted;

		public void Throw ()
		{
			if (!completion.Task.IsCompleted)
				return;
			completion.Task.Result.Error?.Throw ();
		}

		public async Task<bool> WaitForCompletion (bool throwOnError)
		{
			var result = await completion.Task.ConfigureAwait (false);
			if (result.State == 1)
				return true;
			if (throwOnError)
				result.Error.Throw ();
			return false;
		}

		class Result
		{
			public int State {
				get;
			}

			public ExceptionDispatchInfo Error {
				get;
			}

			public Result (int state, ExceptionDispatchInfo error)
			{
				State = state;
				Error = error;
			}
		}
	}
}
