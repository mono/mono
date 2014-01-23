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
		long numberOfGroups;
		SpinLock batchCountLock;
		readonly OutgoingQueue<T[]> outgoing;
		SpinLock batchLock;
		readonly AtomicBoolean nonGreedyProcessing = new AtomicBoolean ();

		public BatchBlock (int batchSize) : this (batchSize, GroupingDataflowBlockOptions.Default)
		{
		}

		public BatchBlock (int batchSize, GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			if (batchSize <= 0)
				throw new ArgumentOutOfRangeException ("batchSize", batchSize,
					"The batchSize must be positive.");
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");
			if (dataflowBlockOptions.BoundedCapacity != -1
			    && batchSize > dataflowBlockOptions.BoundedCapacity)
				throw new ArgumentOutOfRangeException ("batchSize",
					"The batchSize must be smaller than the value of BoundedCapacity.");

			this.batchSize = batchSize;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);

			Action<bool> processQueue;
			Func<bool> canAccept;
			if (dataflowBlockOptions.MaxNumberOfGroups == -1) {
				processQueue = newItem => BatchProcess (newItem ? 1 : 0);
				canAccept = null;
			} else {
				processQueue = _ => BatchProcess ();
				canAccept = TryAdd;
			}

			this.messageBox = new PassingMessageBox<T> (this, messageQueue, compHelper,
				() => outgoing.IsCompleted, processQueue, dataflowBlockOptions,
				dataflowBlockOptions.Greedy, canAccept);
			this.outgoing = new OutgoingQueue<T[]> (this, compHelper,
				() => messageQueue.IsCompleted, messageBox.DecreaseCount,
				dataflowBlockOptions, batch => batch.Length);
		}

		DataflowMessageStatus ITargetBlock<T>.OfferMessage (
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

		T[] ISourceBlock<T[]>.ConsumeMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<T[]> target,
			out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		void ISourceBlock<T[]>.ReleaseReservation (
			DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		bool ISourceBlock<T[]>.ReserveMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
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

		/// <summary>
		/// Verifies whether <see cref="GroupingDataflowBlockOptions.MaxNumberOfGroups"/>
		/// has been reached. If it did, <see cref="Complete"/>s the block.
		/// </summary>
		void VerifyMaxNumberOfGroups ()
		{
			if (dataflowBlockOptions.MaxNumberOfGroups == -1)
				return;

			bool shouldComplete;

			bool lockTaken = false;
			try {
				batchCountLock.Enter (ref lockTaken);

				shouldComplete = numberOfGroups >= dataflowBlockOptions.MaxNumberOfGroups;
			} finally {
				if (lockTaken)
					batchCountLock.Exit ();
			}

			if (shouldComplete)
				Complete ();
		}

		/// <summary>
		/// Returns whether a new item can be accepted, and increments a counter if it can.
		/// Only makes sense when <see cref="GroupingDataflowBlockOptions.MaxNumberOfGroups"/>
		/// is not unbounded.
		/// </summary>
		bool TryAdd ()
		{
			bool lockTaken = false;
			try {
				batchCountLock.Enter (ref lockTaken);

				if (numberOfGroups + batchCount / batchSize
				    >= dataflowBlockOptions.MaxNumberOfGroups)
					return false;

				batchCount++;
				return true;
			} finally {
				if (lockTaken)
					batchCountLock.Exit ();
			}
		}

		public void TriggerBatch ()
		{
			if (dataflowBlockOptions.Greedy) {
				int earlyBatchSize;

				bool lockTaken = false;
				try {
					batchCountLock.Enter (ref lockTaken);
					
					if (batchCount == 0)
						return;

					earlyBatchSize = batchCount;
					batchCount = 0;
					numberOfGroups++;
				} finally {
					if (lockTaken)
						batchCountLock.Exit ();
				}

				MakeBatch (earlyBatchSize);
			} else {
				if (dataflowBlockOptions.BoundedCapacity == -1
				    || outgoing.Count <= dataflowBlockOptions.BoundedCapacity)
					EnsureNonGreedyProcessing (true);
			}
		}

		/// <summary>
		/// Decides whether to create a new batch or not.
		/// </summary>
		/// <param name="addedItems">
		/// Number of newly added items. Used only with greedy processing.
		/// </param>
		void BatchProcess (int addedItems = 0)
		{
			if (dataflowBlockOptions.Greedy) {
				bool makeBatch = false;

				bool lockTaken = false;
				try {
					batchCountLock.Enter (ref lockTaken);

					batchCount += addedItems;

					if (batchCount >= batchSize) {
						batchCount -= batchSize;
						numberOfGroups++;
						makeBatch = true;
					}
				} finally {
					if (lockTaken)
						batchCountLock.Exit ();
				}

				if (makeBatch)
					MakeBatch (batchSize);
			} else {
				if (ShouldProcessNonGreedy ())
					EnsureNonGreedyProcessing (false);
			}
		}

		/// <summary>
		/// Returns whether non-greedy creation of a batch should be started.
		/// </summary>
		bool ShouldProcessNonGreedy ()
		{
			// do we have enough items waiting and would the new batch fit?
			return messageBox.PostponedMessagesCount >= batchSize
			       && (dataflowBlockOptions.BoundedCapacity == -1
			           || outgoing.Count + batchSize <= dataflowBlockOptions.BoundedCapacity);
		}

		/// <summary>
		/// Creates a batch of the given size and adds the result to the output queue.
		/// </summary>
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

			VerifyMaxNumberOfGroups ();
		}

		/// <summary>
		/// Starts non-greedy creation of batches, if one doesn't already run.
		/// </summary>
		/// <param name="manuallyTriggered">Whether the batch was triggered by <see cref="TriggerBatch"/>.</param>
		void EnsureNonGreedyProcessing (bool manuallyTriggered)
		{
			if (nonGreedyProcessing.TrySet ())
				Task.Factory.StartNew (() => NonGreedyProcess (manuallyTriggered),
					dataflowBlockOptions.CancellationToken,
					TaskCreationOptions.PreferFairness,
					dataflowBlockOptions.TaskScheduler);
		}

		/// <summary>
		/// Creates batches in non-greedy mode,
		/// making sure the whole batch is available by using reservations.
		/// </summary>
		/// <param name="manuallyTriggered">Whether the batch was triggered by <see cref="TriggerBatch"/>.</param>
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

					// non-greedy doesn't need lock
					numberOfGroups++;

					VerifyMaxNumberOfGroups ();
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
			TriggerBatch ();
			outgoing.Complete ();
		}

		void IDataflowBlock.Fault (Exception exception)
		{
			compHelper.RequestFault (exception);
		}

		public Task Completion {
			get { return compHelper.Completion; }
		}

		public int OutputCount {
			get { return outgoing.Count; }
		}

		public int BatchSize {
			get { return batchSize; }
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}