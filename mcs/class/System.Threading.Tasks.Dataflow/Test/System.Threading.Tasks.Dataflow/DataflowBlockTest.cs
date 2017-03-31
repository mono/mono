// DataflowBlockTest.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
//       Petr Onderka <gsvick@gmail.com>
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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class DataflowBlockTest {
		[Test]
		public void TryReceiveTest ()
		{
			var block = new BufferBlock<int> ();
			int value = -1;

			block.Post (42);
			Thread.Sleep (500);
			Assert.IsTrue (block.TryReceive (out value));
			Assert.AreEqual (42, value);
		}

		[Test]
		public void ReceiveTest ()
		{
			var block = new BufferBlock<int> ();
			Task.Factory.StartNew (() => { Thread.Sleep (300); block.Post (42); });
			Assert.AreEqual (42, block.Receive ());
		}

		[Test]
		public void ReceiveCompletedTest ()
		{
			var block = new BufferBlock<int> ();
			block.Complete ();
			AssertEx.Throws<InvalidOperationException> (
				() => block.Receive (TimeSpan.FromMilliseconds (1000)));
		}

		[Test]
		public void ReceiveTimeoutTest ()
		{
			var block = new BufferBlock<int> ();
			AssertEx.Throws<TimeoutException> (
				() => block.Receive (TimeSpan.FromMilliseconds (1000)));
		}

		[Test]
		public void ReceiveCancelledTest ()
		{
			var block = new BufferBlock<int> ();
			var tokenSource = new CancellationTokenSource (200);

			AssertEx.Throws<OperationCanceledException> (
				() => block.Receive (tokenSource.Token));
		}

		[Test]
		public void AsyncReceiveTest ()
		{
			int result = -1;
			var mre = new ManualResetEventSlim (false);

			var block = new WriteOnceBlock<int> (null);
			block.ReceiveAsync ().ContinueWith (i =>
			{
				result = i.Result;
				mre.Set ();
			});
			Task.Factory.StartNew (() =>
			{
				Thread.Sleep (100);
				block.Post (42);
			});
			Assert.IsTrue (mre.Wait (1000));

			Assert.AreEqual (42, result);
		}

		[Test]
		public void AsyncReceiveTestCanceled ()
		{
			var src = new CancellationTokenSource ();

			var block = new WriteOnceBlock<int> (null);
			var task = block.ReceiveAsync (src.Token);
			Task.Factory.StartNew (() =>
			{
				Thread.Sleep (800);
				block.Post (42);
			});
			Thread.Sleep (50);
			src.Cancel ();

			AggregateException ex = null;

			try {
				task.Wait ();
			} catch (AggregateException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.IsNotNull (ex.InnerException);
			Assert.IsInstanceOfType (typeof(TaskCanceledException),
				ex.InnerException);
			Assert.IsTrue (task.IsCompleted);
			Assert.AreEqual (TaskStatus.Canceled, task.Status);
		}

		[Test]
		public void SendAsyncAcceptedTest ()
		{
			var target = new BufferBlock<int> ();
			var task = target.SendAsync (1);

			Assert.IsTrue (task.Wait (0));
			Assert.IsTrue (task.Result);
		}

		[Test]
		public void SendAsyncDeclinedTest ()
		{
			var target = new BufferBlock<int> ();
			target.Complete ();
			var task = target.SendAsync (1);

			Assert.IsTrue (task.Wait (0));
			Assert.IsFalse (task.Result);
		}

		[Test]
		public void SendAsyncPostponedAcceptedTest ()
		{
			var target =
				new BufferBlock<int> (new DataflowBlockOptions { BoundedCapacity = 1 });

			Assert.IsTrue (target.Post (1));

			var task = target.SendAsync (1);

			Assert.IsFalse (task.Wait (100));

			Assert.AreEqual (1, target.Receive ());

			Assert.IsTrue (task.Wait (1000));
			Assert.IsTrue (task.Result);
		}

		[Test]
		public void SendAsyncPostponedDeclinedTest ()
		{
			var target =
				new BufferBlock<int> (new DataflowBlockOptions { BoundedCapacity = 1 });

			Assert.IsTrue (target.Post (1));

			var task = target.SendAsync (1);

			Assert.IsFalse (task.Wait (100));

			target.Complete ();

			Assert.IsTrue (task.Wait (1000));
			Assert.IsFalse (task.Result);
		}

		[Test]
		public void LinkToPredicateTest ()
		{
			var scheduler = new TestScheduler ();
			var source = new BufferBlock<int> (
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> ();
			source.LinkTo (target, i => i % 2 == 1);

			Assert.IsTrue (source.Post (1));
			Assert.IsTrue (source.Post (2));
			Assert.IsTrue (source.Post (3));

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (1, item);
			Assert.IsFalse (target.TryReceive (out item));

			Assert.IsTrue (source.TryReceive (out item));
			Assert.AreEqual (2, item);

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (3, item);
		}

		[Test]
		public void LinkToPredicateMaxMessagesTest ()
		{
			var scheduler = new TestScheduler ();
			var source = new BufferBlock<int> (
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> ();
			source.LinkTo (target, new DataflowLinkOptions { MaxMessages = 1 },
				i => i % 2 == 1);

			Assert.IsTrue (source.Post (2));
			Assert.IsTrue (source.Post (1));
			Assert.IsTrue (source.Post (3));

			scheduler.ExecuteAll ();

			int item;
			Assert.IsFalse (target.TryReceive (out item));
			Assert.IsTrue (source.TryReceive (out item));
			Assert.AreEqual (2, item);

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (1, item);

			scheduler.ExecuteAll ();
			
			Assert.IsFalse (target.TryReceive (out item));
		}

		[Test]
		public void LinkToPredicatePostponed ()
		{
			var scheduler = new TestScheduler ();
			var source = new BufferBlock<int> (
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> (
				new DataflowBlockOptions { BoundedCapacity = 1, TaskScheduler = scheduler });
			source.LinkTo (target, i => true);

			Assert.IsTrue (target.Post (1));
			Assert.IsTrue (source.Post (2));

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (1, item);

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (2, item);
		}

		[Test]
		public void LinkToPredicateClonerTest ()
		{
			var scheduler = new TestScheduler ();
			var source = new BroadcastBlock<int> (i => i * 10,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> ();
			source.LinkTo (target, i => i < 10);

			Assert.IsTrue (source.Post (1));

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (10, item);
		}

		[Test]
		public void NullTargetTest ()
		{
			var target = DataflowBlock.NullTarget<int> ();
			Assert.IsTrue (target.Post (1));

			var source = new TestSourceBlock<int> ();
			var header = new DataflowMessageHeader (1);
			source.AddMessage (header, 2);

			Assert.IsFalse (source.WasConsumed (header));

			Assert.AreEqual (DataflowMessageStatus.Accepted,
				target.OfferMessage (header, 2, source, true));
			Assert.IsTrue (source.WasConsumed (header));

			Assert.IsFalse (target.Completion.Wait (100));

			target.Complete ();

			Assert.IsFalse (target.Completion.Wait (100));

			target.Fault (new Exception ());

			Assert.IsFalse (target.Completion.Wait (100));
		}
	}
}
