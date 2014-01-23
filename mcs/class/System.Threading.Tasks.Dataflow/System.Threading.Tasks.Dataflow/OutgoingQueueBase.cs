// OutgoingQueueBase.cs
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Handles outgoing messages that get queued when there is no
	/// block on the other end to proces it. It also allows receive operations.
	/// </summary>
	abstract class OutgoingQueueBase<T> {
		protected ConcurrentQueue<T> Store { get; private set; }
		protected BlockingCollection<T> Outgoing { get; private set; }
		int outgoingCount;
		readonly CompletionHelper compHelper;
		readonly Func<bool> externalCompleteTester;
		readonly DataflowBlockOptions options;
		protected AtomicBoolean IsProcessing { get; private set; }
		protected abstract TargetCollectionBase<T> Targets { get; }
		int totalModifiedCount;
		readonly Action<int> decreaseItemsCount;
		volatile bool forceProcessing;

		protected OutgoingQueueBase (
			CompletionHelper compHelper, Func<bool> externalCompleteTester,
			Action<int> decreaseItemsCount, DataflowBlockOptions options)
		{
			IsProcessing = new AtomicBoolean ();
			Store = new ConcurrentQueue<T> ();
			Outgoing = new BlockingCollection<T> (Store);
			this.compHelper = compHelper;
			this.externalCompleteTester = externalCompleteTester;
			this.options = options;
			this.decreaseItemsCount = decreaseItemsCount;
		}

		/// <summary>
		/// Is the queue completed?
		/// Queue is completed after <see cref="Complete"/> is called
		/// and all items are retrieved from it.
		/// </summary>
		public bool IsCompleted {
			get { return Outgoing.IsCompleted; }
		}

		/// <summary>
		/// Current number of items in the queue.
		/// Item are counted the way <see cref="DataflowBlockOptions.BoundedCapacity"/>
		/// counts them, e.g. each item in a batch counts, even if batch is a single object.
		/// </summary>
		public int Count {
			get { return totalModifiedCount; }
		}

		/// <summary>
		/// Calculates the count of items in the given object.
		/// </summary>
		protected virtual int GetModifiedCount (T data)
		{
			return 1;
		}

		/// <summary>
		/// Adds an object to the queue.
		/// </summary>
		public void AddData (T data)
		{
			try {
				Outgoing.Add (data);
				Interlocked.Add (ref totalModifiedCount, GetModifiedCount (data));
				if (Interlocked.Increment (ref outgoingCount) == 1)
					EnsureProcessing ();
			} catch (InvalidOperationException) {
				VerifyCompleteness ();
			}
		}

		/// <summary>
		/// Makes sure sending messages to targets is running.
		/// </summary>
		protected void EnsureProcessing ()
		{
			ForceProcessing = true;
			if (IsProcessing.TrySet())
				Task.Factory.StartNew (Process, CancellationToken.None,
					TaskCreationOptions.PreferFairness, options.TaskScheduler);
		}

		/// <summary>
		/// Indicates whether sending messages should be forced to start.
		/// </summary>
		protected bool ForceProcessing {
			get { return forceProcessing; }
			set { forceProcessing = value; }
		}

		/// <summary>
		/// Sends messages to targets.
		/// </summary>
		protected abstract void Process ();

		/// <summary>
		/// Adds a target block to send messages to.
		/// </summary>
		/// <returns>
		/// An object that can be used to destroy the link to the added target.
		/// </returns>
		public IDisposable AddTarget (ITargetBlock<T> targetBlock, DataflowLinkOptions linkOptions)
		{
			if (targetBlock == null)
				throw new ArgumentNullException ("targetBlock");
			if (linkOptions == null)
				throw new ArgumentNullException ("linkOptions");

			var result = Targets.AddTarget (targetBlock, linkOptions);
			EnsureProcessing ();
			return result;
		}

		/// <summary>
		/// Makes sure the block is completed if it should be.
		/// </summary>
		protected void VerifyCompleteness ()
		{
			if (Outgoing.IsCompleted && externalCompleteTester ())
				compHelper.Complete ();
		}

		/// <summary>
		/// Is the block faulted or cancelled?
		/// </summary>
		protected bool IsFaultedOrCancelled {
			get { return compHelper.Completion.IsFaulted || compHelper.Completion.IsCanceled; }
		}

		/// <summary>
		/// Used to notify that object was removed from the queue
		/// and to update counts.
		/// </summary>
		protected void DecreaseCounts (T data)
		{
			var modifiedCount = GetModifiedCount (data);
			Interlocked.Add (ref totalModifiedCount, -modifiedCount);
			Interlocked.Decrement (ref outgoingCount);
			decreaseItemsCount (modifiedCount);
		}

		/// <summary>
		/// Marks the queue for completion.
		/// </summary>
		public void Complete ()
		{
			Outgoing.CompleteAdding ();
			VerifyCompleteness ();
		}
	}
}