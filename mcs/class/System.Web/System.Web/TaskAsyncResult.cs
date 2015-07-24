//
// System.Web.TaskAsyncResult.cs
//
// Author:
//   Kornel Pal (kornelpal@gmail.com)
//
// Copyright (C) 2014 Kornel Pal
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Threading;
using System.Threading.Tasks;

namespace System.Web
{
	sealed class TaskAsyncResult : IAsyncResult
	{
		static readonly Action<Task, object> invokeCallback = InvokeCallback;
		readonly Task task;
		readonly AsyncCallback callback;

		public object AsyncState {
			get;
			private set;
		}

		public WaitHandle AsyncWaitHandle {
			get { return ((IAsyncResult) task).AsyncWaitHandle; }
		}

		public bool CompletedSynchronously {
			get;
			private set;
		}

		public bool IsCompleted {
			get { return task.IsCompleted; }
		}

		TaskAsyncResult (Task task, AsyncCallback callback, object state)
		{
			this.task = task;
			this.callback = callback;
			this.AsyncState = state;
			this.CompletedSynchronously = task.IsCompleted;
		}

		public static IAsyncResult GetAsyncResult (Task task, AsyncCallback callback, object state)
		{
			if (task == null)
				return null;

			var result = new TaskAsyncResult (task, callback, state);

			if (callback != null) {
				if (result.CompletedSynchronously)
					callback (result);
				else
					task.ContinueWith (invokeCallback, result);
			}

			return result;
		}

		public static void Wait (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");

			var taskAsyncResult = result as TaskAsyncResult;
			if (taskAsyncResult == null)
				throw new ArgumentException ("The provided IAsyncResult is invalid.", "result");

			taskAsyncResult.task.GetAwaiter ().GetResult ();
		}

		static void InvokeCallback (Task task, object state)
		{
			var result = (TaskAsyncResult) state;
			result.callback (result);
		}
	}
}
