//
// TaskContinuation.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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
//
//

#if NET_4_0 || MOBILE

namespace System.Threading.Tasks
{
	interface IContinuation
	{
		void Execute ();
	}

	class TaskContinuation : IContinuation
	{
		readonly Task task;
		readonly TaskContinuationOptions continuationOptions;

		public TaskContinuation (Task task, TaskContinuationOptions continuationOptions)
		{
			this.task = task;
			this.continuationOptions = continuationOptions;
		}

		bool ContinuationStatusCheck (TaskContinuationOptions kind)
		{
			if (kind == TaskContinuationOptions.None)
				return true;

			int kindCode = (int) kind;
			var status = task.Parent.Status;

			if (kindCode >= ((int) TaskContinuationOptions.NotOnRanToCompletion)) {
				// Remove other options
				kind &= ~(TaskContinuationOptions.PreferFairness
						  | TaskContinuationOptions.LongRunning
						  | TaskContinuationOptions.AttachedToParent
						  | TaskContinuationOptions.ExecuteSynchronously);

				if (status == TaskStatus.Canceled) {
					if (kind == TaskContinuationOptions.NotOnCanceled)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnFaulted)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnRanToCompletion)
						return false;
				} else if (status == TaskStatus.Faulted) {
					if (kind == TaskContinuationOptions.NotOnFaulted)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnCanceled)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnRanToCompletion)
						return false;
				} else if (status == TaskStatus.RanToCompletion) {
					if (kind == TaskContinuationOptions.NotOnRanToCompletion)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnFaulted)
						return false;
					if (kind == TaskContinuationOptions.OnlyOnCanceled)
						return false;
				}
			}

			return true;
		}

		public void Execute ()
		{
			if (!ContinuationStatusCheck (continuationOptions)) {
				task.CancelReal ();
				task.Dispose ();
				return;
			}

			if ((continuationOptions & TaskContinuationOptions.ExecuteSynchronously) != 0)
				task.RunSynchronously (task.scheduler);
			else
				task.Schedule ();
		}
	}

	class ActionContinuation : IContinuation
	{
		readonly Action action;

		public ActionContinuation (Action action)
		{
			this.action = action;
		}

		public void Execute ()
		{
			action ();
		}
	}

	class SynchronizationContextContinuation : IContinuation
	{
		readonly Action action;
		readonly SynchronizationContext ctx;

		public SynchronizationContextContinuation (Action action, SynchronizationContext ctx)
		{
			this.action = action;
			this.ctx = ctx;
		}

		public void Execute ()
		{
			ctx.Post (l => ((Action) l) (), action);
		}
	}
}

#endif
