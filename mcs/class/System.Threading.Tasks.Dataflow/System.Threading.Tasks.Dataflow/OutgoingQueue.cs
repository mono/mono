// OutgoingQueue.cs
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Version of <see cref="OutgoingQueueBase{T}"/> for
	/// non-broadcast blocks.
	/// </summary>
	class OutgoingQueue<T> : OutgoingQueueBase<T> {
		readonly Func<T, int> countSelector;
		SpinLock firstItemLock = new SpinLock();
		volatile ITargetBlock<T> reservedForTargetBlock;
		readonly TargetCollection<T> targets;

		protected override TargetCollectionBase<T> Targets {
			get { return targets; }
		}

		public OutgoingQueue (
			ISourceBlock<T> block, CompletionHelper compHelper,
			Func<bool> externalCompleteTester, Action<int> decreaseItemsCount,
			DataflowBlockOptions options, Func<T, int> countSelector = null)
			: base (compHelper, externalCompleteTester,
				decreaseItemsCount, options)
		{
			targets = new TargetCollection<T> (block);
			this.countSelector = countSelector;
		}

		/// <summary>
		/// Calculates the count of items in the given object.
		/// </summary>
		protected override int GetModifiedCount(T data)
		{
			if (countSelector == null)
				return 1;

			return countSelector (data);
		}

		/// <summary>
		/// Sends messages to targets.
		/// </summary>
		protected override void Process ()
		{
			bool processed;
			do {
				ForceProcessing = false;

				bool lockTaken = false;
				try {
					firstItemLock.Enter (ref lockTaken);

					T item;
					if (!Store.TryPeek (out item))
						break;

					if (!targets.HasCurrentItem)
						targets.SetCurrentItem (item);

					if (reservedForTargetBlock != null)
						break;

					processed = targets.OfferItemToTargets ();
					if (processed) {
						Outgoing.TryTake (out item);
						DecreaseCounts (item);
						FirstItemChanged ();
					}
				} finally {
					if (lockTaken)
						firstItemLock.Exit ();
				}
			} while (processed);

			IsProcessing.Value = false;

			// to guard against race condition
			if (ForceProcessing && reservedForTargetBlock == null)
				EnsureProcessing ();

			VerifyCompleteness ();
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader,
		                         ITargetBlock<T> targetBlock, out bool messageConsumed)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (targetBlock == null)
				throw new ArgumentNullException("target");

			T result = default(T);
			messageConsumed = false;

			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (targets.VerifyHeader (messageHeader, targetBlock)
				    && (reservedForTargetBlock == null
				        || reservedForTargetBlock == targetBlock)) {
					// cannot consume from faulted block, unless reserved
					if (reservedForTargetBlock == null && IsFaultedOrCancelled)
						return result;

					Outgoing.TryTake (out result);
					messageConsumed = true;
					DecreaseCounts (result);
					reservedForTargetBlock = null;
					FirstItemChanged ();
				}
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}

			targets.UnpostponeTarget (targetBlock, messageConsumed);
			EnsureProcessing ();
			VerifyCompleteness ();

			return result;
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (target == null)
				throw new ArgumentNullException("target");

			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (targets.VerifyHeader(messageHeader, target)) {
					reservedForTargetBlock = target;
					return true;
				}

				targets.UnpostponeTarget (target, false);
				EnsureProcessing ();

				return false;
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (target == null)
				throw new ArgumentNullException("target");

			bool lockTaken = false;
			try
			{
				firstItemLock.Enter(ref lockTaken);

				if (!targets.VerifyHeader(messageHeader, target)
				    || reservedForTargetBlock != target)
					throw new InvalidOperationException(
						"The target did not have the message reserved.");

				reservedForTargetBlock = null;
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}

			targets.UnpostponeTarget (target, false);
			EnsureProcessing ();
		}

		/// <summary>
		/// Notifies that the first item in the queue changed.
		/// </summary>
		void FirstItemChanged ()
		{
			T firstItem;
			if (Store.TryPeek (out firstItem))
				targets.SetCurrentItem (firstItem);
			else
				targets.ResetCurrentItem ();
		}

		public bool TryReceive (Predicate<T> filter, out T item)
		{
			bool success = false;
			item = default (T);

			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (reservedForTargetBlock != null)
					return false;

				T result;
				if (Store.TryPeek (out result) && (filter == null || filter (result))) {
					Outgoing.TryTake (out item);
					success = true;
					DecreaseCounts (item);
					FirstItemChanged ();
				}
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}

			EnsureProcessing ();
			VerifyCompleteness ();

			return success;
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			items = null;

			if (Store.IsEmpty)
				return false;

			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (reservedForTargetBlock != null)
					return false;

				var list = new List<T> (Outgoing.Count);

				T item;
				while (Outgoing.TryTake (out item)) {
					DecreaseCounts (item);
					list.Add (item);
				}

				items = list;

				FirstItemChanged ();
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}

			EnsureProcessing ();
			VerifyCompleteness ();

			return items.Count > 0;
		}
	}
}