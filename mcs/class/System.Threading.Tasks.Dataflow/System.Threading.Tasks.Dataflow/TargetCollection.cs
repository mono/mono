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
	class TargetCollection<T> {
		/// <summary>
		/// Represents a target block with its options.
		/// </summary>
		class Target : IDisposable {
			public Target (ITargetBlock<T> targetBlock, int maxMessages)
			{
				TargetBlock = targetBlock;
				remainingMessages = maxMessages;
			}

			public ITargetBlock<T> TargetBlock { get; private set; }

			volatile int remainingMessages;

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
				// to avoid memory leak; it could take a long time
				// before this object is actually removed from the collection
				TargetBlock = null;
			}
		}

		readonly ISourceBlock<T> block;

		readonly ConcurrentQueue<Target> prependQueue = new ConcurrentQueue<Target> ();
		readonly ConcurrentQueue<Target> appendQueue = new ConcurrentQueue<Target> ();
		readonly LinkedList<Target> targets = new LinkedList<Target> ();
		readonly ConcurrentDictionary<ITargetBlock<T>, Target> postponedTargetBlocks =
			new ConcurrentDictionary<ITargetBlock<T>, Target> ();
		readonly ConcurrentQueue<Target> unpostponedTargets =
			new ConcurrentQueue<Target> ();

		int messageHeaderId;

		bool firstOffering;
		T currentItem;
		DataflowMessageHeader currentHeader;

		public TargetCollection (ISourceBlock<T> block)
		{
			this.block = block;
		}

		public IDisposable AddTarget(ITargetBlock<T> targetBlock, DataflowLinkOptions options)
		{
			var target = new Target (targetBlock, options.MaxMessages);
			if (options.Append)
				appendQueue.Enqueue (target);
			else
				prependQueue.Enqueue (target);
			return target;
		}

		public void SetCurrentItem(T item)
		{
			firstOffering = true;
			currentItem = item;
			currentHeader = new DataflowMessageHeader(++messageHeaderId);

			// clear unpostponed
			Target ignored;
			while (unpostponedTargets.TryDequeue (out ignored)) {
			}
		}

		public void ResetCurrentItem()
		{
			currentItem = default(T);
			currentHeader = new DataflowMessageHeader();
		}

		public bool HasCurrentItem {
			get { return currentHeader.IsValid; }
		}

		public bool OfferItemToTargets()
		{
			// is there an item to offer?
			if (!currentHeader.IsValid)
				return false;

			var old = Tuple.Create (targets.First, targets.Last);

			do {
				// order is important here, we want to make sure that prepended target
				// added after appended target is always processed first
				var appended = PrependOrAppend (false);
				var prepended = PrependOrAppend (true);

				if (OfferItemToTargets(prepended))
					return true;

				if (firstOffering) {
					if (OfferItemToTargets(old))
						return true;
					firstOffering = false;
				} else {
					if (OfferItemToUnpostponed())
						return true;
				}

				if (OfferItemToTargets(appended))
					return true;
			} while (NeedsProcessing);

			return false;
		}

		public bool NeedsProcessing {
			get {
				return !appendQueue.IsEmpty || !prependQueue.IsEmpty
				       || !unpostponedTargets.IsEmpty;
			}
		}

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

				if (OfferItem(node.Value))
					return true;

				node = node.Next;
			}

			return false;
		}

		bool OfferItemToUnpostponed ()
		{
			Target target;
			while (unpostponedTargets.TryDequeue (out target)) {
				if (!target.Disabled && OfferItem(target))
					return true;
			}

			return false;
		}

		bool OfferItem (Target target)
		{
			if (postponedTargetBlocks.ContainsKey (target.TargetBlock))
				return false;

			var result = target.TargetBlock.OfferMessage (
				currentHeader, currentItem, block, false);

			switch (result) {
			case DataflowMessageStatus.Accepted:
				target.MessageSent ();
				return true;
			case DataflowMessageStatus.Postponed:
				postponedTargetBlocks.TryAdd (target.TargetBlock, target);
				return false;
			case DataflowMessageStatus.DecliningPermanently:
				target.Dispose ();
				return false;
			default:
				return false;
			}
		}

		public void UnpostponeTarget (ITargetBlock<T> targetBlock, bool messageConsumed)
		{
			Target target;
			postponedTargetBlocks.TryRemove (targetBlock, out target);
			if (messageConsumed)
				target.MessageSent ();
			unpostponedTargets.Enqueue (target);
		}

		public bool VerifyHeader (DataflowMessageHeader header, ITargetBlock<T> targetBlock)
		{
			return header.IsValid && header == currentHeader
			       && postponedTargetBlocks.ContainsKey (targetBlock);
		}
	}
}