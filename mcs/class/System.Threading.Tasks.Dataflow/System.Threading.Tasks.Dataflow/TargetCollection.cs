// TargetCollection.cs
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
using System.Collections.Generic;

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Collection of target blocks for a source block.
	/// Also handles sending messages to the target blocks.
	/// </summary>
	abstract class TargetCollectionBase<T> {
		/// <summary>
		/// Represents a target block with its options.
		/// </summary>
		protected class Target : IDisposable {
			readonly TargetCollectionBase<T> targetCollection;
			volatile int remainingMessages;
			readonly CancellationTokenSource cancellationTokenSource;

			public ITargetBlock<T> TargetBlock { get; private set; }

			public Target (TargetCollectionBase<T> targetCollection,
			               ITargetBlock<T> targetBlock, int maxMessages,
			               CancellationTokenSource cancellationTokenSource)
			{
				TargetBlock = targetBlock;
				this.targetCollection = targetCollection;
				remainingMessages = maxMessages;
				this.cancellationTokenSource = cancellationTokenSource;

				Postponed = new AtomicBoolean ();
				Reserved = new AtomicBoolean ();
			}

			public void MessageSent()
			{
				if (remainingMessages != -1)
					remainingMessages--;
				if (remainingMessages == 0)
					Dispose ();
			}

			readonly AtomicBoolean disabled = new AtomicBoolean ();
			public bool Disabled
			{
				get { return disabled.Value; }
			}

			public void Dispose ()
			{
				disabled.Value = true;

				if (cancellationTokenSource != null)
					cancellationTokenSource.Cancel ();

				Target ignored;
				targetCollection.targetDictionary.TryRemove (TargetBlock, out ignored);

				// to avoid memory leak; it could take a long time
				// before this object is actually removed from the collection
				TargetBlock = null;
			}

			public AtomicBoolean Postponed { get; private set; }
			
			// used only by broadcast blocks
			public AtomicBoolean Reserved { get; private set; }
		}

		readonly ISourceBlock<T> block;
		readonly bool broadcast;
		readonly bool consumeToAccept;

		readonly ConcurrentQueue<Target> prependQueue = new ConcurrentQueue<Target> ();
		readonly ConcurrentQueue<Target> appendQueue = new ConcurrentQueue<Target> ();
		readonly LinkedList<Target> targets = new LinkedList<Target> ();

		protected readonly ConcurrentDictionary<ITargetBlock<T>, Target> targetDictionary =
			new ConcurrentDictionary<ITargetBlock<T>, Target> ();

		// lastMessageHeaderId will be always accessed only from one thread
		long lastMessageHeaderId;
		// currentMessageHeaderId can be read from multiple threads at the same time
		long currentMessageHeaderId;

		bool firstOffering;
		T currentItem;

		public TargetCollectionBase (ISourceBlock<T> block, bool broadcast, bool consumeToAccept)
		{
			this.block = block;
			this.broadcast = broadcast;
			this.consumeToAccept = consumeToAccept;
		}

		public IDisposable AddTarget (ITargetBlock<T> targetBlock, DataflowLinkOptions options)
		{
			CancellationTokenSource cancellationTokenSource = null;
			if (options.PropagateCompletion) {
				cancellationTokenSource = new CancellationTokenSource();
				block.Completion.ContinueWith (t =>
				{
					if (t.IsFaulted)
						targetBlock.Fault (t.Exception);
					else
						targetBlock.Complete ();
				}, cancellationTokenSource.Token);
			}

			var target = new Target (
				this, targetBlock, options.MaxMessages, cancellationTokenSource);
			targetDictionary [targetBlock] = target;
			if (options.Append)
				appendQueue.Enqueue (target);
			else
				prependQueue.Enqueue (target);

			return target;
		}

		public void SetCurrentItem (T item)
		{
			firstOffering = true;
			currentItem = item;
			Thread.VolatileWrite (ref currentMessageHeaderId, ++lastMessageHeaderId);

			ClearUnpostponed ();
		}

		protected abstract void ClearUnpostponed ();

		public void ResetCurrentItem ()
		{
			currentItem = default(T);
			Thread.VolatileWrite (ref currentMessageHeaderId, 0);
		}

		public bool HasCurrentItem {
			get { return Thread.VolatileRead (ref currentMessageHeaderId) != 0; }
		}

		public bool OfferItemToTargets ()
		{
			// is there an item to offer?
			if (!HasCurrentItem)
				return false;

			var old = Tuple.Create (targets.First, targets.Last);

			do {
				// order is important here, we want to make sure that prepended target
				// added after appended target is always processed first
				var appended = PrependOrAppend (false);
				var prepended = PrependOrAppend (true);

				if (OfferItemToTargets (prepended))
					return true;

				if (firstOffering) {
					if (OfferItemToTargets (old))
						return true;
					firstOffering = false;
				} else {
					if (OfferItemToUnpostponed ())
						return true;
				}

				if (OfferItemToTargets (appended))
					return true;
			} while (NeedsProcessing);

			return false;
		}

		public bool NeedsProcessing {
			get {
				return !appendQueue.IsEmpty || !prependQueue.IsEmpty
				       || !UnpostponedIsEmpty;
			}
		}

		protected abstract bool UnpostponedIsEmpty { get; }

		Tuple<LinkedListNode<Target>, LinkedListNode<Target>> PrependOrAppend (
			bool prepend)
		{
			var queue = prepend ? prependQueue : appendQueue;

			if (queue.IsEmpty)
				return null;

			LinkedListNode<Target> first = null;
			LinkedListNode<Target> last = null;

			Target target;
			while (queue.TryDequeue (out target)) {
				var node = prepend
					           ? targets.AddFirst (target)
					           : targets.AddLast (target);
				if (first == null)
					first = node;
				last = node;
			}

			return prepend
				       ? Tuple.Create (last, first)
				       : Tuple.Create (first, last);
		}

		bool OfferItemToTargets (
			Tuple<LinkedListNode<Target>, LinkedListNode<Target>> targetPair)
		{
			if (targetPair == null
			    || targetPair.Item1 == null || targetPair.Item2 == null)
				return false;

			var node = targetPair.Item1;
			while (node != targetPair.Item2.Next) {
				if (node.Value.Disabled) {
					var nodeToRemove = node;
					node = node.Next;
					targets.Remove (nodeToRemove);
					continue;
				}

				if (OfferItem (node.Value) && !broadcast)
					return true;

				node = node.Next;
			}

			return false;
		}

		protected abstract bool OfferItemToUnpostponed ();

		protected bool OfferItem (Target target)
		{
			if (target.Reserved.Value)
				return false;
			if (!broadcast && target.Postponed.Value)
				return false;

			var result = target.TargetBlock.OfferMessage (
				// volatile read is not necessary here,
				// because currentMessageHeaderId is always written from this thread
				new DataflowMessageHeader (currentMessageHeaderId), currentItem, block,
				consumeToAccept);

			switch (result) {
			case DataflowMessageStatus.Accepted:
				target.MessageSent ();
				return true;
			case DataflowMessageStatus.Postponed:
				target.Postponed.Value = true;
				return false;
			case DataflowMessageStatus.DecliningPermanently:
				target.Dispose ();
				return false;
			default:
				return false;
			}
		}

		public bool VerifyHeader (DataflowMessageHeader header)
		{
			return header.Id == Thread.VolatileRead (ref currentMessageHeaderId);
		}
	}

	class TargetCollection<T> : TargetCollectionBase<T> {
		readonly ConcurrentQueue<Target> unpostponedTargets =
			new ConcurrentQueue<Target> ();

		public TargetCollection (ISourceBlock<T> block)
			: base (block, false, false)
		{
		}

		protected override bool UnpostponedIsEmpty {
			get { return unpostponedTargets.IsEmpty; }
		}

		public bool VerifyHeader (DataflowMessageHeader header, ITargetBlock<T> targetBlock)
		{
			return VerifyHeader (header)
			       && targetDictionary[targetBlock].Postponed.Value;
		}

		public void UnpostponeTarget (ITargetBlock<T> targetBlock, bool messageConsumed)
		{
			Target target;
			if (!targetDictionary.TryGetValue (targetBlock, out target))
				return;

			if (messageConsumed)
				target.MessageSent ();
			unpostponedTargets.Enqueue (target);

			target.Postponed.Value = false;
		}

		protected override void ClearUnpostponed ()
		{
			Target ignored;
			while (unpostponedTargets.TryDequeue (out ignored)) {
			}
		}

		protected override bool OfferItemToUnpostponed ()
		{
			Target target;
			while (unpostponedTargets.TryDequeue (out target)) {
				if (!target.Disabled && OfferItem (target))
					return true;
			}

			return false;
		}
	}

	class BroadcastTargetCollection<T> : TargetCollectionBase<T> {
		// it's necessary to store the headers because of a race between
		// UnpostponeTargetConsumed and SetCurrentItem
		readonly ConcurrentQueue<Tuple<Target, DataflowMessageHeader>>
			unpostponedTargets =
				new ConcurrentQueue<Tuple<Target, DataflowMessageHeader>> ();

		public BroadcastTargetCollection (ISourceBlock<T> block, bool consumeToAccept)
			: base (block, true, consumeToAccept)
		{
		}

		protected override bool UnpostponedIsEmpty {
			get { return unpostponedTargets.IsEmpty; }
		}

		public void ReserveTarget (ITargetBlock<T> targetBlock)
		{
			targetDictionary [targetBlock].Reserved.Value = true;
		}

		public void UnpostponeTargetConsumed (ITargetBlock<T> targetBlock,
		                                      DataflowMessageHeader header)
		{
			Target target = targetDictionary [targetBlock];

			target.MessageSent ();
			unpostponedTargets.Enqueue (Tuple.Create (target, header));

			target.Postponed.Value = false;
			target.Reserved.Value = false;
		}

		public void UnpostponeTargetNotConsumed (ITargetBlock<T> targetBlock)
		{
			Target target;
			if (!targetDictionary.TryGetValue (targetBlock, out target))
				return;

			unpostponedTargets.Enqueue (Tuple.Create (target,
				new DataflowMessageHeader ()));

			target.Postponed.Value = false;
			target.Reserved.Value = false;
		}

		protected override void ClearUnpostponed ()
		{
			Tuple<Target, DataflowMessageHeader> ignored;
			while (unpostponedTargets.TryDequeue (out ignored)) {
			}
		}

		protected override bool OfferItemToUnpostponed ()
		{
			Tuple<Target, DataflowMessageHeader> tuple;
			while (unpostponedTargets.TryDequeue (out tuple)) {
				// offer to unconditionaly unpostponed
				// and those that consumed some old value
				if (!tuple.Item1.Disabled
				    && (!tuple.Item2.IsValid || !VerifyHeader (tuple.Item2)))
					OfferItem (tuple.Item1);
			}

			return false;
		}
	}
}