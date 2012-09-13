// ActionBlock.cs
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
	public sealed class ActionBlock<TInput> : ITargetBlock<TInput> {
		readonly CompletionHelper compHelper;
		readonly BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		readonly ExecutingMessageBoxBase<TInput> messageBox;
		readonly Action<TInput> action;
		readonly Func<TInput, Task> asyncAction;
		readonly ExecutionDataflowBlockOptions dataflowBlockOptions;

		ActionBlock (ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = new CompletionHelper (dataflowBlockOptions);
		}

		public ActionBlock (Action<TInput> action)
			: this (action, ExecutionDataflowBlockOptions.Default)
		{
		}

		public ActionBlock (Action<TInput> action,
		                    ExecutionDataflowBlockOptions dataflowBlockOptions)
			: this (dataflowBlockOptions)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			this.action = action;
			this.messageBox = new ExecutingMessageBox<TInput> (this, messageQueue, compHelper,
				() => true, ProcessItem, () => { }, dataflowBlockOptions);
		}

		public ActionBlock (Func<TInput, Task> action)
			: this (action, ExecutionDataflowBlockOptions.Default)
		{
		}

		public ActionBlock (Func<TInput, Task> action,
		                    ExecutionDataflowBlockOptions dataflowBlockOptions)
			: this (dataflowBlockOptions)
		{
			if (action == null)
				throw new ArgumentNullException ("action");

			this.asyncAction = action;
			this.messageBox = new AsyncExecutingMessageBox<TInput, Task> (
				this, messageQueue, compHelper, () => true, AsyncProcessItem, null,
				() => { }, dataflowBlockOptions);
		}

		DataflowMessageStatus ITargetBlock<TInput>.OfferMessage (
			DataflowMessageHeader messageHeader, TInput messageValue,
			ISourceBlock<TInput> source, bool consumeToAccept)
		{
			return messageBox.OfferMessage (
				messageHeader, messageValue, source, consumeToAccept);
		}

		public bool Post (TInput item)
		{
			return messageBox.OfferMessage (
				new DataflowMessageHeader (1), item, null, false)
			       == DataflowMessageStatus.Accepted;
		}

		/// <summary>
		/// Processes one item from the queue if the action is synchronous.
		/// </summary>
		/// <returns>Returns whether an item was processed. Returns <c>false</c> if the queue is empty.</returns>
		bool ProcessItem ()
		{
			TInput data;
			bool dequeued = messageQueue.TryTake (out data);
			if (dequeued)
				action (data);
			return dequeued;
		}

		/// <summary>
		/// Processes one item from the queue if the action is asynchronous.
		/// </summary>
		/// <param name="task">The Task that was returned by the synchronous part of the action.</param>
		/// <returns>Returns whether an item was processed. Returns <c>false</c> if the queue was empty.</returns>
		bool AsyncProcessItem(out Task task)
		{
			TInput data;
			bool dequeued = messageQueue.TryTake (out data);
			if (dequeued)
				task = asyncAction (data);
			else
				task = null;
			return dequeued;
		}

		public void Complete ()
		{
			messageBox.Complete ();
		}

		void IDataflowBlock.Fault (Exception exception)
		{
			compHelper.RequestFault (exception);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		public int InputCount {
			get {
				return messageQueue.Count;
			}
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}