// WriteOnceBlock.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class WriteOnceBlock<T> : IPropagatorBlock<T, T>, ITargetBlock<T>, IDataflowBlock, ISourceBlock<T>, IReceivableSourceBlock<T>
	{
		static readonly DataflowBlockOptions defaultOptions = new DataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		BlockingCollection<T> messageQueue = new BlockingCollection<T> ();
		MessageBox<T> messageBox;
		MessageVault<T> vault;
		DataflowBlockOptions dataflowBlockOptions;
		readonly Func<T, T> cloner;
		TargetBuffer<T> targets = new TargetBuffer<T> ();
		DataflowMessageHeader headers = DataflowMessageHeader.NewValid ();

		AtomicBooleanValue written;
		bool ready;
		T finalValue;

		public WriteOnceBlock (Func<T, T> cloner) : this (cloner, defaultOptions)
		{

		}

		public WriteOnceBlock (Func<T, T> cloner, DataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.cloner = cloner;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.messageBox = new PassingMessageBox<T> (messageQueue, compHelper, () => true, BroadcastProcess, dataflowBlockOptions);
			this.vault = new MessageVault<T> ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           T messageValue,
		                                           ISourceBlock<T> source,
		                                           bool consumeToAccept)
		{
			if (written.TryRelaxedSet ()) {
				Thread.MemoryBarrier ();
				finalValue = messageValue;
				Thread.MemoryBarrier ();
				ready = true;
				return messageBox.OfferMessage (this, messageHeader, finalValue, source, consumeToAccept);
			} else {
				return DataflowMessageStatus.DecliningPermanently;
			}
		}

		public IDisposable LinkTo (ITargetBlock<T> target, bool unlinkAfterOne)
		{
			return targets.AddTarget (target, unlinkAfterOne);
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
		{
			return cloner(vault.ConsumeMessage (messageHeader, target, out messageConsumed));
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<T> filter, out T item)
		{
			item = default (T);
			if (!written.Value)
				return false;

			if (!ready) {
				SpinWait spin = new SpinWait ();
				while (!ready)
					spin.SpinOnce ();
			}

			if (filter == null || filter (finalValue)) {
				item = cloner != null ? cloner (finalValue) : finalValue;
				return true;
			}

			return false;
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			items = null;
			if (!written.Value)
				return false;

			T item;
			if (!TryReceive (null, out item))
				return false;

			items = new T[] { item };
			return true;
		}

		void BroadcastProcess ()
		{
			T input;

			if (!messageQueue.TryTake (out input) || targets.Current == null)
				return;

			foreach (var target in targets) {
				DataflowMessageHeader header = headers.Increment ();
				if (cloner != null)
					vault.StoreMessage (header, input);
				target.OfferMessage (header, input, this, cloner != null);
			}
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
	}
}

