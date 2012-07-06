// TransformBlock.cs
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

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class TransformBlock<TInput, TOutput> :
		IPropagatorBlock<TInput, TOutput>, IReceivableSourceBlock<TOutput>
	{
		static readonly ExecutionDataflowBlockOptions defaultOptions = new ExecutionDataflowBlockOptions ();

		readonly ExecutionDataflowBlockOptions dataflowBlockOptions;
		readonly CompletionHelper compHelper;
		readonly BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		readonly MessageBox<TInput> messageBox;
		readonly MessageOutgoingQueue<TOutput> outgoing;
		readonly Func<TInput, TOutput> transformer;

		public TransformBlock (Func<TInput, TOutput> transformer) : this (transformer, defaultOptions)
		{

		}

		public TransformBlock (Func<TInput, TOutput> transformer, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.transformer = transformer;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			this.messageBox = new ExecutingMessageBox<TInput> (
				this, messageQueue, compHelper,
				() => outgoing.IsCompleted, TransformProcess, () => outgoing.Complete (),
				dataflowBlockOptions);
			this.outgoing = new MessageOutgoingQueue<TOutput> (this, compHelper,
				() => messageQueue.IsCompleted, messageBox.DecreaseCount,
				dataflowBlockOptions);
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget (target, linkOptions);
		}

		public TOutput ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<TOutput> filter, out TOutput item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<TOutput> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		bool TransformProcess ()
		{
			TInput input;

			var dequeued = messageQueue.TryTake (out input);
			if (dequeued)
				outgoing.AddData (transformer (input));

			return dequeued;
		}

		public void Complete ()
		{
			messageBox.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.RequestFault (ex);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		public int OutputCount {
			get {
				return outgoing.Count;
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