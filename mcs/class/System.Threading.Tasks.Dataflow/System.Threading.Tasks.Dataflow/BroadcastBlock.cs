// BroadcastBlock.cs
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
	public sealed class BroadcastBlock<T> : IPropagatorBlock<T, T>, IReceivableSourceBlock<T> {
		readonly CompletionHelper compHelper;
		readonly BlockingCollection<T> messageQueue = new BlockingCollection<T> ();
		readonly MessageBox<T> messageBox;
		readonly DataflowBlockOptions dataflowBlockOptions;
		readonly Func<T, T> cloner;
		readonly BroadcastOutgoingQueue<T> outgoing;

		public BroadcastBlock (Func<T, T> cloner)
			: this (cloner, DataflowBlockOptions.Default)
		{
		}

		public BroadcastBlock (Func<T, T> cloner,
		                       DataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.cloner = cloner;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			this.messageBox = new PassingMessageBox<T> (this, messageQueue, compHelper,
				() => outgoing.IsCompleted, _ => BroadcastProcess (), dataflowBlockOptions);
			this.outgoing = new BroadcastOutgoingQueue<T> (this, compHelper,
				() => messageQueue.IsCompleted, messageBox.DecreaseCount,
				dataflowBlockOptions, cloner != null);
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source,
			bool consumeToAccept)
		{
			return messageBox.OfferMessage (messageHeader, messageValue, source,
				consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<T> target, DataflowLinkOptions linkOptions)
		{
			if (linkOptions == null)
				throw new ArgumentNullException("linkOptions");

			return outgoing.AddTarget (target, linkOptions);
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader,
		                         ITargetBlock<T> target, out bool messageConsumed)
		{
			T message = outgoing.ConsumeMessage (
				messageHeader, target, out messageConsumed);
			if (messageConsumed && cloner != null)
				message = cloner (message);
			return message;
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader,
		                            ITargetBlock<T> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader,
		                                ITargetBlock<T> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		public bool TryReceive (Predicate<T> filter, out T item)
		{
			var received = outgoing.TryReceive (filter, out item);
			if (received && cloner != null)
				item = cloner (item);
			return received;
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			T[] originalItems;
			bool received = outgoing.TryReceiveAll (out originalItems);
			if (received && cloner != null)
				items = Array.ConvertAll (originalItems, new Converter<T, T> (cloner));
			else
				items = originalItems;

			return received;
		}

		void BroadcastProcess ()
		{
			T item;
			while (messageQueue.TryTake (out item))
				outgoing.AddData (item);
		}

		public void Complete ()
		{
			messageBox.Complete ();
			outgoing.Complete ();
		}

		public void Fault (Exception exception)
		{
			compHelper.RequestFault (exception);
		}

		public Task Completion {
			get { return compHelper.Completion; }
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}