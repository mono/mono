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
		readonly Func<bool> processItem;
		readonly Action outgoingQueueComplete;
		readonly CompletionHelper compHelper;

		// even number: Task is waiting to run
		// odd number: Task is not waiting to run
		// invariant: dop / 2 Tasks are running or waiting
		int degreeOfParallelism = 1;

		public ExecutingMessageBox (
			BlockingCollection<TInput> messageQueue, CompletionHelper compHelper,
			Func<bool> externalCompleteTester, Func<bool> processItem, Action outgoingQueueComplete,
			ExecutionDataflowBlockOptions options)
			: base (messageQueue, compHelper, externalCompleteTester)
		{
			this.options = options;
			this.processItem = processItem;
			this.outgoingQueueComplete = outgoingQueueComplete;
			this.compHelper = compHelper;
		}

		protected override void EnsureProcessing ()
		{
			StartProcessing ();
		}

		void StartProcessing ()
		{
			// atomically increase degreeOfParallelism by 1 only if it's odd
			// and low enough
			int startDegreeOfParallelism;
			int currentDegreeOfParallelism = degreeOfParallelism;
			do {
				startDegreeOfParallelism = currentDegreeOfParallelism;
				if (startDegreeOfParallelism % 2 == 0
				    || (options.MaxDegreeOfParallelism != DataflowBlockOptions.Unbounded
				        && startDegreeOfParallelism / 2 >= options.MaxDegreeOfParallelism))
					return;
				currentDegreeOfParallelism =
					Interlocked.CompareExchange (ref degreeOfParallelism,
						startDegreeOfParallelism + 1, startDegreeOfParallelism);
			} while (startDegreeOfParallelism != currentDegreeOfParallelism);

			Task.Factory.StartNew (ProcessQueue, options.CancellationToken,
				TaskCreationOptions.PreferFairness, options.TaskScheduler);
		}

		void ProcessQueue ()
		{
			compHelper.CanFaultOrCancelImmediatelly = false;

			int incrementedDegreeOfParallelism =
				Interlocked.Increment (ref degreeOfParallelism);
			if ((options.MaxDegreeOfParallelism == DataflowBlockOptions.Unbounded
			     || incrementedDegreeOfParallelism / 2 < options.MaxDegreeOfParallelism)
			    && MessageQueue.Count > 0 && compHelper.CanRun)
				StartProcessing ();

			try {
				int i = 0;
				while (compHelper.CanRun
				       && (options.MaxMessagesPerTask == DataflowBlockOptions.Unbounded
				           || i++ < options.MaxMessagesPerTask)) {
					if (!processItem ())
						break;
				}
			} catch (Exception e) {
				compHelper.RequestFault (e);
			}

			int decrementedDegreeOfParallelism =
				Interlocked.Add (ref degreeOfParallelism, -2);

			if (decrementedDegreeOfParallelism % 2 == 1) {
				if (decrementedDegreeOfParallelism == 1) {
					compHelper.CanFaultOrCancelImmediatelly = true;
					base.VerifyCompleteness ();
					if (MessageQueue.IsCompleted)
						outgoingQueueComplete ();
				}
				if (MessageQueue.Count > 0)
					EnsureProcessing ();
			}
		}

		protected override void OutgoingQueueComplete ()
		{
			if (MessageQueue.IsCompleted
			    && Thread.VolatileRead (ref degreeOfParallelism) == 1)
				outgoingQueueComplete ();
		}

		protected override void VerifyCompleteness ()
		{
			if (Thread.VolatileRead (ref degreeOfParallelism) == 1)
				base.VerifyCompleteness ();
		}
	}
}