// PropagatorWrapperBlock.cs
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


using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	internal class PropagatorWrapperBlock<TInput, TOutput> : IPropagatorBlock<TInput, TOutput>
	{
		ITargetBlock<TInput> target;
		ISourceBlock<TOutput> source;
		CompletionHelper compHelper = CompletionHelper.GetNew ();

		public PropagatorWrapperBlock (ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
		{
			this.target = target;
			this.source = source;
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TInput messageValue,
		                                           ISourceBlock<TInput> source,
		                                           bool consumeToAccept)
		{
			return target.OfferMessage (messageHeader, messageValue, source, consumeToAccept);
		}

		public TOutput ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
		{
			return source.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public IDisposable LinkTo (ITargetBlock<TOutput> target, bool unlinkAfterOne)
		{
			return source.LinkTo (target, unlinkAfterOne);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			source.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
		{
			return source.ReserveMessage (messageHeader, target);
		}

		public void Complete ()
		{
			compHelper.Complete ();
			source.Complete ();
			target.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.Fault (ex);
			source.Fault (ex);
			target.Fault (ex);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}
	}
}

