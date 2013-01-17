// 
// BatchBlockTest.cs
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
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class BatchBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (10);
			var block = new ActionBlock<int[]> (i =>
			{
				array = i;
				evt.Set ();
			});
			buffer.LinkTo<int[]> (block);

			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));

			Assert.IsFalse (evt.Wait (100));

			Assert.IsNull (array);

			Assert.IsTrue (buffer.Post (42));
			Assert.IsTrue (evt.Wait (100));

			Assert.IsNotNull (array);
			CollectionAssert.AreEqual (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 42 }, array);
		}

		[Test]
		public void TriggerBatchTest ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (10);
			var block = new ActionBlock<int[]> (i =>
			{
				array = i;
				evt.Set ();
			});
			buffer.LinkTo (block);

			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));

			buffer.TriggerBatch ();
			evt.Wait ();

			Assert.IsNotNull (array);
			Assert.IsTrue (buffer.Post (42));
			evt.Wait (1600);

			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
				array);
		}

		[Test]
		public void TriggerBatchLateBinding ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (10);
			var block = new ActionBlock<int[]> (i =>
			{
				array = i;
				evt.Set ();
			});

			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));

			buffer.TriggerBatch ();
			buffer.LinkTo (block);

			evt.Wait ();
			Assert.IsNotNull (array);

			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
				array);
		}

		[Test]
		public void LateTriggerBatchKeepCountTest ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (15);
			var block = new ActionBlock<int[]> (i =>
			{
				array = i;
				evt.Set ();
			});

			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));
			buffer.TriggerBatch ();
			Assert.IsTrue (buffer.Post (42));
			buffer.LinkTo (block);

			evt.Wait ();

			Assert.IsNotNull (array);
			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
				array);
		}

		[Test]
		public void TriggerBatchWhenEmpty ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (5,
				new GroupingDataflowBlockOptions { TaskScheduler = scheduler });
			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			int[] batch;
			Assert.IsFalse (block.TryReceive (out batch));
			Assert.IsNull (batch);
		}

		[Test]
		public void NonGreedyBatchWithBoundedCapacityTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (2,
				new GroupingDataflowBlockOptions
				{ Greedy = false, BoundedCapacity = 2, TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block));
			Assert.IsNotNull (source2.LinkTo (block));

			Assert.IsTrue (source1.Post (11));
			Assert.IsTrue (source2.Post (21));

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.Post (12));
			Assert.IsTrue (source2.Post (22));

			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source1.TryReceive (out i));
			Assert.AreEqual (12, i);

			Assert.IsTrue (source1.Post (13));

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 11, 21 }, batch);

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 13, 22 }, batch);
		}

		[Test]
		public void GreedyBatchWithBoundedCapacityTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (3,
				new GroupingDataflowBlockOptions
				{ Greedy = true, BoundedCapacity = 3, TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));
			Assert.IsTrue (block.Post (2));

			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.Post (3));
			Assert.IsFalse (block.Post (4));

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEqual (new[] { 1, 2 }, batch);

			Assert.IsTrue (block.Post (5));
			Assert.IsTrue (block.Post (6));
		}

		[Test]
		public void NonGreedyBatchWithBoundedCapacityTriggerTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (3,
				new GroupingDataflowBlockOptions
				{ Greedy = false, BoundedCapacity = 3, TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block));
			Assert.IsNotNull (source2.LinkTo (block));

			// trigger 2 and then trigger 1 with capacity of 3

			Assert.IsTrue (source1.Post (11));
			Assert.IsTrue (source2.Post (21));
			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.Post (12));
			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			int i;
			Assert.IsFalse (source1.TryReceive (out i));

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 11, 21 }, batch);

			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 12 }, batch);
		}

		[Test]
		public void NonGreedyBatchWithBoundedCapacityTriggerTest2 ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (3,
				new GroupingDataflowBlockOptions
				{ Greedy = false, BoundedCapacity = 3, TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block));
			Assert.IsNotNull (source2.LinkTo (block));

			// trigger 2, then trigger another 2 and then trigger 2 once more
			// while havaing capacity of 3

			Assert.IsTrue (source1.Post (11));
			Assert.IsTrue (source2.Post (21));
			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.Post (12));
			Assert.IsTrue (source2.Post (22));
			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.Post (13));
			Assert.IsTrue (source2.Post (23));
			block.TriggerBatch ();

			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source1.TryReceive (out i));
			Assert.AreEqual (13, i);
			Assert.IsTrue (source2.TryReceive (out i));
			Assert.AreEqual (23, i);

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 11, 21 }, batch);

			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 12, 22 }, batch);

			Assert.IsFalse (block.TryReceive (out batch));
		}

		[Test]
		public void MaxNumberOfGroupsTest ()
		{
			var scheduler = new TestScheduler ();

			var block = new BatchBlock<int> (2,
				new GroupingDataflowBlockOptions
				{ MaxNumberOfGroups = 2, TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));
			Assert.IsTrue (block.Post (2));

			Assert.IsTrue (block.Post (3));
			Assert.IsTrue (block.Post (4));

			Assert.IsFalse (block.Post (5));

			scheduler.ExecuteAll ();

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEqual (new[] { 1, 2 }, batch);

			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEqual (new[] { 3, 4 }, batch);

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void CompletionWithTriggerTest ()
		{
			var block = new BatchBlock<int> (2);

			Assert.IsTrue (block.Post (1));

			block.TriggerBatch ();

			block.Complete ();

			CollectionAssert.AreEqual (new[] { 1 },
				block.Receive (TimeSpan.FromMilliseconds (200)));

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void CompletionWithoutTriggerTest ()
		{
			var block = new BatchBlock<int> (2);

			Assert.IsTrue (block.Post (1));
			Assert.IsTrue (block.Post (2));

			block.Complete ();

			CollectionAssert.AreEqual (new[] { 1, 2 },
				block.Receive (TimeSpan.FromMilliseconds (200)));

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void CompleteTriggersBatchTest ()
		{
			var block = new BatchBlock<int> (2);

			Assert.IsTrue (block.Post (1));

			block.Complete ();

			CollectionAssert.AreEqual (new[] { 1 },
				block.Receive (TimeSpan.FromMilliseconds (200)));

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void NonGreedyCompleteDoesnTriggerBatchTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (2,
				new GroupingDataflowBlockOptions
				{ Greedy = false, TaskScheduler = scheduler });
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsNotNull (source.LinkTo (block));

			Assert.IsTrue (source.Post (1));

			block.Complete ();

			int[] batch;
			Assert.IsFalse (block.TryReceive (out batch));

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void NonGreedyMaxNumberOfGroupsTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (2,
				new GroupingDataflowBlockOptions
				{ MaxNumberOfGroups = 1, Greedy = false, TaskScheduler = scheduler });
			ITargetBlock<int> target = block;
			var source1 = new TestSourceBlock<int> ();
			var source2 = new TestSourceBlock<int> ();

			var header1 = new DataflowMessageHeader (1);
			source1.AddMessage (header1, 11);
			source2.AddMessage (header1, 21);

			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header1, 11, source1, false));
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header1, 21, source2, false));

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.WasConsumed (header1));
			Assert.IsTrue (source2.WasConsumed (header1));

			var header2 = new DataflowMessageHeader (2);
			Assert.AreEqual (DataflowMessageStatus.DecliningPermanently,
				target.OfferMessage (header2, 21, source1, false));

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 11, 21 }, batch);

			Assert.IsTrue (block.Completion.Wait (100));
		}
	}
}