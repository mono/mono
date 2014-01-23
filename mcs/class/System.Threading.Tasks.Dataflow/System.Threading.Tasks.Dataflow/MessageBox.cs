// MessageBox.cs
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
using System.Linq;

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// In MessageBox we store message that have been offered to us so that they can be
	/// later processed 
	/// </summary>
	internal abstract class MessageBox<TInput> {
		protected ITargetBlock<TInput> Target { get; set; }
		protected CompletionHelper CompHelper { get; private set; }
		readonly Func<bool> externalCompleteTester;
		readonly DataflowBlockOptions options;
		readonly bool greedy;
		readonly Func<bool> canAccept;

		readonly ConcurrentDictionary<ISourceBlock<TInput>, DataflowMessageHeader>
			postponedMessages =
				new ConcurrentDictionary<ISourceBlock<TInput>, DataflowMessageHeader> ();
		int itemCount;
		readonly AtomicBoolean postponedProcessing = new AtomicBoolean ();

		// these two fields are used only in one special case
		SpinLock consumingLock;
		// this is necessary, because canAccept is not pure
		bool canAcceptFromBefore;

		protected BlockingCollection<TInput> MessageQueue { get; private set; }

		protected MessageBox (
			ITargetBlock<TInput> target, BlockingCollection<TInput> messageQueue,
			CompletionHelper compHelper, Func<bool> externalCompleteTester,
			DataflowBlockOptions options, bool greedy = true, Func<bool> canAccept = null)
		{
			this.Target = target;
			this.CompHelper = compHelper;
			this.MessageQueue = messageQueue;
			this.externalCompleteTester = externalCompleteTester;
			this.options = options;
			this.greedy = greedy;
			this.canAccept = canAccept;
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TInput messageValue,
			ISourceBlock<TInput> source, bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (consumeToAccept && source == null)
				throw new ArgumentException (
					"consumeToAccept may only be true if provided with a non-null source.",
					"consumeToAccept");

			if (MessageQueue.IsAddingCompleted || !CompHelper.CanRun)
				return DataflowMessageStatus.DecliningPermanently;

			var full = options.BoundedCapacity != -1
			           && Thread.VolatileRead (ref itemCount) >= options.BoundedCapacity;
			if (!greedy || full) {
				if (source == null)
					return DataflowMessageStatus.Declined;

				postponedMessages [source] = messageHeader;

				// necessary to avoid race condition
				DecreaseCount (0);

				if (!greedy && !full)
					EnsureProcessing (true);
				
				return DataflowMessageStatus.Postponed;
			}

			// in this case, we need to use locking to make sure
			// we don't consume when we can't accept
			if (consumeToAccept && canAccept != null) {
				bool lockTaken = false;
				try {
					consumingLock.Enter (ref lockTaken);
					if (!canAcceptFromBefore && !canAccept ())
						return DataflowMessageStatus.DecliningPermanently;

					bool consummed;
					messageValue = source.ConsumeMessage (messageHeader, Target, out consummed);
					if (!consummed) {
						canAcceptFromBefore = true;
						return DataflowMessageStatus.NotAvailable;
					}

					canAcceptFromBefore = false;
				} finally {
					if (lockTaken)
						consumingLock.Exit ();
				}
			} else {
				if (consumeToAccept) {
					bool consummed;
					messageValue = source.ConsumeMessage (messageHeader, Target, out consummed);
					if (!consummed)
						return DataflowMessageStatus.NotAvailable;
				}

				if (canAccept != null && !canAccept ())
					return DataflowMessageStatus.DecliningPermanently;
			}

			try {
				MessageQueue.Add (messageValue);
			} catch (InvalidOperationException) {
				// This is triggered either if the underlying collection didn't accept the item
				// or if the messageQueue has been marked complete, either way it corresponds to a false
				return DataflowMessageStatus.DecliningPermanently;
			}

			IncreaseCount ();

			EnsureProcessing (true);

			VerifyCompleteness ();

			return DataflowMessageStatus.Accepted;
		}

		/// <summary>
		/// Increses the count of items in the block by 1.
		/// </summary>
		public void IncreaseCount ()
		{
			Interlocked.Increment (ref itemCount);
		}

		/// <summary>
		/// Decreses the number of items in the block by the given count.
		/// </summary>
		/// <remarks>
		/// The <paramref name="count"/> parameter is used when one object
		/// can represent many items, like a batch in <see cref="BatchBlock{T}"/>.
		/// </remarks>
		public void DecreaseCount (int count = 1)
		{
			int decreased = Interlocked.Add (ref itemCount, -count);

			// if BoundedCapacity is -1, there is no need to do this
			if (decreased < options.BoundedCapacity && !postponedMessages.IsEmpty) {
				if (greedy)
					EnsurePostponedProcessing ();
				else
					EnsureProcessing (false);
			}
		}

		/// <summary>
		/// The number of messages that were postponed
		/// and can be attempted to be consumed.
		/// </summary>
		public int PostponedMessagesCount {
			get { return postponedMessages.Count; }
		}

		/// <summary>
		/// Reserves a message from those that were postponed.
		/// Does not guarantee any order of the messages being reserved.
		/// </summary>
		/// <returns>
		/// An object representing the reservation on success,
		/// <c>null</c> on failure.
		/// </returns>
		public Tuple<ISourceBlock<TInput>, DataflowMessageHeader> ReserveMessage()
		{
			while (!postponedMessages.IsEmpty) {
				// KeyValuePair is a struct, so default value is not null
				var block = postponedMessages.FirstOrDefault () .Key;

				// collection is empty
				if (block == null)
					break;

				DataflowMessageHeader header;
				bool removed = postponedMessages.TryRemove (block, out header);

				// another thread was faster, try again
				if (!removed)
					continue;

				bool reserved = block.ReserveMessage (header, Target);
				if (reserved)
					return Tuple.Create (block, header);
			}

			return null;
		}

		/// <summary>
		/// Releases the given reservation.
		/// </summary>
		public void RelaseReservation(Tuple<ISourceBlock<TInput>, DataflowMessageHeader> reservation)
		{
			reservation.Item1.ReleaseReservation (reservation.Item2, Target);
		}

		/// <summary>
		/// Consumes previously reserved item.
		/// </summary>
		public TInput ConsumeReserved(Tuple<ISourceBlock<TInput>, DataflowMessageHeader> reservation)
		{
			bool consumed;
			return reservation.Item1.ConsumeMessage (
				reservation.Item2, Target, out consumed);
		}

		/// <summary>
		/// Makes sure retrieving items that were postponed,
		/// because they would exceed <see cref="DataflowBlockOptions.BoundedCapacity"/>,
		/// is currently running.
		/// </summary>
		void EnsurePostponedProcessing ()
		{
			if (postponedProcessing.TrySet())
				Task.Factory.StartNew (RetrievePostponed, options.CancellationToken,
					TaskCreationOptions.PreferFairness, options.TaskScheduler);
		}

		/// <summary>
		/// Retrieves items that were postponed,
		/// because they would exceed <see cref="DataflowBlockOptions.BoundedCapacity"/>.
		/// </summary>
		void RetrievePostponed ()
		{
			// BoundedCapacity can't be -1 here, because in that case there would be no postponing
			while (Thread.VolatileRead (ref itemCount) < options.BoundedCapacity
			       && !postponedMessages.IsEmpty && !MessageQueue.IsAddingCompleted) {
				var block = postponedMessages.First ().Key;
				DataflowMessageHeader header;
				postponedMessages.TryRemove (block, out header);

				bool consumed;
				var item = block.ConsumeMessage (header, Target, out consumed);
				if (consumed) {
					try {
						MessageQueue.Add (item);
						IncreaseCount ();
						EnsureProcessing (false);
					} catch (InvalidOperationException) {
						break;
					}
				}
			}

			// release all postponed messages
			if (MessageQueue.IsAddingCompleted) {
				while (!postponedMessages.IsEmpty) {
					var block = postponedMessages.First ().Key;
					DataflowMessageHeader header;
					postponedMessages.TryRemove (block, out header);

					if (block.ReserveMessage (header, Target))
						block.ReleaseReservation (header, Target);
				}
			}

			postponedProcessing.Value = false;

			// because of race
			if ((Thread.VolatileRead (ref itemCount) < options.BoundedCapacity
			     || MessageQueue.IsAddingCompleted)
			    && !postponedMessages.IsEmpty)
				EnsurePostponedProcessing ();
		}

		/// <summary>
		/// Makes sure the input queue is processed the way it needs to.
		/// </summary>
		/// <param name="newItem">Was new item just added?</param>
		protected abstract void EnsureProcessing (bool newItem);

		/// <summary>
		/// Completes the box, no new messages will be accepted.
		/// Also starts the process of completing the output queue.
		/// </summary>
		public void Complete ()
		{
			// Make message queue complete
			MessageQueue.CompleteAdding ();
			OutgoingQueueComplete ();
			VerifyCompleteness ();

			if (!postponedMessages.IsEmpty)
				EnsurePostponedProcessing ();
		}

		/// <summary>
		/// Notifies that outgoing queue should be completed, if possible.
		/// </summary>
		protected virtual void OutgoingQueueComplete ()
		{
		}

		/// <summary>
		/// Makes sure the block is completed if it should be.
		/// </summary>
		protected virtual void VerifyCompleteness ()
		{
			if (MessageQueue.IsCompleted && externalCompleteTester ())
				CompHelper.Complete ();
		}
	}
}