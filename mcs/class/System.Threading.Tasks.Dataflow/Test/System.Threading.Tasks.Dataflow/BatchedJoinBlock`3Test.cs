// BatchedJoinBlockTest.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class BatchedJoinBlock3Test {
		[Test]
		public void BasicUsageTest ()
		{
			Tuple<IList<int>, IList<int>, IList<string>> result = null;
			var evt = new ManualResetEventSlim (false);

			var actionBlock =
				new ActionBlock<Tuple<IList<int>, IList<int>, IList<string>>> (r =>
				{
					result = r;
					evt.Set ();
				});
			var block = new BatchedJoinBlock<int, int, string> (3);

			block.LinkTo (actionBlock);

			// all targets once
			Assert.IsTrue (block.Target1.Post (1));
			Assert.IsTrue (block.Target2.Post (2));

			Assert.IsFalse (evt.Wait (100));
			Assert.IsNull (result);

			Assert.IsTrue (block.Target3.Post ("foo"));

			Assert.IsTrue (evt.Wait (100));

			Assert.IsNotNull (result);
			CollectionAssert.AreEqual (new[] { 1 }, result.Item1);
			CollectionAssert.AreEqual (new[] { 2 }, result.Item2);
			CollectionAssert.AreEqual (new[] { "foo" }, result.Item3);
		}

		[Test]
		public void BoundedCapacityTest ()
		{
			AssertEx.Throws<ArgumentException> (
				() =>
				new BatchedJoinBlock<int, int> (2,
					new GroupingDataflowBlockOptions { BoundedCapacity = 3 }));
		}

		[Test]
		public void CompletionTest ()
		{
			var block = new BatchedJoinBlock<int, int, int> (2);

			Assert.IsTrue (block.Target1.Post (1));

			block.Complete ();

			Tuple<IList<int>, IList<int>, IList<int>> batch;
			Assert.IsTrue (block.TryReceive (out batch), batch.ToString ());
			CollectionAssert.AreEqual (new[] { 1 }, batch.Item1);
			CollectionAssert.IsEmpty (batch.Item2);
			CollectionAssert.IsEmpty (batch.Item3);

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void MaxNumberOfGroupsTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BatchedJoinBlock<int, int, int> (1,
				new GroupingDataflowBlockOptions
				{ MaxNumberOfGroups = 3, TaskScheduler = scheduler });

			Assert.IsTrue (block.Target1.Post (1));

			Assert.IsTrue (block.Target2.Post (2));

			Assert.IsTrue (block.Target3.Post (3));

			Assert.IsFalse (block.Target3.Post (4));
			Assert.IsFalse (block.Target2.Post (5));
			Assert.IsFalse (block.Target1.Post (6));

			Tuple<IList<int>, IList<int>, IList<int>> batch;
			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.AreEqual (new[] { 1 }, batch.Item1);
			CollectionAssert.IsEmpty (batch.Item2);
			CollectionAssert.IsEmpty (batch.Item3);

			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.IsEmpty (batch.Item1);
			CollectionAssert.AreEqual (new[] { 2 }, batch.Item2);
			CollectionAssert.IsEmpty (batch.Item3);

			Assert.IsTrue (block.TryReceive (out batch));
			CollectionAssert.IsEmpty (batch.Item1);
			CollectionAssert.IsEmpty (batch.Item2);
			CollectionAssert.AreEqual (new[] { 3 }, batch.Item3);

			scheduler.ExecuteAll ();

			Assert.IsTrue (block.Completion.Wait (100));
		}
	}
}