// AsyncExecutingMessageBox.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
// Copyright (c) 2012 Petr Onderka
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

using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow {
	class AsyncExecutingMessageBox<TInput, TTask>
		: ExecutingMessageBoxBase<TInput>
		where TTask : Task {
		public delegate bool AsyncProcessItem (out TTask task);

		readonly AsyncProcessItem processItem;
		readonly Action<TTask> processFinishedTask;

		public AsyncExecutingMessageBox (
			ITargetBlock<TInput> target, BlockingCollection<TInput> messageQueue,
			CompletionHelper compHelper, Func<bool> externalCompleteTester,
			AsyncProcessItem processItem, Action<TTask> processFinishedTask,
			Action outgoingQueueComplete, ExecutionDataflowBlockOptions options)
			: base (
				target, messageQueue, compHelper, externalCompleteTester,
				outgoingQueueComplete, options)
		{
			this.processItem = processItem;
			this.processFinishedTask = processFinishedTask;
		}

		protected override void ProcessQueue ()
		{
			StartProcessQueue ();

			ProcessQueueWithoutStart ();
		}

		void ProcessQueueWithoutStart ()
		{
			// catch is needed here, if the Task-returning delegate throws exception itself
			try {
				int i = 0;
				while (CanRun (i)) {
					TTask task;
					if (!processItem (out task))
						break;
					if (task == null || task.IsCanceled
					    || (task.IsCompleted && !task.IsFaulted)) {
						if (processFinishedTask != null)
							processFinishedTask (task);
					} else if (task.IsFaulted) {
						CompHelper.RequestFault (task.Exception, false);
						break;
					} else {
						task.ContinueWith (
							t => TaskFinished ((TTask)t), Options.TaskScheduler);
						return;
					}
					i++;
				}
			} catch (Exception e) {
				CompHelper.RequestFault (e, false);
			}

			FinishProcessQueue ();
		}

		void TaskFinished (TTask task)
		{
			if (task.IsFaulted) {
				CompHelper.RequestFault (task.Exception, false);
				FinishProcessQueue ();
				return;
			}

			if (processFinishedTask != null)
				processFinishedTask (task);

			ProcessQueueWithoutStart ();
		}
	}
}