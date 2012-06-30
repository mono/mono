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
//
//

using System.Collections.Generic;

namespace System.Threading.Tasks.Dataflow {
	public sealed class BatchedJoinBlock<T1, T2, T3> :
		IReceivableSourceBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>> {
		readonly GroupingDataflowBlockOptions options;

		CompletionHelper completionHelper;
		readonly MessageOutgoingQueue<Tuple<IList<T1>, IList<T2>, IList<T3>>> outgoing;

		readonly MessageVault<Tuple<IList<T1>, IList<T2>, IList<T3>>> vault =
			new MessageVault<Tuple<IList<T1>, IList<T2>, IList<T3>>> ();

		readonly TargetBuffer<Tuple<IList<T1>, IList<T2>, IList<T3>>> targets =
			new TargetBuffer<Tuple<IList<T1>, IList<T2>, IList<T3>>> ();

		DataflowMessageHeader headers;
		SpinLock batchLock;

		readonly JoinTarget<T1> target1;
		readonly JoinTarget<T2> target2;
		readonly JoinTarget<T3> target3;

		int batchCount;

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

			BatchSize = batchSize;
			options = dataflowBlockOptions;
			completionHelper = CompletionHelper.GetNew (options);

			target1 = new JoinTarget<T1> (
				this, SignalTarget, completionHelper, () => outgoing.IsCompleted);
			target2 = new JoinTarget<T2> (
				this, SignalTarget, completionHelper, () => outgoing.IsCompleted);
			target3 = new JoinTarget<T3>(
				this, SignalTarget, completionHelper, () => outgoing.IsCompleted);

			outgoing = new MessageOutgoingQueue<Tuple<IList<T1>, IList<T2>, IList<T3>>> (
				completionHelper,
				() => target1.Buffer.IsCompleted || target2.Buffer.IsCompleted || target3.Buffer.IsCompleted);
		}

		public int BatchSize { get; private set; }

		public ITargetBlock<T1> Target1 {
			get { return target1; }
		}

		public ITargetBlock<T2> Target2 {
			get { return target2; }
		}

		public ITargetBlock<T3> Target3 {
			get { return target3; }
		}

		void SignalTarget ()
		{
			int current = Interlocked.Increment (ref batchCount);

			if (current % BatchSize != 0)
				return;

			Interlocked.Add (ref batchCount, -current);

			MakeBatch (BatchSize);
		}

		void MakeBatch (int batchSize)
		{
			var list1 = new List<T1> ();
			var list2 = new List<T2> ();
			var list3 = new List<T3> ();

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

				T3 item3;
				while (i < batchSize && target3.Buffer.TryTake (out item3)) {
					list3.Add (item3);
					i++;
				}

				if (i < batchSize)
					throw new InvalidOperationException ("Unexpected count of items.");
			} finally {
				if (taken)
					batchLock.Exit ();
			}

			var batch = Tuple.Create<IList<T1>, IList<T2>, IList<T3>> (list1, list2,
				list3);

			var target = targets.Current;
			if (target == null)
				outgoing.AddData (batch);
			else
				target.OfferMessage (headers.Increment (), batch, this, false);

			if (!outgoing.IsEmpty && targets.Current != null)
				outgoing.ProcessForTarget (targets.Current, this, false, ref headers);
		}

		public Task Completion
		{
			get { return completionHelper.Completion; }
		}

		public void Complete ()
		{
			outgoing.Complete ();
		}

		void IDataflowBlock.Fault (Exception exception)
		{
			completionHelper.Fault (exception);
		}

		Tuple<IList<T1>, IList<T2>, IList<T3>>
			ISourceBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>>.ConsumeMessage (
			DataflowMessageHeader messageHeader,
			ITargetBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>> target,
			out bool messageConsumed)
		{
			return vault.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public IDisposable LinkTo (
			ITargetBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>> target,
			bool unlinkAfterOne)
		{
			var result = targets.AddTarget (target, unlinkAfterOne);
			outgoing.ProcessForTarget (target, this, false, ref headers);
			return result;
		}

		void ISourceBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>>.ReleaseReservation (
			DataflowMessageHeader messageHeader,
			ITargetBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		bool ISourceBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>>.ReserveMessage (
			DataflowMessageHeader messageHeader,
			ITargetBlock<Tuple<IList<T1>, IList<T2>, IList<T3>>> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (
			Predicate<Tuple<IList<T1>, IList<T2>, IList<T3>>> filter,
			out Tuple<IList<T1>, IList<T2>, IList<T3>> item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (
			out IList<Tuple<IList<T1>, IList<T2>, IList<T3>>> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, options);
		}
	}
}