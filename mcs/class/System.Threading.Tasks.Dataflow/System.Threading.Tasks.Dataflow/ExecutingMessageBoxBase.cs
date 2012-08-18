// ExecutingMessageBoxBase.cs
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
	/// <summary>
	/// Base message box for execution blocks (synchronous and asynchrnous).
	/// </summary>
	/// <typeparam name="TInput">Type of the item the block is processing.</typeparam>
	abstract class ExecutingMessageBoxBase<TInput> : MessageBox<TInput> {
		protected ExecutionDataflowBlockOptions Options { get; private set; }
		readonly Action outgoingQueueComplete;

		// even number: Task is waiting to run
		// odd number: Task is not waiting to run
		// invariant: dop / 2 Tasks are running or waiting
		int degreeOfParallelism = 1;

		protected ExecutingMessageBoxBase (
			ITargetBlock<TInput> target, BlockingCollection<TInput> messageQueue,
			CompletionHelper compHelper, Func<bool> externalCompleteTester,
			Action outgoingQueueComplete, ExecutionDataflowBlockOptions options)
			: base (
				target, messageQueue, compHelper, externalCompleteTester,
				options)
		{
			this.Options = options;
			this.outgoingQueueComplete = outgoingQueueComplete;
		}

		/// <summary>
		/// Makes sure the input queue is processed the way it needs to.
		/// </summary>
		/// <param name="newItem">Was new item just added?</param>
		protected override void EnsureProcessing (bool newItem)
		{
			StartProcessing ();
		}

		/// <summary>
		/// Starts processing queue on a task,
		/// assuming <see cref="ExecutionDataflowBlockOptions.MaxDegreeOfParallelism"/>
		/// was't reached yet.
		/// </summary>
		void StartProcessing ()
		{
			// atomically increase degreeOfParallelism by 1 only if it's odd
			// and low enough
			int startDegreeOfParallelism;
			int currentDegreeOfParallelism = degreeOfParallelism;
			do {
				startDegreeOfParallelism = currentDegreeOfParallelism;
				if (startDegreeOfParallelism % 2 == 0
				    || (Options.MaxDegreeOfParallelism != DataflowBlockOptions.Unbounded
				        && startDegreeOfParallelism / 2 >= Options.MaxDegreeOfParallelism))
					return;
				currentDegreeOfParallelism =
					Interlocked.CompareExchange (ref degreeOfParallelism,
						startDegreeOfParallelism + 1, startDegreeOfParallelism);
			} while (startDegreeOfParallelism != currentDegreeOfParallelism);

			Task.Factory.StartNew (ProcessQueue, CancellationToken.None,
				TaskCreationOptions.PreferFairness, Options.TaskScheduler);
		}

		/// <summary>
		/// Processes the input queue of the block.
		/// </summary>
		/// <remarks>
		/// Should first call <see cref="StartProcessQueue"/>,
		/// then process the queue and finally call <see cref="FinishProcessQueue"/>.
		/// </remarks>
		protected abstract void ProcessQueue ();

		/// <summary>
		/// Notifies that another processing task was started.
		/// Should be called right after <see cref="ProcessQueue"/> is actually executed.
		/// </summary>
		protected void StartProcessQueue ()
		{
			CompHelper.CanFaultOrCancelImmediatelly = false;

			int incrementedDegreeOfParallelism =
				Interlocked.Increment (ref degreeOfParallelism);
			if ((Options.MaxDegreeOfParallelism == DataflowBlockOptions.Unbounded
			     || incrementedDegreeOfParallelism / 2 < Options.MaxDegreeOfParallelism)
			    && MessageQueue.Count > 1 && CompHelper.CanRun)
				StartProcessing ();
		}

		/// <summary>
		/// Notifies that a processing task was finished.
		/// Should be called after <see cref="ProcessQueue"/> actually finishes processing.
		/// </summary>
		protected void FinishProcessQueue ()
		{
			int decrementedDegreeOfParallelism =
				Interlocked.Add (ref degreeOfParallelism, -2);

			if (decrementedDegreeOfParallelism % 2 == 1) {
				if (decrementedDegreeOfParallelism == 1) {
					CompHelper.CanFaultOrCancelImmediatelly = true;
					base.VerifyCompleteness ();
					if (MessageQueue.IsCompleted)
						outgoingQueueComplete ();
				}
				if (MessageQueue.Count > 0 && CompHelper.CanRun)
					StartProcessing ();
			}
		}

		/// <summary>
		/// Notifies that outgoing queue should be completed, if possible.
		/// </summary>
		protected override void OutgoingQueueComplete ()
		{
			if (MessageQueue.IsCompleted
			    && Thread.VolatileRead (ref degreeOfParallelism) == 1)
				outgoingQueueComplete ();
		}

		/// <summary>
		/// Makes sure the block is completed if it should be.
		/// </summary>
		protected override void VerifyCompleteness ()
		{
			if (Thread.VolatileRead (ref degreeOfParallelism) == 1)
				base.VerifyCompleteness ();
		}

		/// <summary>
		/// Indicates whether a processing task can continue executing.
		/// </summary>
		/// <param name="iteration">The number of the iteration of the task, starting from 0.</param>
		protected bool CanRun (int iteration)
		{
			return CompHelper.CanRun
			       && (Options.MaxMessagesPerTask == DataflowBlockOptions.Unbounded
			           || iteration < Options.MaxMessagesPerTask);
		}
	}
}