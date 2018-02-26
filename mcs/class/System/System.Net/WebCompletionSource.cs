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
using System.Runtime.ExceptionServices;

namespace System.Net
{
	class WebCompletionSource
	{
		volatile Result state;

		public bool SetCompleted ()
		{
			var result = new Result (1, null);
			return Interlocked.CompareExchange (ref state, result, null) == null;
		}

		public bool SetCanceled ()
		{
			var error = new OperationCanceledException ();
			var result = new Result (2, ExceptionDispatchInfo.Capture (error));
			return Interlocked.CompareExchange (ref state, result, null) == null;
		}

		public bool SetException (Exception error)
		{
			var result = new Result (3, ExceptionDispatchInfo.Capture (error));
			return Interlocked.CompareExchange (ref state, result, null) == null;
		}

		public (int state, ExceptionDispatchInfo error) CurrentState {
			get {
				var result = state;
				return result != null ? (result.State, result.Error) : (0, null);
			}
		}

		public bool IsCompleted => CurrentState.state == 1;

		public bool IsCanceled => CurrentState.state == 2;

		public bool IsFaulted => CurrentState.state == 3;

		public void Throw ()
		{
			state?.Error?.Throw ();
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
