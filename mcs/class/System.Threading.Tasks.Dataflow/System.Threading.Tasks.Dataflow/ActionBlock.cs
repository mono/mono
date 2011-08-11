// ActionBlock.cs
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
//
//


using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class ActionBlock<TInput> : ITargetBlock<TInput>, IDataflowBlock
	{
		static readonly ExecutionDataflowBlockOptions defaultOptions = new ExecutionDataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		ExecutingMessageBox<TInput> messageBox;
		Action<TInput> action;
		ExecutionDataflowBlockOptions dataflowBlockOptions;


		public ActionBlock (Action<TInput> action) : this (action, defaultOptions)
		{
			
		}

		public ActionBlock (Action<TInput> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.action = action;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.messageBox = new ExecutingMessageBox<TInput> (messageQueue, compHelper, () => true, ProcessQueue, dataflowBlockOptions);
		}

		[MonoTODO]
		public ActionBlock (Func<TInput, Task> action) : this (action, defaultOptions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ActionBlock (Func<TInput, Task> action, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			throw new NotImplementedException ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (this, messageHeader, messageValue, source, consumeToAccept);
		}

		void ProcessQueue ()
		{
			TInput data;
			while (messageQueue.TryTake (out data))
				action (data);
		}

		public void Complete ()
		{
			messageBox.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.Fault (ex);
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
	}
}

