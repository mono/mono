// TransformManyBlock.cs
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

#if NET_4_0 || MOBILE

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class TransformManyBlock<TInput, TOutput> :
		IPropagatorBlock<TInput, TOutput>, ITargetBlock<TInput>, IDataflowBlock, ISourceBlock<TOutput>, IReceivableSourceBlock<TOutput>
	{
		static readonly ExecutionDataflowBlockOptions defaultOptions = new ExecutionDataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		BlockingCollection<TInput> messageQueue = new BlockingCollection<TInput> ();
		MessageBox<TInput> messageBox;
		MessageVault<TOutput> vault;
		ExecutionDataflowBlockOptions dataflowBlockOptions;
		readonly Func<TInput, IEnumerable<TOutput>> transformer;
		MessageOutgoingQueue<TOutput> outgoing;
		TargetBuffer<TOutput> targets = new TargetBuffer<TOutput> ();
		DataflowMessageHeader headers = DataflowMessageHeader.NewValid ();

		public TransformManyBlock (Func<TInput, IEnumerable<TOutput>> transformer) : this (transformer, defaultOptions)
		{

		}

		public TransformManyBlock (Func<TInput, IEnumerable<TOutput>> transformer, ExecutionDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.transformer = transformer;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.messageBox = new ExecutingMessageBox<TInput> (messageQueue,
			                                                   compHelper,
			                                                   () => outgoing.IsCompleted,
			                                                   TransformProcess,
			                                                   dataflowBlockOptions);
			this.outgoing = new MessageOutgoingQueue<TOutput> (compHelper, () => messageQueue.IsCompleted);
			this.vault = new MessageVault<TOutput> ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (this, messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<TOutput> target, bool unlinkAfterOne)
		{
			var result = targets.AddTarget (target, unlinkAfterOne);
			outgoing.ProcessForTarget (target, this, false, ref headers);
			return result;
		}

		public TOutput ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
		{
			return vault.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<TOutput> filter, out TOutput item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<TOutput> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		void TransformProcess ()
		{
			ITargetBlock<TOutput> target;
			TInput input;

			while (messageQueue.TryTake (out input)) {
				foreach (var item in transformer (input)) {
					if ((target = targets.Current) != null)
						target.OfferMessage (headers.Increment (), item, this, false);
					else
						outgoing.AddData (item);
				}
			}

			if (!outgoing.IsEmpty && (target = targets.Current) != null)
				outgoing.ProcessForTarget (target, this, false, ref headers);
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
	}
}

#endif
