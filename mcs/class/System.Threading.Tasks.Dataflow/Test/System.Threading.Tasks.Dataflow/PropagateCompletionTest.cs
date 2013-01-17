// PropagateCompletionTest.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class PropagateCompletionTest {
		[Test]
		public void PropagateCompletionSimpleTest ()
		{
			var source = new BufferBlock<int> ();
			var target = new BufferBlock<int> ();
			Assert.IsNotNull (source.LinkTo (target,
				new DataflowLinkOptions { PropagateCompletion = true }));

			Assert.IsFalse (target.Completion.Wait (100));
			source.Complete ();
			Assert.IsTrue (target.Completion.Wait (100));
		}

		[Test]
		public void PropagateFaultTest ()
		{
			ISourceBlock<int> source = new BufferBlock<int> ();
			var target = new BufferBlock<int> ();
			Assert.IsNotNull (source.LinkTo (target,
				new DataflowLinkOptions { PropagateCompletion = true }));

			Assert.IsFalse (target.Completion.Wait (100));
			var exception = new Exception ();
			source.Fault (exception);

			var ae =
				AssertEx.Throws<AggregateException> (() => source.Completion.Wait (100));
			Assert.AreEqual (exception, ae.Flatten ().InnerException);

			ae = AssertEx.Throws<AggregateException> (() => target.Completion.Wait (100));
			Assert.AreEqual (exception, ae.Flatten ().InnerException);
		}

		[Test]
		public void PropagateCancellationTest ()
		{
			var tokenSource = new CancellationTokenSource ();
			var source = new BufferBlock<int> (
				new DataflowBlockOptions { CancellationToken = tokenSource.Token });
			var target = new BufferBlock<int> ();
			Assert.IsNotNull (source.LinkTo (target,
				new DataflowLinkOptions { PropagateCompletion = true }));

			Assert.IsFalse (target.Completion.Wait (100));
			tokenSource.Cancel ();

			var ae =
				AssertEx.Throws<AggregateException> (() => source.Completion.Wait (100));
			Assert.IsInstanceOfType (
				typeof(TaskCanceledException), ae.Flatten ().InnerException);

			Assert.IsTrue (target.Completion.Wait (100));
		}

		[Test]
		public void PropagateCompletionAfterWaitTest ()
		{
			var evt = new ManualResetEventSlim ();

			var source = new TransformBlock<int, int> (
				i =>
				{
					evt.Wait ();
					return i;
				});
			var target = new BufferBlock<int> ();
			Assert.IsNotNull (source.LinkTo (target,
				new DataflowLinkOptions { PropagateCompletion = true }));

			Assert.IsTrue (source.Post (42));

			Assert.IsFalse (target.Completion.Wait (100));

			source.Complete ();
			Assert.IsFalse (target.Completion.Wait (100));

			evt.Set ();
			Assert.IsFalse (target.Completion.Wait (100));

			Assert.AreEqual (42, target.Receive ());
			Assert.IsTrue (target.Completion.Wait (100));
		}

		[Test]
		public void PropagateCompletionLinkDisposeTest()
		{
			var source = new BufferBlock<int>();
			var target = new BufferBlock<int>();
			var link = source.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });

			Assert.IsFalse(target.Completion.Wait(100));

			link.Dispose();
			source.Complete();
			Assert.IsFalse(target.Completion.Wait(100));
		}
 
	}
}