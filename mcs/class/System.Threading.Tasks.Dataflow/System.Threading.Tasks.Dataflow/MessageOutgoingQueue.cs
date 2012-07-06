// MessageOutgoingQueue.cs
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
	/// <summary>
	/// This class handles outgoing message that get queued when there is no
	/// block on the other end to proces it. It also allows receive operations.
	/// </summary>
	class MessageOutgoingQueue<T> {
		readonly ConcurrentQueue<T> store = new ConcurrentQueue<T> ();
		readonly BlockingCollection<T> outgoing;
		int outgoingCount;
		readonly CompletionHelper compHelper;
		readonly Func<bool> externalCompleteTester;
		readonly Action<int> decreaseItemsCount;
		readonly DataflowBlockOptions options;
		readonly Func<T, int> countSelector;
		readonly AtomicBoolean isProcessing = new AtomicBoolean ();
		readonly TargetCollection<T> targets;
		SpinLock firstItemLock = new SpinLock();
		volatile ITargetBlock<T> reservedForTargetBlock;
		int totalModifiedCount;

		public MessageOutgoingQueue (
			ISourceBlock<T> block, CompletionHelper compHelper,
			Func<bool> externalCompleteTester, Action<int> decreaseItemsCount, DataflowBlockOptions options,
			Func<T, int> countSelector = null)
		{
			this.outgoing = new BlockingCollection<T> (store);
			this.targets = new TargetCollection<T> (block);
			this.compHelper = compHelper;
			this.externalCompleteTester = externalCompleteTester;
			this.decreaseItemsCount = decreaseItemsCount;
			this.options = options;
			this.countSelector = countSelector;
		}

		public int GetModifiedCount(T data)
		{
			if (countSelector == null)
				return 1;

			return countSelector (data);
		}

		public void AddData (T data)
		{
			try {
				outgoing.Add (data);
				Interlocked.Add (ref totalModifiedCount, GetModifiedCount (data));
				if (Interlocked.Increment (ref outgoingCount) == 1)
					VerifyProcessing ();
			} catch (InvalidOperationException) {
				VerifyCompleteness ();
			}
		}

		void VerifyProcessing ()
		{
			if (isProcessing.TrySet())
				Task.Factory.StartNew (Process, CancellationToken.None,
					TaskCreationOptions.PreferFairness, options.TaskScheduler);
		}

		void Process ()
		{
			bool processed;
			do {
				bool lockTaken = false;
				try {
					firstItemLock.Enter (ref lockTaken);

					T item;
					if (!store.TryPeek (out item))
						break;

					if (!targets.HasCurrentItem)
						targets.SetCurrentItem (item);

					if (reservedForTargetBlock != null)
						break;

					processed = targets.OfferItemToTargets ();
					if (processed) {
						outgoing.TryTake (out item);
						DecreaseCounts (item);
						FirstItemChanged ();
					}
				} finally {
					if (lockTaken)
						firstItemLock.Exit ();
				}
			} while (processed);

			isProcessing.Value = false;

			// to guard against race condition
			if (!store.IsEmpty && reservedForTargetBlock == null && targets.NeedsProcessing)
				VerifyProcessing ();

			VerifyCompleteness ();
		}

		void DecreaseCounts (T data)
		{
			var modifiedCount = GetModifiedCount (data);
			Interlocked.Add (ref totalModifiedCount, -modifiedCount);
			Interlocked.Decrement (ref outgoingCount);
			decreaseItemsCount (modifiedCount);
		}

		public IDisposable AddTarget(ITargetBlock<T> targetBlock, DataflowLinkOptions linkOptions)
		{
			var result = targets.AddTarget (targetBlock, linkOptions);
			VerifyProcessing ();
			return result;
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> targetBlock, out bool messageConsumed)
		{
			T result = default(T);
			messageConsumed = false;

			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (targets.VerifyHeader (messageHeader, targetBlock)
				    && (reservedForTargetBlock == null
				        || reservedForTargetBlock == targetBlock)) {
					outgoing.TryTake (out result);
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
			VerifyProcessing ();
			VerifyCompleteness ();

			return result;
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (targets.VerifyHeader(messageHeader, target)) {
					reservedForTargetBlock = target;
					return true;
				}

				targets.UnpostponeTarget (target, false);
				VerifyProcessing ();

				return false;
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
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

			VerifyProcessing ();
		}

		void FirstItemChanged ()
		{
			T firstItem;
			if (store.TryPeek (out firstItem))
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
				if (store.TryPeek (out result) && (filter == null || filter (result))) {
					outgoing.TryTake (out item);
					success = true;
					DecreaseCounts (item);
					FirstItemChanged ();
				}
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}

			VerifyProcessing ();
			VerifyCompleteness ();

			return success;
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			items = null;

			if (store.IsEmpty)
				return false;

			bool lockTaken = false;
			try {
				firstItemLock.Enter (ref lockTaken);

				if (reservedForTargetBlock != null)
					return false;

				var list = new List<T> (outgoing.Count);

				T item;
				while (outgoing.TryTake (out item)) {
					DecreaseCounts (item);
					list.Add (item);
				}

				items = list;

				FirstItemChanged ();
			} finally {
				if (lockTaken)
					firstItemLock.Exit ();
			}

			VerifyProcessing ();
			VerifyCompleteness ();

			return items.Count > 0;
		}

		public void Complete ()
		{
			outgoing.CompleteAdding ();
			VerifyCompleteness ();
		}

		void VerifyCompleteness ()
		{
			if (outgoing.IsCompleted && externalCompleteTester ())
				compHelper.Complete ();
		}

		public bool IsEmpty {
			get { return store.IsEmpty; }
		}

		public int Count {
			get { return totalModifiedCount; }
		}

		public bool IsCompleted {
			get { return outgoing.IsCompleted; }
		}
	}
}