// ExecutingMessageBox.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
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

namespace System.Threading.Tasks.Dataflow
{
	internal class ExecutingMessageBox<TInput> : MessageBox<TInput>
	{
		readonly ExecutionDataflowBlockOptions options;
		readonly Action<int> processQueue;
		CompletionHelper compHelper;

		readonly AtomicBoolean waitingTask = new AtomicBoolean ();
		int degreeOfParallelism;

		public ExecutingMessageBox (
			BlockingCollection<TInput> messageQueue, CompletionHelper compHelper,
			Func<bool> externalCompleteTester, Action<int> processQueue,
			ExecutionDataflowBlockOptions options)
			: base (messageQueue, compHelper, externalCompleteTester)
		{
			this.options = options;
			this.processQueue = processQueue;
			this.compHelper = compHelper;
		}

		protected override void EnsureProcessing ()
		{
			if ((options.MaxDegreeOfParallelism != DataflowBlockOptions.Unbounded
			     && Thread.VolatileRead (ref degreeOfParallelism) >= options.MaxDegreeOfParallelism) ||
			    !waitingTask.TrySet ())
				return;

			StartProcessing ();
		}

		void StartProcessing ()
		{
			Task.Factory.StartNew (ProcessQueue, TaskCreationOptions.PreferFairness);
		}

		void ProcessQueue ()
		{
			int incrementedDegreeOfParallelism =
				Interlocked.Increment (ref degreeOfParallelism);
			if ((options.MaxDegreeOfParallelism == DataflowBlockOptions.Unbounded
			     || incrementedDegreeOfParallelism < options.MaxDegreeOfParallelism)
			    && (MessageQueue.Count > 0))
				StartProcessing();
			else
				waitingTask.Value = false;

			processQueue (options.MaxMessagesPerTask);

			int decrementedDegreeOfParallelism =
				Interlocked.Decrement (ref degreeOfParallelism);

			if (!waitingTask.Value) {
				if (decrementedDegreeOfParallelism == 0 && MessageQueue.IsCompleted)
					compHelper.Complete ();
				else if (MessageQueue.Count > 0)
					EnsureProcessing ();
			}
		}
	}
}