// BroadcastOutgoingQueue.cs
//
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Version of <see cref="OutgoingQueueBase{T}"/> for broadcast blocks.
	/// </summary>
	class BroadcastOutgoingQueue<T> : OutgoingQueueBase<T> {
		volatile bool hasCurrentItem;
		// don't use directly, only through CurrentItem (and carefully)
		T currentItem;
		SpinLock currentItemLock = new SpinLock();

		readonly BroadcastTargetCollection<T> targets;

		protected override TargetCollectionBase<T> Targets {
			get { return targets; }
		}

		readonly ConcurrentDictionary<Tuple<DataflowMessageHeader, ITargetBlock<T>>, T>
			reservedMessages =
				new ConcurrentDictionary<Tuple<DataflowMessageHeader, ITargetBlock<T>>, T>();

		public BroadcastOutgoingQueue (
			ISourceBlock<T> block, CompletionHelper compHelper,
			Func<bool> externalCompleteTester, Action<int> decreaseItemsCount,
			DataflowBlockOptions options, bool hasCloner)
			: base (compHelper, externalCompleteTester, decreaseItemsCount, options)
		{
			targets = new BroadcastTargetCollection<T> (block, hasCloner);
		}

		/// <summary>
		/// The current item that is to be sent to taget blocks.
		/// </summary>
		T CurrentItem {
			get {
				T item;
				bool lockTaken = false;
				try {
					currentItemLock.Enter (ref lockTaken);
					item = currentItem;
				} finally {
					if (lockTaken)
						currentItemLock.Exit ();
				}
				return item;
			}
			set {
				hasCurrentItem = true;

				bool lockTaken = false;
				try {
					currentItemLock.Enter (ref lockTaken);
					currentItem = value;
				} finally {
					if (lockTaken)
						currentItemLock.Exit ();
				}
			}
		}

		/// <summary>
		/// Takes an item from the queue and sets it as <see cref="CurrentItem"/>.
		/// </summary>
		public void DequeueItem ()
		{
			T item;
			if (Outgoing.TryTake (out item)) {
				DecreaseCounts (item);
				targets.SetCurrentItem (item);

				CurrentItem = item;
			}
		}

		/// <summary>
		/// Manages sending items to the target blocks.
		/// </summary>
		protected override void Process ()
		{
			do {
				ForceProcessing = false;

				DequeueItem ();

				targets.OfferItemToTargets ();
			} while (!Store.IsEmpty || targets.NeedsProcessing);

			IsProcessing.Value = false;

			// to guard against race condition
			if (ForceProcessing)
				EnsureProcessing ();

			VerifyCompleteness ();
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader,
		                         ITargetBlock<T> target, out bool messageConsumed)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (target == null)
				throw new ArgumentNullException("target");

			T item;
			if (reservedMessages.TryRemove (Tuple.Create (messageHeader, target), out item)) {
				messageConsumed = true;
				return item;
			}

			// if we first retrieve CurrentItem and then check the header,
			// there will be no race condition

			item = CurrentItem;

			if (!targets.VerifyHeader (messageHeader)) {
				targets.UnpostponeTargetNotConsumed (target);

				messageConsumed = false;
				return default(T);
			}

			targets.UnpostponeTargetConsumed (target, messageHeader);
			EnsureProcessing ();

			messageConsumed = true;
			return item;
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader,
		                            ITargetBlock<T> target)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (target == null)
				throw new ArgumentNullException("target");

			T item = CurrentItem;

			if (!targets.VerifyHeader (messageHeader)) {
				targets.UnpostponeTargetNotConsumed (target);
				EnsureProcessing ();
				return false;
			}

			targets.ReserveTarget (target);
			reservedMessages [Tuple.Create (messageHeader, target)] = item;
			return true;
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader,
		                                ITargetBlock<T> target)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (target == null)
				throw new ArgumentNullException("target");

			T item;
			if (!reservedMessages.TryRemove (Tuple.Create (messageHeader, target), out item))
				throw new InvalidOperationException (
					"The target did not have the message reserved.");

			targets.UnpostponeTargetNotConsumed (target);
			EnsureProcessing ();
		}

		public bool TryReceive (Predicate<T> filter, out T retrievedItem)
		{
			retrievedItem = default(T);

			if (!hasCurrentItem) {
				return false;
			}

			T item = CurrentItem;

			if (filter == null || filter(item)) {
				retrievedItem = item;
				return true;
			}

			return false;
		}
	}
}