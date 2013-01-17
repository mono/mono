// BufferBlock.cs
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

namespace System.Threading.Tasks.Dataflow {
	public sealed class BufferBlock<T> : IPropagatorBlock<T, T>, IReceivableSourceBlock<T> {
		readonly DataflowBlockOptions dataflowBlockOptions;
		readonly CompletionHelper compHelper;
		readonly MessageBox<T> messageBox;
		readonly OutgoingQueue<T> outgoing;
		readonly BlockingCollection<T> messageQueue = new BlockingCollection<T> ();

		public BufferBlock () : this (DataflowBlockOptions.Default)
		{
		}

		public BufferBlock (DataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			this.messageBox = new PassingMessageBox<T> (this, messageQueue, compHelper,
				() => outgoing.IsCompleted, _ => ProcessQueue (), dataflowBlockOptions);
			this.outgoing = new OutgoingQueue<T> (this, compHelper,
				() => messageQueue.IsCompleted, messageBox.DecreaseCount,
				dataflowBlockOptions);
		}

		DataflowMessageStatus ITargetBlock<T>.OfferMessage (
			DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source,
			bool consumeToAccept)
		{
			return messageBox.OfferMessage (messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<T> target, DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget (target, linkOptions);
		}

		T ISourceBlock<T>.ConsumeMessage (DataflowMessageHeader messageHeader,
		                                  ITargetBlock<T> target,
		                                  out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		bool ISourceBlock<T>.ReserveMessage (DataflowMessageHeader messageHeader,
		                                     ITargetBlock<T> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		void ISourceBlock<T>.ReleaseReservation (DataflowMessageHeader messageHeader,
		                                         ITargetBlock<T> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		public bool TryReceive (Predicate<T> filter, out T item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		/// <summary>
		/// Moves items from the input queue to the output queue.
		/// </summary>
		void ProcessQueue ()
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

		void IDataflowBlock.Fault (Exception exception)
		{
			compHelper.RequestFault (exception);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		public int Count {
			get {
				return outgoing.Count;
			}
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}

