// 
// BroadcastBlockTest.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class BroadcastBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			bool act1 = false, act2 = false;
			var evt = new CountdownEvent (2);

			var broadcast = new BroadcastBlock<int> (null);
			var action1 = new ActionBlock<int> (i =>
			{
				act1 = i == 42;
				evt.Signal ();
			});
			var action2 = new ActionBlock<int> (i =>
			{
				act2 = i == 42;
				evt.Signal ();
			});

			broadcast.LinkTo (action1);
			broadcast.LinkTo (action2);

			Assert.IsTrue (broadcast.Post (42));

			Assert.IsTrue (evt.Wait (100));

			Assert.IsTrue (act1);
			Assert.IsTrue (act2);
		}

		[Test]
		public void LinkAfterPostTest ()
		{
			bool act = false;
			var evt = new ManualResetEventSlim ();

			var broadcast = new BroadcastBlock<int> (null);
			var action = new ActionBlock<int> (i =>
			{
				act = i == 42;
				evt.Set ();
			});

			Assert.IsTrue (broadcast.Post (42));

			broadcast.LinkTo (action);

			Assert.IsTrue (evt.Wait (100));

			Assert.IsTrue (act);
		}

		[Test]
		public void PostponedTest ()
		{
			var broadcast = new BroadcastBlock<int> (null);
			var target = new BufferBlock<int> (
				new DataflowBlockOptions { BoundedCapacity = 1 });
			broadcast.LinkTo (target);

			Assert.IsTrue (target.Post (1));

			Assert.IsTrue (broadcast.Post (2));

			Assert.AreEqual (1, target.Receive (TimeSpan.FromMilliseconds (0)));
			Assert.AreEqual (2, target.Receive (TimeSpan.FromMilliseconds (100)));
		}

		[Test]
		public void ConsumeChangedTest ()
		{
			var scheduler = new TestScheduler ();
			var broadcast = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };

			broadcast.LinkTo (target);

			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (broadcast.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.HasPostponed);

			Assert.IsTrue (broadcast.Post (2));

			scheduler.ExecuteAll ();

			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
			Assert.AreEqual (2, value);
		}

		[Test]
		public void ReserveConsumeChangedTest ()
		{
			var scheduler = new TestScheduler ();
			var broadcast = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };

			broadcast.LinkTo (target);

			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (broadcast.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.HasPostponed);

			Assert.IsTrue (target.ReservePostponed ());

			Assert.IsTrue (broadcast.Post (2));

			scheduler.ExecuteAll ();

			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
			Assert.AreEqual (1, value);
		}

		[Test]
		public void ReserveChangedTest ()
		{
			var scheduler = new TestScheduler ();
			var broadcast = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };

			broadcast.LinkTo (target);

			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (broadcast.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.HasPostponed);

			Assert.IsTrue(broadcast.Post(2));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.ReservePostponed ());

			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
			Assert.AreEqual (2, value);
		}

		[Test]
		public void QueuedMessagesTest ()
		{
			var scheduler = new TestScheduler ();
			var broadcast = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> ();
			broadcast.LinkTo (target);

			Assert.IsTrue (broadcast.Post (1));
			Assert.IsTrue (broadcast.Post (2));

			AssertEx.Throws<TimeoutException> (
				() => target.Receive (TimeSpan.FromMilliseconds (100)));

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (1, item);
			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (2, item);
		}

		[Test]
		public void BoundedQueuedTest ()
		{
			var scheduler = new TestScheduler ();
			var broadcast = new BroadcastBlock<int> (
				null,
				new DataflowBlockOptions { TaskScheduler = scheduler, BoundedCapacity = 1 });

			Assert.IsTrue (broadcast.Post (1));
			Assert.IsFalse (broadcast.Post (2));
		}

		[Test]
		public void BoundedPostponedTest ()
		{
			var scheduler = new TestScheduler ();
			var broadcast = new BroadcastBlock<int> (
				null,
				new DataflowBlockOptions { TaskScheduler = scheduler, BoundedCapacity = 1 });
			ITargetBlock<int> target = broadcast;
			var source = new TestSourceBlock<int> ();

			Assert.IsTrue (broadcast.Post (1));
			var header = new DataflowMessageHeader (1);
			source.AddMessage (header, 2);
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header, 2, source, false));
			Assert.IsFalse (source.WasConsumed (header));

			scheduler.ExecuteAll ();

			Assert.IsTrue (source.WasConsumed (header));
		}

		[Test]
		public void CloningTest ()
		{
			object act1 = null, act2 = null;
			var evt = new CountdownEvent (2);

			object source = new object ();
			var broadcast = new BroadcastBlock<object> (o => new object ());
			var action1 = new ActionBlock<object> (i =>
			{
				act1 = i;
				evt.Signal ();
			});
			var action2 = new ActionBlock<object> (i =>
			{
				act2 = i;
				evt.Signal ();
			});

			broadcast.LinkTo (action1);
			broadcast.LinkTo (action2);

			Assert.IsTrue (broadcast.Post (source));

			Assert.IsTrue (evt.Wait (100));

			Assert.IsNotNull (act1);
			Assert.IsNotNull (act2);

			Assert.IsFalse (source.Equals (act1));
			Assert.IsFalse (source.Equals (act2));
			Assert.IsFalse (act2.Equals (act1));
		}

		[Test]
		public void TryReceiveTest()
		{
			var scheduler = new TestScheduler();
			var block = new BroadcastBlock<int>(i => i * 10, new DataflowBlockOptions { TaskScheduler = scheduler });

			int item;
			Assert.IsFalse(block.TryReceive(null, out item));

			Assert.IsTrue(block.Post(1));
			Assert.IsTrue(block.Post(2));

			scheduler.ExecuteAll();

			Assert.IsTrue(block.TryReceive(null, out item));
			Assert.AreEqual(20, item);
			// predicate is tested on original value, but returned is cloned
			Assert.IsTrue(block.TryReceive(i => i < 10, out item));
			Assert.AreEqual(20, item);
		}

		[Test]
		public void TryReceiveAllTest()
		{
			var scheduler = new TestScheduler();
			var block = new BroadcastBlock<int>(null, new DataflowBlockOptions { TaskScheduler = scheduler });
			IReceivableSourceBlock<int> source = block;

			Assert.IsTrue(block.Post(1));
			Assert.IsTrue(block.Post(2));

			scheduler.ExecuteAll();

			IList<int> items;
			Assert.IsTrue(source.TryReceiveAll(out items));

			CollectionAssert.AreEqual(new[] { 2 }, items);
		}

		[Test]
		public void DontOfferTwiceTest()
		{
			var scheduler = new TestScheduler ();
			var block = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target =
				new TestTargetBlock<int> { Postpone = true };
			block.LinkTo (target);

			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll();

			Assert.IsTrue (target.HasPostponed);

			target.Postpone = false;

			int value;
			Assert.IsTrue(target.RetryPostponed(out value));
			Assert.AreEqual(1, value);

			block.LinkTo(new BufferBlock<int>());

			scheduler.ExecuteAll();

			Assert.AreEqual(default(int), target.DirectlyAccepted);
		}
	}
}