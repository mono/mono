// PredicateBlock.cs
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// This block is used by the version of <see cref="DataflowBlock.LinkTo"/>
	/// that has a predicate to wrap the target block,
	/// so that the predicate can be checked.
	/// </summary>
	class PredicateBlock<T> : ITargetBlock<T> {
		/// <summary>
		/// Wraps the source block of the link.
		/// This is necessary so that the communication from target to source works correctly.
		/// </summary>
		class SourceBlock : ISourceBlock<T> {
			readonly ISourceBlock<T> actualSource;
			readonly PredicateBlock<T> predicateBlock;

			public SourceBlock (ISourceBlock<T> actualSource,
			                    PredicateBlock<T> predicateBlock)
			{
				this.actualSource = actualSource;
				this.predicateBlock = predicateBlock;
			}

			public Task Completion
			{
				get { return actualSource.Completion; }
			}

			public void Complete ()
			{
				actualSource.Complete ();
			}

			public void Fault (Exception exception)
			{
				actualSource.Fault (exception);
			}

			public T ConsumeMessage (DataflowMessageHeader messageHeader,
			                         ITargetBlock<T> target, out bool messageConsumed)
			{
				return actualSource.ConsumeMessage (messageHeader, predicateBlock,
					out messageConsumed);
			}

			public IDisposable LinkTo (ITargetBlock<T> target,
			                           DataflowLinkOptions linkOptions)
			{
				return actualSource.LinkTo (target, linkOptions);
			}

			public void ReleaseReservation (DataflowMessageHeader messageHeader,
			                                ITargetBlock<T> target)
			{
				actualSource.ReleaseReservation (messageHeader, predicateBlock);
			}

			public bool ReserveMessage (DataflowMessageHeader messageHeader,
			                            ITargetBlock<T> target)
			{
				return actualSource.ReserveMessage (messageHeader, predicateBlock);
			}
		}

		readonly ITargetBlock<T> actualTarget;
		readonly Predicate<T> predicate;
		readonly SourceBlock sourceBlock;

		public PredicateBlock (ISourceBlock<T> actualSource,
		                       ITargetBlock<T> actualTarget, Predicate<T> predicate)
		{
			this.actualTarget = actualTarget;
			this.predicate = predicate;
			sourceBlock = new SourceBlock (actualSource, this);
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source,
			bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (consumeToAccept && source == null)
				throw new ArgumentException (
					"consumeToAccept may only be true if provided with a non-null source.",
					"consumeToAccept");

			if (!predicate(messageValue))
				return DataflowMessageStatus.Declined;

			return actualTarget.OfferMessage (messageHeader, messageValue, sourceBlock,
				consumeToAccept);
		}

		public Task Completion {
			get { return actualTarget.Completion; }
		}

		public void Complete ()
		{
			actualTarget.Complete ();
		}

		public void Fault (Exception exception)
		{
			actualTarget.Fault (exception);
		}
	}
}