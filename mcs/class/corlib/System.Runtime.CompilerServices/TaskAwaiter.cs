//
// TaskAwaiter.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_5

using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace System.Runtime.CompilerServices
{
	public struct TaskAwaiter : ICriticalNotifyCompletion
	{
		readonly Task task;

		internal TaskAwaiter (Task task)
		{
			this.task = task;
		}

		public bool IsCompleted {
			get {
				return task.IsCompleted;
			}
		}

		public void GetResult ()
		{
			if (!task.IsCompleted)
				task.WaitCore (Timeout.Infinite, CancellationToken.None);

			if (task.Status != TaskStatus.RanToCompletion)
				// Merge current and dispatched stack traces if there is any
				ExceptionDispatchInfo.Capture (HandleUnexpectedTaskResult (task)).Throw ();
		}

		internal static Exception HandleUnexpectedTaskResult (Task task)
		{
			switch (task.Status) {
			case TaskStatus.Canceled:
				return new TaskCanceledException (task);
			case TaskStatus.Faulted:
				return task.Exception.InnerException;
			default:
				throw new ArgumentException ("Should never be reached");
			}
		}

		internal static void HandleOnCompleted (Task task, Action continuation, bool continueOnSourceContext, bool manageContext)
		{
			if (continueOnSourceContext && SynchronizationContext.Current != null) {
				task.ContinueWith (new SynchronizationContextContinuation (continuation, SynchronizationContext.Current));
			} else {
				task.ContinueWith (new ActionContinuation (continuation));
			}
		}

		public void OnCompleted (Action continuation)
		{
			if (continuation == null)
				throw new ArgumentNullException ("continuation");

			HandleOnCompleted (task, continuation, true, true);
		}
		
		public void UnsafeOnCompleted (Action continuation)
		{
			if (continuation == null)
				throw new ArgumentNullException ("continuation");

			HandleOnCompleted (task, continuation, true, false);
		}
	}
}

#endif