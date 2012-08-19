// DataflowBlockTest.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class ChooseTest {
		[Test]
		public void BasicTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();

			bool action1 = false;
			bool action2 = false;
			var completion = DataflowBlock.Choose (
				source1, _ => action1 = true,
				source2, _ => action2 = true);

			source1.Post (42);

			Assert.IsTrue (completion.Wait (500));
			Assert.AreEqual (0, completion.Result);
			Assert.IsTrue (action1);
			Assert.IsFalse (action2);
		}

		[Test]
		public void OnlyOneConsumedTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();

			int action1 = 0;
			int action2 = 0;
			var completion = DataflowBlock.Choose (
				source1, _ => action1++,
				source2, _ => action2++);

			source1.Post (42);
			source1.Post (43);

			Assert.IsTrue (completion.Wait (500));
			Assert.AreEqual (0, completion.Result);
			Assert.AreEqual (1, action1);
			Assert.AreEqual (0, action2);

			int item;
			Assert.IsTrue (source1.TryReceive (out item));
			Assert.AreEqual (43, item);
		}

		[Test]
		public void RaceTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<int> ();

			int action1 = 0;
			int action2 = 0;
			var completion = DataflowBlock.Choose (
				source1, _ => action1++,
				source2, _ => action2++);

			var barrier = new Barrier (2);

			var t1 = Task.Run (() =>
			{
				barrier.SignalAndWait ();
				source1.Post (10);
			});
			var t2 = Task.Run (() =>
			{
				barrier.SignalAndWait ();
				source2.Post (20);
			});

			Task.WaitAll (t1, t2);

			Assert.IsTrue (completion.Wait (500));
			Assert.AreEqual (1, action1 + action2);

			int item;
			Assert.IsTrue (source1.TryReceive (out item) || source2.TryReceive (out item));
		}

		[Test]
		public void BlockCompletionTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();

			var completion = DataflowBlock.Choose (
				source1, _ => { }, source2, _ => { });

			Assert.IsFalse (completion.IsCanceled);

			((IDataflowBlock)source1).Fault (new Exception ());
			source2.Complete ();

			Thread.Sleep (100);

			Assert.IsTrue (completion.IsCanceled);
		}

		[Test]
		public void CancellationTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();

			var tokenSource = new CancellationTokenSource ();
			var options = new DataflowBlockOptions
			{ CancellationToken = tokenSource.Token };

			var completion = DataflowBlock.Choose (
				source1, _ => { }, source2, _ => { }, options);

			Assert.IsFalse (completion.IsCanceled);

			tokenSource.Cancel ();

			Thread.Sleep (100);

			Assert.IsTrue (completion.IsCanceled);
		}

		[Test]
		public void ConsumeToAcceptTest ()
		{
			var source1 = new BroadcastBlock<int> (_ => 42);
			var source2 = new BufferBlock<int> ();

			int action1 = 0;
			int action2 = 0;
			var completion = DataflowBlock.Choose (
				source1, i => action1 = i,
				source2, i => action2 = i);

			source1.Post (10);

			Assert.IsTrue (completion.Wait (500));
			Assert.AreEqual (0, completion.Result);
			Assert.AreEqual (42, action1);
			Assert.AreEqual (0, action2);
		}

		[Test]
		public void ExceptionTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();

			var exception = new Exception ();
			var completion = DataflowBlock.Choose (
				source1, _ => { throw exception; },
				source2, _ => { });

			source1.Post (42);

			var ae = AssertEx.Throws<AggregateException> (() => completion.Wait (500));
			Assert.AreEqual (1, ae.InnerExceptions.Count);
			Assert.AreSame (exception, ae.InnerException);
		}

		[Test]
		public void BasicTest_3 ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();
			var source3 = new BufferBlock<object> ();

			bool action1 = false;
			bool action2 = false;
			bool action3 = false;
			var completion = DataflowBlock.Choose (
				source1, _ => action1 = true,
				source2, _ => action2 = true,
				source3, _ => action3 = true);

			source3.Post (new object ());

			Assert.IsTrue (completion.Wait (500));
			Assert.AreEqual (2, completion.Result);
			Assert.IsFalse (action1);
			Assert.IsFalse (action2);
			Assert.IsTrue (action3);
		}
	}
}