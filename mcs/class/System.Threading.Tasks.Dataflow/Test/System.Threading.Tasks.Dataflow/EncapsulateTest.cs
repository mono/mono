// EncapsulateTest.cs
//  
// Author:
//       Petr Onderka <gsvick@gmail.com>
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

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class EncapsulateTest {
		[Test]
		public void CompletionTest ()
		{
			var target = new CompletionCheckerBlock<int, int> ();
			var source = new CompletionCheckerBlock<int, int> ();

			var encapsulated = DataflowBlock.Encapsulate (target, source);

			Assert.AreSame (source.Completion, encapsulated.Completion);
		}

		[Test]
		public void CompleteTest ()
		{
			var target = new CompletionCheckerBlock<int, int> ();
			var source = new CompletionCheckerBlock<int, int> ();

			var encapsulated = DataflowBlock.Encapsulate (target, source);

			encapsulated.Complete ();

			Assert.IsFalse (source.WasCompleted);
			Assert.IsTrue (target.WasCompleted);
			Assert.AreSame (source.Completion, encapsulated.Completion);
		}

		[Test]
		public void FaultTest ()
		{
			var target = new CompletionCheckerBlock<int, int> ();
			var source = new CompletionCheckerBlock<int, int> ();

			var encapsulated = DataflowBlock.Encapsulate (target, source);

			encapsulated.Fault (new Exception ());
			Assert.IsFalse (source.WasFaulted);
			Assert.IsTrue (target.WasFaulted);
			Assert.AreSame (source.Completion, encapsulated.Completion);
		}
	}

	class CompletionCheckerBlock<TInput, TOutput> :
		IPropagatorBlock<TInput, TOutput> {
		readonly Task completion = Task.FromResult (true);

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TInput messageValue,
			ISourceBlock<TInput> source, bool consumeToAccept)
		{
			throw new NotImplementedException ();
		}

		public void Complete ()
		{
			WasCompleted = true;
		}

		public bool WasCompleted { get; private set; }

		public void Fault (Exception exception)
		{
			WasFaulted = true;
		}

		public bool WasFaulted { get; private set; }

		public Task Completion
		{
			get { return completion; }
		}

		public IDisposable LinkTo (ITargetBlock<TOutput> target,
		                           DataflowLinkOptions linkOptions)
		{
			throw new NotImplementedException ();
		}

		public TOutput ConsumeMessage (DataflowMessageHeader messageHeader,
		                               ITargetBlock<TOutput> target,
		                               out bool messageConsumed)
		{
			throw new NotImplementedException ();
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader,
		                            ITargetBlock<TOutput> target)
		{
			throw new NotImplementedException ();
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader,
		                                ITargetBlock<TOutput> target)
		{
			throw new NotImplementedException ();
		}
	}
}