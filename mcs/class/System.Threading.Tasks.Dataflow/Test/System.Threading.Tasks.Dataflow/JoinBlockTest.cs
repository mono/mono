// 
// JoinBlockTest.cs
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
	public class JoinBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			Tuple<int, int> tuple = null;
			var evt = new ManualResetEventSlim (false);

			var ablock = new ActionBlock<Tuple<int, int>> (t =>
			{
				tuple = t;
				evt.Set ();
			});
			var block = new JoinBlock<int, int> ();
			block.LinkTo (ablock);

			block.Target1.Post (42);

			evt.Wait (1000);
			Assert.IsNull (tuple);

			block.Target2.Post (24);

			evt.Wait ();
			Assert.IsNotNull (tuple);
			Assert.AreEqual (42, tuple.Item1);
			Assert.AreEqual (24, tuple.Item2);
		}

		[Test]
		public void DeadlockTest ()
		{
			Tuple<int, int> tuple = null;
			var evt = new ManualResetEventSlim (false);

			var ablock = new ActionBlock<Tuple<int, int>> (t =>
			{
				tuple = t;
				evt.Set ();
			});
			var block = new JoinBlock<int, int> ();
			block.LinkTo (ablock);

			Task.Factory.StartNew (() => block.Target1.Post (42));
			Task.Factory.StartNew (() => block.Target2.Post (24));

			Assert.IsTrue (evt.Wait (500));
			Assert.IsNotNull (tuple);
			Assert.AreEqual (42, tuple.Item1);
			Assert.AreEqual (24, tuple.Item2);
		}

		[Test]
		public void BoundedCapacityTest ()
		{
			var block = new JoinBlock<int, int> (
				new GroupingDataflowBlockOptions { BoundedCapacity = 1 });
			Assert.IsTrue (block.Target1.Post (1));
			Assert.IsFalse (block.Target1.Post (2));

			Assert.IsTrue (block.Target2.Post (10));
			Assert.IsFalse (block.Target2.Post (11));
			Assert.IsFalse (block.Target1.Post (3));

			Assert.AreEqual (Tuple.Create (1, 10), block.Receive ());
			Assert.IsTrue (block.Target1.Post (4));
		}

		[Test]
		public void CompletionTest ()
		{
			var block = new JoinBlock<int, int> ();

			Assert.IsTrue (block.Target1.Post (1));

			block.Complete ();

			Tuple<int, int> tuple;
			Assert.IsFalse (block.TryReceive (out tuple));

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void MaxNumberOfGroupsTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new JoinBlock<int, int> (
				new GroupingDataflowBlockOptions
				{ MaxNumberOfGroups = 1, TaskScheduler = scheduler });

			Assert.IsTrue (block.Target1.Post (1));

			Assert.IsFalse (block.Target1.Post (2));

			Assert.IsTrue (block.Target2.Post (3));

			Assert.IsFalse (block.Target2.Post (4));

			Tuple<int, int> batch;
			Assert.IsTrue (block.TryReceive (out batch));
			Assert.AreEqual (Tuple.Create (1, 3), batch);

			Assert.IsFalse (block.TryReceive (out batch));

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void NonGreedyMaxNumberOfGroupsTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new JoinBlock<int, int> (
				new GroupingDataflowBlockOptions
				{ MaxNumberOfGroups = 1, Greedy = false, TaskScheduler = scheduler });
			var source1 = new TestSourceBlock<int> ();
			var source2 = new TestSourceBlock<int> ();

			var header1 = new DataflowMessageHeader (1);
			source1.AddMessage (header1, 11);
			source2.AddMessage (header1, 21);

			Assert.AreEqual (DataflowMessageStatus.Postponed,
				block.Target1.OfferMessage (header1, 11, source1, false));
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				block.Target2.OfferMessage (header1, 21, source2, false));

			scheduler.ExecuteAll ();

			Assert.IsTrue (source1.WasConsumed (header1));
			Assert.IsTrue (source2.WasConsumed (header1));

			var header2 = new DataflowMessageHeader (2);
			Assert.AreEqual (DataflowMessageStatus.DecliningPermanently,
				block.Target1.OfferMessage (header2, 21, source1, false));

			Tuple<int, int> tuple;
			Assert.IsTrue (block.TryReceive (out tuple));
			Assert.AreEqual (Tuple.Create (11, 21), tuple);

			Assert.IsTrue (block.Completion.Wait (100));
		}
	}
}