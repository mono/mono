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
	/// This class handles outgoing message that get queued when there is no
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

		public bool IsCompleted {
			get { return Outgoing.IsCompleted; }
		}

		public int Count {
			get { return totalModifiedCount; }
		}

		protected virtual int GetModifiedCount (T data)
		{
			return 1;
		}

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

		protected void EnsureProcessing ()
		{
			ForceProcessing = true;
			if (IsProcessing.TrySet())
				Task.Factory.StartNew (Process, CancellationToken.None,
					TaskCreationOptions.PreferFairness, options.TaskScheduler);
		}

		protected bool ForceProcessing {
			get { return forceProcessing; }
			set { forceProcessing = value; }
		}

		protected abstract void Process ();

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

		protected void VerifyCompleteness ()
		{
			if (Outgoing.IsCompleted && externalCompleteTester ())
				compHelper.Complete ();
		}

		protected void DecreaseCounts (T data)
		{
			var modifiedCount = GetModifiedCount (data);
			Interlocked.Add (ref totalModifiedCount, -modifiedCount);
			Interlocked.Decrement (ref outgoingCount);
			decreaseItemsCount (modifiedCount);
		}

		public void Complete ()
		{
			Outgoing.CompleteAdding ();
			VerifyCompleteness ();
		}
	}
}