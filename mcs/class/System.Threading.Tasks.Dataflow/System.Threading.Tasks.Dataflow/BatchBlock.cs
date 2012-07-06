// BatchBlock.cs
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
	public sealed class BatchBlock<T> : IPropagatorBlock<T, T[]>, IReceivableSourceBlock<T[]> {
		readonly CompletionHelper compHelper;
		readonly BlockingCollection<T> messageQueue = new BlockingCollection<T> ();
		readonly MessageBox<T> messageBox;
		readonly GroupingDataflowBlockOptions dataflowBlockOptions;
		readonly int batchSize;
		int batchCount;
		readonly MessageOutgoingQueue<T[]> outgoing;
		SpinLock batchLock;
		readonly AtomicBoolean nonGreedyProcessing = new AtomicBoolean ();

		public BatchBlock (int batchSize) : this (batchSize, GroupingDataflowBlockOptions.Default)
		{
		}

		public BatchBlock (int batchSize, GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.batchSize = batchSize;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			this.messageBox = new PassingMessageBox<T> (this, messageQueue, compHelper,
				() => outgoing.IsCompleted, newItem => BatchProcess (newItem ? 1 : 0),
				dataflowBlockOptions, dataflowBlockOptions.Greedy);
			this.outgoing = new MessageOutgoingQueue<T[]> (this, compHelper,
				() => messageQueue.IsCompleted, messageBox.DecreaseCount,
				dataflowBlockOptions, batch => batch.Length);
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source,
			bool consumeToAccept)
		{
			return messageBox.OfferMessage (
				messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<T[]> target, DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget (target, linkOptions);
		}

		public T[] ConsumeMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<T[]> target,
			out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<T[]> filter, out T[] item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<T[]> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public void TriggerBatch ()
		{
			if (dataflowBlockOptions.Greedy) {
				int earlyBatchSize;
				do {
					earlyBatchSize = batchCount;
					if (earlyBatchSize == 0)
						return;
				} while (Interlocked.CompareExchange (ref batchCount, 0, earlyBatchSize)
				         != earlyBatchSize);

				MakeBatch (earlyBatchSize);
			} else {
				if (dataflowBlockOptions.BoundedCapacity == -1
				    || outgoing.Count <= dataflowBlockOptions.BoundedCapacity)
					EnsureNonGreedyProcessing (true);
			}
		}

		void BatchProcess (int addedItems = 1)
		{
			if (dataflowBlockOptions.Greedy) {
				// the Interlocked pattern is necessary, because this method
				// has to deal correctly with concurrent TriggerBatch ()
				int current;
				int previousCount;
				bool makeBatch;
				do {
					makeBatch = false;
					previousCount = batchCount;
					current = previousCount + addedItems;

					if (current >= batchSize) {
						current -= batchSize;
						makeBatch = true;
					}
				} while (Interlocked.CompareExchange (ref batchCount, current, previousCount)
				         != previousCount);

				if (makeBatch)
					MakeBatch (batchSize);
			} else {
				if (ShouldProcessNonGreedy ())
					EnsureNonGreedyProcessing (false);
			}
		}

		bool ShouldProcessNonGreedy ()
		{
			// do we have enough items waiting and would the new batch fit?
			return messageBox.PostponedMessagesCount >= batchSize
			       && (dataflowBlockOptions.BoundedCapacity == -1
			           || outgoing.Count + batchSize <= dataflowBlockOptions.BoundedCapacity);
		}

		void MakeBatch (int size)
		{
			T[] batch = new T[size];

			// lock is necessary here to make sure items are in the correct order
			bool taken = false;
			try {
				batchLock.Enter (ref taken);

				for (int i = 0; i < size; ++i)
					messageQueue.TryTake (out batch [i]);
			} finally {
				if (taken)
					batchLock.Exit ();
			}

			outgoing.AddData (batch);
		}

		void EnsureNonGreedyProcessing (bool manuallyTriggered)
		{
			if (nonGreedyProcessing.TrySet ())
				Task.Factory.StartNew (() => NonGreedyProcess (manuallyTriggered),
					dataflowBlockOptions.CancellationToken,
					TaskCreationOptions.PreferFairness,
					dataflowBlockOptions.TaskScheduler);
		}

		void NonGreedyProcess (bool manuallyTriggered)
		{
			bool first = true;

			do {
				var reservations =
					new List<Tuple<ISourceBlock<T>, DataflowMessageHeader>> ();

				int expectedReservationsCount = messageBox.PostponedMessagesCount;

				if (expectedReservationsCount == 0)
					break;

				bool gotReservation;
				do {
					var reservation = messageBox.ReserveMessage ();
					gotReservation = reservation != null;
					if (gotReservation)
						reservations.Add (reservation);
				} while (gotReservation && reservations.Count < batchSize);

				int expectedSize = manuallyTriggered && first
					                   ? Math.Min (expectedReservationsCount, batchSize)
					                   : batchSize;

				if (reservations.Count < expectedSize) {
					foreach (var reservation in reservations)
						messageBox.RelaseReservation (reservation);
					BatchProcess (reservations.Count);

					// some reservations failed, which most likely means the message
					// was consumed by someone else and a new one will be offered soon;
					// so postpone the batch, so that the other block has time to do that
					// (MS .Net does something like this too)
					if (manuallyTriggered && first) {
						Task.Factory.StartNew (() => NonGreedyProcess (true),
							dataflowBlockOptions.CancellationToken,
							TaskCreationOptions.PreferFairness,
							dataflowBlockOptions.TaskScheduler);
						return;
					}
				} else {
					T[] batch = new T[reservations.Count];

					for (int i = 0; i < reservations.Count; i++)
						batch [i] = messageBox.ConsumeReserved (reservations [i]);

					outgoing.AddData (batch);
				}

				first = false;
			} while (ShouldProcessNonGreedy ());

			nonGreedyProcessing.Value = false;
			if (ShouldProcessNonGreedy ())
				EnsureNonGreedyProcessing (false);
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

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}