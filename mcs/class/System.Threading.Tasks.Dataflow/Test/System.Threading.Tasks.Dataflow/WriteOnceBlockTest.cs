// WriteOnceBlockTest.cs
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
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class WriteOnceBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			bool act1 = false, act2 = false;
			var evt = new CountdownEvent (2);

			var block = new WriteOnceBlock<int> (null);
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

			block.LinkTo (action1);
			block.LinkTo (action2);

			Assert.IsTrue (block.Post (42));
			Assert.IsFalse (block.Post (43));

			Assert.IsTrue (evt.Wait (100));

			Assert.IsTrue (act1);
			Assert.IsTrue (act2);
		}

		[Test]
		public void LinkAfterPostTest ()
		{
			bool act = false;
			var evt = new ManualResetEventSlim ();

			var block = new WriteOnceBlock<int> (null);
			var action = new ActionBlock<int> (i =>
			{
				act = i == 42;
				evt.Set ();
			});

			Assert.IsTrue (block.Post (42));

			block.LinkTo (action);

			Assert.IsTrue (evt.Wait (100));

			Assert.IsTrue (act);
		}

		[Test]
		public void PostponedTest ()
		{
			var block = new WriteOnceBlock<int> (null);
			var target = new BufferBlock<int> (
				new DataflowBlockOptions { BoundedCapacity = 1 });
			block.LinkTo (target);

			Assert.IsTrue (target.Post (1));

			Assert.IsTrue (block.Post (2));

			Assert.AreEqual (1, target.Receive (TimeSpan.FromMilliseconds (100)));
			Assert.AreEqual (2, target.Receive (TimeSpan.FromMilliseconds (100)));
		}

		[Test]
		public void QueuedMessageTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new WriteOnceBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> ();
			block.LinkTo (target);

			Assert.IsTrue (block.Post (1));

			AssertEx.Throws<TimeoutException> (
				() => target.Receive (TimeSpan.FromMilliseconds (100)));

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (out item));
			Assert.AreEqual (1, item);
		}

		[Test]
		public void CloningTest ()
		{
			object act1 = null, act2 = null;
			var evt = new CountdownEvent (2);

			object source = new object ();
			var block = new WriteOnceBlock<object> (o => new object ());
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

			block.LinkTo (action1);
			block.LinkTo (action2);

			Assert.IsTrue (block.Post (source));

			Assert.IsTrue (evt.Wait (100));

			Assert.IsNotNull (act1);
			Assert.IsNotNull (act2);

			Assert.IsFalse (source.Equals (act1));
			Assert.IsFalse (source.Equals (act2));
			Assert.IsFalse (act2.Equals (act1));
		}

		[Test]
		public void WriteOnceBehaviorTest ()
		{
			bool act1 = false, act2 = false;
			var evt = new CountdownEvent (2);

			var broadcast = new WriteOnceBlock<int> (null);
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

			Assert.IsFalse (broadcast.Post (24));
			Thread.Sleep (300);

			Assert.IsTrue (act1);
			Assert.IsTrue (act2);
		}

		[Test]
		public void TryReceiveBehaviorTest ()
		{
			var block = new WriteOnceBlock<int> (null);
			int foo;
			Assert.IsFalse (block.TryReceive (null, out foo));
			block.Post (42);
			Assert.IsTrue (block.TryReceive (null, out foo));
			Assert.AreEqual (42, foo);
			Assert.IsTrue (block.TryReceive (null, out foo));
			Assert.IsFalse (block.TryReceive (i => i == 0, out foo));
			IList<int> bar;
			Assert.IsTrue (((IReceivableSourceBlock<int>)block).TryReceiveAll (out bar));
			CollectionAssert.AreEqual (new[] { 42 }, bar);
		}

		[Test]
		public void DontOfferTwiceTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new WriteOnceBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target =
				new TestTargetBlock<int> { Postpone = true };
			block.LinkTo (target);

			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.HasPostponed);

			target.Postpone = false;

			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
			Assert.AreEqual (1, value);

			block.LinkTo (new BufferBlock<int> ());

			scheduler.ExecuteAll ();

			Assert.AreEqual (default(int), target.DirectlyAccepted);
		}
	}
}