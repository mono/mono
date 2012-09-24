// PropagatorWrapperBlock.cs
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// Block returned by <see cref="DataflowBlock.Encapsulate{TInput,TOutput}"/>.
	/// </summary>
	class PropagatorWrapperBlock<TInput, TOutput> :
		IPropagatorBlock<TInput, TOutput> {
		readonly ITargetBlock<TInput> targetBlock;
		readonly ISourceBlock<TOutput> sourceBlock;

		public PropagatorWrapperBlock (
			ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
		{
			if (target == null)
				throw new ArgumentNullException ("target");
			if (source == null)
				throw new ArgumentNullException ("source");

			targetBlock = target;
			sourceBlock = source;
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TInput messageValue,
			ISourceBlock<TInput> source, bool consumeToAccept)
		{
			return targetBlock.OfferMessage (
				messageHeader, messageValue, source, consumeToAccept);
		}

		public TOutput ConsumeMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target,
			out bool messageConsumed)
		{
			return sourceBlock.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public IDisposable LinkTo (
			ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
		{
			return sourceBlock.LinkTo (target, linkOptions);
		}

		public void ReleaseReservation (
			DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			sourceBlock.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (
			DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			return sourceBlock.ReserveMessage (messageHeader, target);
		}

		public void Complete ()
		{
			targetBlock.Complete ();
		}

		public void Fault (Exception exception)
		{
			targetBlock.Fault (exception);
		}

		public Task Completion {
			get { return sourceBlock.Completion; }
		}
	}
}