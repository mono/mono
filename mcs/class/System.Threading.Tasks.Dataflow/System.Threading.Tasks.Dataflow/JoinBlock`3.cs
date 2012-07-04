// JoinBlock`3.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
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

namespace System.Threading.Tasks.Dataflow
{
	public sealed class JoinBlock<T1, T2, T3> : IReceivableSourceBlock<Tuple<T1, T2, T3>>
	{
		static readonly GroupingDataflowBlockOptions defaultOptions = new GroupingDataflowBlockOptions ();

		readonly CompletionHelper compHelper;
		readonly GroupingDataflowBlockOptions dataflowBlockOptions;
		readonly MessageOutgoingQueue<Tuple<T1, T2, T3>> outgoing;

		readonly JoinTarget<T1> target1;
		readonly JoinTarget<T2> target2;
		readonly JoinTarget<T3> target3;

		SpinLock targetLock = new SpinLock (false);

		DataflowMessageHeader headers;

		public JoinBlock () : this (defaultOptions)
		{

		}

		public JoinBlock (GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);

			target1 = new JoinTarget<T1> (this, SignalArrivalTargetImpl, compHelper,
				() => outgoing.IsCompleted, dataflowBlockOptions);
			target2 = new JoinTarget<T2> (this, SignalArrivalTargetImpl, compHelper,
				() => outgoing.IsCompleted, dataflowBlockOptions);
			target3 = new JoinTarget<T3> (this, SignalArrivalTargetImpl, compHelper,
				() => outgoing.IsCompleted, dataflowBlockOptions);
			outgoing = new MessageOutgoingQueue<Tuple<T1, T2, T3>> (
				this, compHelper,
				() => target1.Buffer.IsCompleted || target2.Buffer.IsCompleted
				      || target3.Buffer.IsCompleted,
				() =>
				{
					target1.DecreaseCount ();
					target2.DecreaseCount ();
					target3.DecreaseCount ();
				}, dataflowBlockOptions);
		}

		public IDisposable LinkTo (ITargetBlock<Tuple<T1, T2, T3>> target, DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget (target, linkOptions);
		}

		public bool TryReceive (Predicate<Tuple<T1, T2, T3>> filter, out Tuple<T1, T2, T3> item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<Tuple<T1, T2, T3>> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public Tuple<T1, T2, T3> ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2, T3>> target, out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2, T3>> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2, T3>> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		public void Complete ()
		{
			outgoing.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.RequestFault (ex);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		// TODO : see if we can find a lockless implementation
		void SignalArrivalTargetImpl ()
		{
			bool taken = false;
			T1 value1;
			T2 value2;
			T3 value3;

			try {
				targetLock.Enter (ref taken);

				if (target1.Buffer.Count == 0 || target2.Buffer.Count == 0 || target3.Buffer.Count == 0)
					return;

				value1 = target1.Buffer.Take ();
				value2 = target2.Buffer.Take ();
				value3 = target3.Buffer.Take ();
			} finally {
				if (taken)
					targetLock.Exit ();
			}

			TriggerMessage (value1, value2, value3);
		}

		void TriggerMessage (T1 val1, T2 val2, T3 val3)
		{
			outgoing.AddData (Tuple.Create (val1, val2, val3));
		}

		public ITargetBlock<T1> Target1 {
			get {
				return target1;
			}
		}

		public ITargetBlock<T2> Target2 {
			get {
				return target2;
			}
		}

		public ITargetBlock<T3> Target3 {
			get {
				return target3;
			}
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}