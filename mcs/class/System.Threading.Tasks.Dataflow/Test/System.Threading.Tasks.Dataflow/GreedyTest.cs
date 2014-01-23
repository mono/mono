// GreedyTest.cs
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
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class GreedyTest {
		[Test]
		public void GreedyJoinTest ()
		{
			var scheduler = new TestScheduler ();
			var block =
				new JoinBlock<int, int> (new GroupingDataflowBlockOptions
				{ TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block.Target1));
			Assert.IsNotNull (source2.LinkTo (block.Target2));

			Assert.IsTrue (source1.Post (1));
			scheduler.ExecuteAll ();

			int i;
			Assert.IsFalse (source1.TryReceive (out i));

			Assert.IsTrue (source2.Post (11));
			scheduler.ExecuteAll ();

			Assert.IsFalse (source2.TryReceive (out i));

			Tuple<int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (1, 11), tuple);
		}

		[Test]
		public void GreedyJoin3Test ()
		{
			var scheduler = new TestScheduler ();
			var block =
				new JoinBlock<int, int, int> (new GroupingDataflowBlockOptions
				{ TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source3 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block.Target1));
			Assert.IsNotNull (source2.LinkTo (block.Target2));
			Assert.IsNotNull (source3.LinkTo (block.Target3));

			Assert.IsTrue (source1.Post (1));
			scheduler.ExecuteAll ();

			int i;
			Assert.IsFalse (source1.TryReceive (out i));

			Assert.IsTrue (source2.Post (11));
			Assert.IsTrue (source3.Post (21));
			scheduler.ExecuteAll ();

			Assert.IsFalse (source2.TryReceive (out i));
			Assert.IsFalse (source3.TryReceive (out i));

			Tuple<int, int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (1, 11, 21), tuple);
		}

		[Test]
		public void NonGreedyJoinTest ()
		{
			var scheduler = new TestScheduler ();
			var block =
				new JoinBlock<int, int> (new GroupingDataflowBlockOptions
				{ TaskScheduler = scheduler, Greedy = false });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block.Target1));
			Assert.IsNotNull (source2.LinkTo (block.Target2));

			Assert.IsTrue (source1.Post (1));
			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source1.TryReceive (out i));

			Assert.IsTrue (source1.Post (2));
			Assert.IsTrue (source2.Post (11));
			scheduler.ExecuteAll ();

			Assert.IsFalse (source1.TryReceive (out i));
			Assert.IsFalse (source2.TryReceive (out i));

			Tuple<int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (2, 11), tuple);
		}

		[Test]
		public void NonGreedyJoin3Test ()
		{
			var scheduler = new TestScheduler ();
			var block =
				new JoinBlock<int, int, int> (new GroupingDataflowBlockOptions
				{ TaskScheduler = scheduler, Greedy = false });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source3 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block.Target1));
			Assert.IsNotNull (source2.LinkTo (block.Target2));
			Assert.IsNotNull (source3.LinkTo (block.Target3));

			Assert.IsTrue (source1.Post (1));
			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source1.TryReceive (out i));

			Assert.IsTrue (source1.Post (2));
			Assert.IsTrue (source2.Post (11));
			Assert.IsTrue (source3.Post (21));
			scheduler.ExecuteAll ();

			Assert.IsFalse (source1.TryReceive (out i));
			Assert.IsFalse (source2.TryReceive (out i));
			Assert.IsFalse (source3.TryReceive (out i));

			Tuple<int, int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (2, 11, 21), tuple);
		}

		[Test]
		public void NonGreedyJoinWithPostTest ()
		{
			var block =
				new JoinBlock<int, int> (new GroupingDataflowBlockOptions { Greedy = false });

			Assert.IsFalse (block.Target1.Post (42));
		}

		[Test]
		public void NonGreedyBatchTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (3,
				new GroupingDataflowBlockOptions
				{ Greedy = false, TaskScheduler = scheduler });
			Assert.IsFalse (block.Post (42));

			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source.LinkTo (block));

			Assert.IsTrue (source.Post (43));

			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source.TryReceive (null, out i));
			Assert.AreEqual (43, i);

			Assert.IsTrue (source.Post (44));
			Assert.IsTrue (source.Post (45));

			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source2.LinkTo (block));

			Assert.IsTrue (source2.Post (142));

			scheduler.ExecuteAll ();

			int[] batch;
			Assert.IsFalse (block.TryReceive (null, out batch));
			Assert.IsNull (batch);

			block.TriggerBatch ();
			scheduler.ExecuteAll ();
			Assert.IsTrue (block.TryReceive (null, out batch));
			CollectionAssert.AreEquivalent (new[] { 44, 142 }, batch);
		}

		[Test]
		public void NonGreedyBatchWithMoreSourcesTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchBlock<int> (2,
				new GroupingDataflowBlockOptions
				{ Greedy = false, TaskScheduler = scheduler });

			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block));
			Assert.IsNotNull (source2.LinkTo (block));

			Assert.IsTrue (source1.Post (43));

			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source1.TryReceive (out i));
			Assert.AreEqual (43, i);

			Assert.IsTrue (source1.Post (44));
			Assert.IsTrue (source2.Post (45));

			scheduler.ExecuteAll ();

			int[] batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEquivalent (new[] { 44, 45 }, batch);
		}

		[Test]
		public void NonGreedyBatchedJoinTest ()
		{
			AssertEx.Throws<ArgumentException> (
				() => new BatchedJoinBlock<int, int> (2,
					      new GroupingDataflowBlockOptions { Greedy = false }));
		}

		[Test]
		public void NonGreedyBatchedJoin3Test ()
		{
			AssertEx.Throws<ArgumentException> (
				() => new BatchedJoinBlock<int, int, int> (2,
					      new GroupingDataflowBlockOptions { Greedy = false }));
		}

		[Test]
		public void NonGreedyJoinWithBoundedCapacityTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new JoinBlock<int, int> (
				new GroupingDataflowBlockOptions
				{ Greedy = false, BoundedCapacity = 1, TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block.Target1));
			Assert.IsNotNull (source2.LinkTo (block.Target2));

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

			Tuple<int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (11, 21), tuple);

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (13, 22), tuple);
		}

		[Test]
		public void NonGreedyJoin3WithBoundedCapacityTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new JoinBlock<int, int, int> (
				new GroupingDataflowBlockOptions
				{ Greedy = false, BoundedCapacity = 1, TaskScheduler = scheduler });
			var source1 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source2 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var source3 =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.IsNotNull (source1.LinkTo (block.Target1));
			Assert.IsNotNull (source2.LinkTo (block.Target2));
			Assert.IsNotNull (source3.LinkTo (block.Target3));

			Assert.IsTrue (source1.Post (11));
			Assert.IsTrue (source2.Post (21));
			Assert.IsTrue (source3.Post (31));

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.Post (12));
			Assert.IsTrue (source2.Post (22));
			Assert.IsTrue (source3.Post (32));

			scheduler.ExecuteAll ();

			int i;
			Assert.IsTrue (source1.TryReceive (out i));
			Assert.AreEqual (12, i);

			Assert.IsTrue (source1.Post (13));

			Tuple<int, int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (11, 21, 31), tuple);

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (13, 22, 32), tuple);
		}
	}
}