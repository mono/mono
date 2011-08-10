// JoinBlock.cs
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
//
//

#if NET_4_0 || MOBILE

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class JoinBlock<T1, T2> : IReceivableSourceBlock<Tuple<T1, T2>>, ISourceBlock<Tuple<T1, T2>>, IDataflowBlock
	{
		static readonly GroupingDataflowBlockOptions defaultOptions = new GroupingDataflowBlockOptions ();

		CompletionHelper compHelper = CompletionHelper.GetNew ();
		GroupingDataflowBlockOptions dataflowBlockOptions;
		TargetBuffer<Tuple<T1, T2>> targets = new TargetBuffer<Tuple<T1, T2>> ();
		MessageVault<Tuple<T1, T2>> vault = new MessageVault<Tuple<T1, T2>> ();
		MessageOutgoingQueue<Tuple<T1, T2>> outgoing;

		JoinTarget<T1> target1;
		JoinTarget<T2> target2;

		DataflowMessageHeader headers;

		public JoinBlock () : this (defaultOptions)
		{

		}

		public JoinBlock (GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.dataflowBlockOptions = dataflowBlockOptions;
			this.target1 = new JoinTarget<T1> (this, SignalArrivalTarget1, new BlockingCollection<T1> (), compHelper);
			this.target2 = new JoinTarget<T2> (this, SignalArrivalTarget2, new BlockingCollection<T2> (), compHelper);
			this.outgoing = new MessageOutgoingQueue<Tuple<T1, T2>> (compHelper, () => target1.Buffer.IsCompleted || target2.Buffer.IsCompleted);
		}

		public IDisposable LinkTo (ITargetBlock<Tuple<T1, T2>> target, bool unlinkAfterOne)
		{
			var result = targets.AddTarget (target, unlinkAfterOne);
			outgoing.ProcessForTarget (target, this, false, ref headers);
			return result;
		}

		public bool TryReceive (Predicate<Tuple<T1, T2>> filter, out Tuple<T1, T2> item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<Tuple<T1, T2>> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public Tuple<T1, T2> ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target, out bool messageConsumed)
		{
			return vault.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public void Complete ()
		{
			outgoing.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.Fault (ex);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		void SignalArrivalTarget1 ()
		{
			T2 val2;
			if (target2.Buffer.TryTake (out val2)) {
				T1 val1 = target1.Buffer.Take ();
				TriggerMessage (val1, val2);
			}
		}

		void SignalArrivalTarget2 ()
		{
			T1 val1;
			if (target1.Buffer.TryTake (out val1)) {
				T2 val2 = target2.Buffer.Take ();
				TriggerMessage (val1, val2);
			}
		}

		void TriggerMessage (T1 val1, T2 val2)
		{
			Tuple<T1, T2> tuple = Tuple.Create (val1, val2);
			ITargetBlock<Tuple<T1, T2>> target = targets.Current;

			if (target == null) {
				outgoing.AddData (tuple);
			} else {
				target.OfferMessage (headers.Increment (),
				                     tuple,
				                     this,
				                     false);
			}

			if (!outgoing.IsEmpty && (target = targets.Current) != null)
				outgoing.ProcessForTarget (target, this, false, ref headers);
		}

		class JoinTarget<TTarget> : MessageBox<TTarget>, ITargetBlock<TTarget>
		{
			JoinBlock<T1, T2> joinBlock;
			BlockingCollection<TTarget> buffer;
			Action signal;

			public JoinTarget (JoinBlock<T1, T2> joinBlock, Action signal, BlockingCollection<TTarget> buffer, CompletionHelper helper)
			: base (buffer, helper, () => joinBlock.outgoing.IsCompleted)
			{
				this.joinBlock = joinBlock;
				this.buffer = buffer;
				this.signal = signal;
			}

			protected override void EnsureProcessing ()
			{
				signal ();
			}

			public BlockingCollection<TTarget> Buffer {
				get {
					return buffer;
				}
			}

			DataflowMessageStatus ITargetBlock<TTarget>.OfferMessage (DataflowMessageHeader messageHeader,
			                                                          TTarget messageValue,
			                                                          ISourceBlock<TTarget> source,
			                                                          bool consumeToAccept)
			{
				return OfferMessage (this, messageHeader, messageValue, source, consumeToAccept);
			}

			void IDataflowBlock.Complete ()
			{
				Complete ();
			}

			Task IDataflowBlock.Completion {
				get {
					return joinBlock.Completion;
				}
			}

			void IDataflowBlock.Fault (Exception e)
			{
				joinBlock.Fault (e);
			}
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
	}
}

#endif
