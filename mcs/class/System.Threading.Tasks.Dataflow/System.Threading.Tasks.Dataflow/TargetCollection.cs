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
	/// Base class for collection of target blocks for a source block.
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

			/// <summary>
			/// Is called after a message was sent,  makes sure the linked is destroyed after
			/// <see cref="DataflowLinkOptions.MaxMessages"/> were sent.
			/// </summary>
			public void MessageSent()
			{
				if (remainingMessages != -1)
					remainingMessages--;
				if (remainingMessages == 0)
					Dispose ();
			}

			readonly AtomicBoolean disabled = new AtomicBoolean ();
			/// <summary>
			/// Is the link destroyed?
			/// </summary>
			public bool Disabled
			{
				get { return disabled.Value; }
			}

			/// <summary>
			/// Destroys the link to this target.
			/// </summary>
			public void Dispose ()
			{
				disabled.Value = true;

				if (cancellationTokenSource != null)
					cancellationTokenSource.Cancel ();

				Target ignored;
				targetCollection.TargetDictionary.TryRemove (TargetBlock, out ignored);

				// to avoid memory leak; it could take a long time
				// before this object is actually removed from the collection
				TargetBlock = null;
			}

			/// <summary>
			/// Does this target have a postponed message?
			/// </summary>
			public AtomicBoolean Postponed { get; private set; }
			
			/// <summary>
			/// Does this target have a reserved message?
			/// </summary>
			/// <remarks>Used only by broadcast blocks.</remarks>
			public AtomicBoolean Reserved { get; private set; }
		}

		readonly ISourceBlock<T> block;
		readonly bool broadcast;
		readonly bool consumeToAccept;

		readonly ConcurrentQueue<Target> prependQueue = new ConcurrentQueue<Target> ();
		readonly ConcurrentQueue<Target> appendQueue = new ConcurrentQueue<Target> ();
		readonly LinkedList<Target> targets = new LinkedList<Target> ();

		protected readonly ConcurrentDictionary<ITargetBlock<T>, Target> TargetDictionary =
			new ConcurrentDictionary<ITargetBlock<T>, Target> ();

		// lastMessageHeaderId will be always accessed only from one thread
		long lastMessageHeaderId;
		// currentMessageHeaderId can be read from multiple threads at the same time
		long currentMessageHeaderId;

		bool firstOffering;
		T currentItem;

		protected TargetCollectionBase (ISourceBlock<T> block, bool broadcast, bool consumeToAccept)
		{
			this.block = block;
			this.broadcast = broadcast;
			this.consumeToAccept = consumeToAccept;
		}

		/// <summary>
		/// Adds a target block to send messages to.
		/// </summary>
		/// <returns>
		/// An object that can be used to destroy the link to the added target.
		/// </returns>
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
			TargetDictionary [targetBlock] = target;
			if (options.Append)
				appendQueue.Enqueue (target);
			else
				prependQueue.Enqueue (target);

			return target;
		}

		/// <summary>
		/// Sets the current item to be offered to targets
		/// </summary>
		public void SetCurrentItem (T item)
		{
			firstOffering = true;
			currentItem = item;
			Thread.VolatileWrite (ref currentMessageHeaderId, ++lastMessageHeaderId);

			ClearUnpostponed ();
		}

		/// <summary>
		/// Clears the collection of "unpostponed" targets.
		/// </summary>
		protected abstract void ClearUnpostponed ();

		/// <summary>
		/// Resets the current item to be offered to targets.
		/// This means there is currently nothing to offer.
		/// </summary>
		public void ResetCurrentItem ()
		{
			currentItem = default(T);
			Thread.VolatileWrite (ref currentMessageHeaderId, 0);
		}

		/// <summary>
		/// Is there an item to send right now?
		/// </summary>
		public bool HasCurrentItem {
			get { return Thread.VolatileRead (ref currentMessageHeaderId) != 0; }
		}

		/// <summary>
		/// Offers the current item to all eligible targets.
		/// </summary>
		/// <returns>Was the item accepted? (Always <c>false</c> for broadcast blocks.)</returns>
		public bool OfferItemToTargets ()
		{
			// is there an item to offer?
			if (!HasCurrentItem)
				return false;

			var old = Tuple.Create (targets.First, targets.Last);

			do {
				// order is important here, we want to make sure that prepended target
				// added before appended target is processed first
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

		/// <summary>
		/// Are there any targets that currently require a message to be sent to them?
		/// </summary>
		public bool NeedsProcessing {
			get {
				return !appendQueue.IsEmpty || !prependQueue.IsEmpty
				       || !UnpostponedIsEmpty;
			}
		}

		/// <summary>
		/// Is the collection of unpostponed targets empty?
		/// </summary>
		protected abstract bool UnpostponedIsEmpty { get; }

		/// <summary>
		/// Prepends (appends) targets that should be prepended (appended) to the collection of targets.
		/// </summary>
		/// <param name="prepend"><c>true</c> to prepend, <c>false</c> to append.</param>
		/// <returns>
		/// Nodes that contain first and last target added to the list,
		/// or <c>null</c> if no nodes were added.
		/// </returns>
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

		/// <summary>
		/// Offers the current item to the targets between the given nodes (inclusive).
		/// </summary>
		/// <returns>Was the item accepted? (Always <c>false</c> for broadcast blocks.)</returns>
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

		/// <summary>
		/// Offers the current item to unpostponed targets.
		/// </summary>
		/// <returns>Was the item accepted? (Always <c>false</c> for broadcast blocks.)</returns>
		protected abstract bool OfferItemToUnpostponed ();

		/// <summary>
		/// Offers the current item to the given target.
		/// </summary>
		/// <returns>Was the item accepted?</returns>
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

		/// <summary>
		/// Returns whether the given header corresponds to the current item.
		/// </summary>
		public bool VerifyHeader (DataflowMessageHeader header)
		{
			return header.Id == Thread.VolatileRead (ref currentMessageHeaderId);
		}
	}

	/// <summary>
	/// Target collection for non-broadcast blocks.
	/// </summary>
	class TargetCollection<T> : TargetCollectionBase<T> {
		readonly ConcurrentQueue<Target> unpostponedTargets =
			new ConcurrentQueue<Target> ();

		public TargetCollection (ISourceBlock<T> block)
			: base (block, false, false)
		{
		}

		/// <summary>
		/// Is the collection of unpostponed targets empty?
		/// </summary>
		protected override bool UnpostponedIsEmpty {
			get { return unpostponedTargets.IsEmpty; }
		}

		/// <summary>
		/// Returns whether the given header corresponds to the current item
		/// and that the given target block postponed this item.
		/// </summary>
		public bool VerifyHeader (DataflowMessageHeader header, ITargetBlock<T> targetBlock)
		{
			return VerifyHeader (header)
			       && TargetDictionary[targetBlock].Postponed.Value;
		}

		/// <summary>
		/// Unpostpones the given target.
		/// </summary>
		/// <param name="targetBlock">Target to unpostpone.</param>
		/// <param name="messageConsumed">Did the target consume an item?</param>
		public void UnpostponeTarget (ITargetBlock<T> targetBlock, bool messageConsumed)
		{
			Target target;
			if (!TargetDictionary.TryGetValue (targetBlock, out target))
				return;

			if (messageConsumed)
				target.MessageSent ();
			unpostponedTargets.Enqueue (target);

			target.Postponed.Value = false;
		}

		/// <summary>
		/// Clears the collection of "unpostponed" targets.
		/// </summary>
		protected override void ClearUnpostponed ()
		{
			Target ignored;
			while (unpostponedTargets.TryDequeue (out ignored)) {
			}
		}

		/// <summary>
		/// Offers the current item to unpostponed targets.
		/// </summary>
		/// <returns>Was the item accepted?</returns>
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

	/// <summary>
	/// Target collection for broadcast blocks.
	/// </summary>
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

		/// <summary>
		/// Is the collection of unpostponed targets empty?
		/// </summary>
		protected override bool UnpostponedIsEmpty {
			get { return unpostponedTargets.IsEmpty; }
		}

		/// <summary>
		/// Marks the target as having a reserved message.
		/// </summary>
		public void ReserveTarget (ITargetBlock<T> targetBlock)
		{
			TargetDictionary [targetBlock].Reserved.Value = true;
		}

		/// <summary>
		/// Unpostpone target after it consumed a message.
		/// </summary>
		/// <param name="targetBlock">The target to unpostpone.</param>
		/// <param name="header">Header of the message the target consumed.</param>
		public void UnpostponeTargetConsumed (ITargetBlock<T> targetBlock,
		                                      DataflowMessageHeader header)
		{
			Target target = TargetDictionary [targetBlock];

			target.MessageSent ();
			unpostponedTargets.Enqueue (Tuple.Create (target, header));

			target.Postponed.Value = false;
			target.Reserved.Value = false;
		}

		/// <summary>
		/// Unpostpone target in the case when it didn't successfuly consume a message.
		/// </summary>
		public void UnpostponeTargetNotConsumed (ITargetBlock<T> targetBlock)
		{
			Target target;
			if (!TargetDictionary.TryGetValue (targetBlock, out target))
				return;

			unpostponedTargets.Enqueue (Tuple.Create (target,
				new DataflowMessageHeader ()));

			target.Postponed.Value = false;
			target.Reserved.Value = false;
		}

		/// <summary>
		/// Clears the collection of "unpostponed" targets.
		/// </summary>
		protected override void ClearUnpostponed ()
		{
			Tuple<Target, DataflowMessageHeader> ignored;
			while (unpostponedTargets.TryDequeue (out ignored)) {
			}
		}

		/// <summary>
		/// Offers the current item to unpostponed targets.
		/// </summary>
		/// <returns>Always <c>false</c>.</returns>
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