// BatchedJoinBlock.cs
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

using System.Collections.Generic;

namespace System.Threading.Tasks.Dataflow {
	public sealed class BatchedJoinBlock<T1, T2> :
		IReceivableSourceBlock<Tuple<IList<T1>, IList<T2>>> {
		readonly GroupingDataflowBlockOptions options;

		readonly CompletionHelper completionHelper;
		readonly OutgoingQueue<Tuple<IList<T1>, IList<T2>>> outgoing;
		SpinLock batchLock;

		readonly JoinTarget<T1> target1;
		readonly JoinTarget<T2> target2;

		int batchCount;
		long numberOfGroups;
		SpinLock batchCountLock;

		public BatchedJoinBlock (int batchSize)
			: this (batchSize, GroupingDataflowBlockOptions.Default)
		{
		}

		public BatchedJoinBlock (int batchSize,
		                         GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			if (batchSize <= 0)
				throw new ArgumentOutOfRangeException (
					"batchSize", batchSize, "The batchSize must be positive.");
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");
			if (!dataflowBlockOptions.Greedy)
				throw new ArgumentException (
					"Greedy must be true for this dataflow block.", "dataflowBlockOptions");
			if (dataflowBlockOptions.BoundedCapacity != DataflowBlockOptions.Unbounded)
				throw new ArgumentException (
					"BoundedCapacity must be Unbounded or -1 for this dataflow block.",
					"dataflowBlockOptions");

			BatchSize = batchSize;
			options = dataflowBlockOptions;
			completionHelper = CompletionHelper.GetNew (dataflowBlockOptions);

			target1 = new JoinTarget<T1> (
				this, SignalTarget, completionHelper, () => outgoing.IsCompleted,
				dataflowBlockOptions, true, TryAdd);
			target2 = new JoinTarget<T2> (
				this, SignalTarget, completionHelper, () => outgoing.IsCompleted,
				dataflowBlockOptions, true, TryAdd);

			outgoing = new OutgoingQueue<Tuple<IList<T1>, IList<T2>>> (
				this, completionHelper,
				() => target1.Buffer.IsCompleted || target2.Buffer.IsCompleted,
				_ =>
				{
					target1.DecreaseCount ();
					target2.DecreaseCount ();
				}, options);
		}

		public int BatchSize { get; private set; }

		public ITargetBlock<T1> Target1 {
			get { return target1; }
		}

		public ITargetBlock<T2> Target2 {
			get { return target2; }
		}

		/// <summary>
		/// Returns whether a new item can be accepted, and increments a counter if it can.
		/// </summary>
		bool TryAdd ()
		{
			bool lockTaken = false;
			try {
				batchCountLock.Enter (ref lockTaken);

				if (options.MaxNumberOfGroups != -1
				    && numberOfGroups + batchCount / BatchSize >= options.MaxNumberOfGroups)
					return false;

				batchCount++;
				return true;
			} finally {
				if (lockTaken)
					batchCountLock.Exit();
			}
		}

		/// <summary>
		/// Decides whether to create a new batch or not.
		/// </summary>
		void SignalTarget ()
		{
			bool lockTaken = false;
			try {
				batchCountLock.Enter (ref lockTaken);

				if (batchCount < BatchSize)
					return;

				batchCount -= BatchSize;
				numberOfGroups++;
			} finally {
				if (lockTaken)
					batchCountLock.Exit();
			}

			MakeBatch (BatchSize);
		}

		/// <summary>
		/// Creates a batch of the given size and adds the resulting batch to the output queue.
		/// </summary>
		void MakeBatch (int batchSize)
		{
			if (batchSize == 0)
				return;

			var list1 = new List<T1> ();
			var list2 = new List<T2> ();
			
			// lock is necessary here to make sure items are in the correct order
			bool taken = false;
			try {
				batchLock.Enter (ref taken);

				int i = 0;

				T1 item1;
				while (i < batchSize && target1.Buffer.TryTake (out item1)) {
					list1.Add (item1);
					i++;
				}

				T2 item2;
				while (i < batchSize && target2.Buffer.TryTake (out item2)) {
					list2.Add (item2);
					i++;
				}

				if (i < batchSize)
					throw new InvalidOperationException("Unexpected count of items.");
			} finally {
				if (taken)
					batchLock.Exit ();
			}

			var batch = Tuple.Create<IList<T1>, IList<T2>> (list1, list2);

			outgoing.AddData (batch);

			VerifyMaxNumberOfGroups ();
		}

		/// <summary>
		/// Verifies whether <see cref="GroupingDataflowBlockOptions.MaxNumberOfGroups"/>
		/// has been reached. If it did, <see cref="Complete"/>s the block.
		/// </summary>
		void VerifyMaxNumberOfGroups ()
		{
			if (options.MaxNumberOfGroups == -1)
				return;

			bool shouldComplete;

			bool lockTaken = false;
			try {
				batchCountLock.Enter (ref lockTaken);

				shouldComplete = numberOfGroups >= options.MaxNumberOfGroups;
			} finally {
				if (lockTaken)
					batchCountLock.Exit ();
			}

			if (shouldComplete)
				Complete ();
		}

		public Task Completion {
			get { return completionHelper.Completion; }
		}

		public void Complete ()
		{
			target1.Complete ();
			target2.Complete ();
			MakeBatch (batchCount);
			outgoing.Complete ();
		}

		void IDataflowBlock.Fault (Exception exception)
		{
			completionHelper.RequestFault (exception);
		}

		Tuple<IList<T1>, IList<T2>> ISourceBlock<Tuple<IList<T1>, IList<T2>>>.ConsumeMessage (
			DataflowMessageHeader messageHeader,
			ITargetBlock<Tuple<IList<T1>, IList<T2>>> target,
			out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public IDisposable LinkTo (ITargetBlock<Tuple<IList<T1>, IList<T2>>> target,
		                           DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget(target, linkOptions);
		}

		void ISourceBlock<Tuple<IList<T1>, IList<T2>>>.ReleaseReservation (
			DataflowMessageHeader messageHeader,
			ITargetBlock<Tuple<IList<T1>, IList<T2>>> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		bool ISourceBlock<Tuple<IList<T1>, IList<T2>>>.ReserveMessage (
			DataflowMessageHeader messageHeader,
			ITargetBlock<Tuple<IList<T1>, IList<T2>>> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<Tuple<IList<T1>, IList<T2>>> filter,
		                        out Tuple<IList<T1>, IList<T2>> item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<Tuple<IList<T1>, IList<T2>>> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public int OutputCount {
			get { return outgoing.Count; }
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, options);
		}
	}
}