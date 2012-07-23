// TransformManyBlock.cs
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

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow {
	public sealed class TransformManyBlock<TInput, TOutput> :
		IPropagatorBlock<TInput, TOutput>, IReceivableSourceBlock<TOutput> {
		readonly CompletionHelper compHelper;
		readonly BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		readonly MessageBox<TInput> messageBox;
		readonly ExecutionDataflowBlockOptions dataflowBlockOptions;
		readonly Func<TInput, IEnumerable<TOutput>> transform;
		readonly OutgoingQueue<TOutput> outgoing;

		public TransformManyBlock (Func<TInput, IEnumerable<TOutput>> transform)
			: this (transform, ExecutionDataflowBlockOptions.Default)
		{
		}

		public TransformManyBlock (Func<TInput, IEnumerable<TOutput>> transform,
		                           ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			if (transform == null)
				throw new ArgumentNullException ("transform");
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.transform = transform;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			this.messageBox = new ExecutingMessageBox<TInput> (this, messageQueue, compHelper,
				() => outgoing.IsCompleted, TransformProcess, () => outgoing.Complete (),
				dataflowBlockOptions);
			this.outgoing = new OutgoingQueue<TOutput> (this, compHelper,
				() => messageQueue.IsCompleted, messageBox.DecreaseCount,
				dataflowBlockOptions);
		}

		DataflowMessageStatus ITargetBlock<TInput>.OfferMessage (
			DataflowMessageHeader messageHeader, TInput messageValue,
			ISourceBlock<TInput> source, bool consumeToAccept)
		{
			return messageBox.OfferMessage (messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget (target, linkOptions);
		}

		TOutput ISourceBlock<TOutput>.ConsumeMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target,
			out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		void ISourceBlock<TOutput>.ReleaseReservation (
			DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		bool ISourceBlock<TOutput>.ReserveMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
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
			if (dequeued) {
				var result = transform (input);

				bool first = true;
				if (result != null) {
					foreach (var item in result) {
						if (first)
							first = false;
						else
							messageBox.IncreaseCount ();
						outgoing.AddData (item);
					}
				}
				if (first)
					messageBox.DecreaseCount ();
			}

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