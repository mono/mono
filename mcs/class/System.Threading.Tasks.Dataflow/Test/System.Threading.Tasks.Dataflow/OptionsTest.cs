// OptionsTest.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class OptionsTest {
		static IEnumerable<IDataflowBlock> CreateBlocksWithOptions (
			DataflowBlockOptions dataflowBlockOptions,
			ExecutionDataflowBlockOptions executionDataflowBlockOptions,
			GroupingDataflowBlockOptions groupingDataflowBlockOptions)
		{
			yield return new ActionBlock<int> (i => { }, executionDataflowBlockOptions);
			yield return new BatchBlock<double> (10, groupingDataflowBlockOptions);
			yield return new BatchedJoinBlock<dynamic, object> (
				10, groupingDataflowBlockOptions);
			yield return new BatchedJoinBlock<dynamic, object, char> (
				10, groupingDataflowBlockOptions);
			yield return new BroadcastBlock<byte> (x => x, dataflowBlockOptions);
			yield return new BufferBlock<int> (dataflowBlockOptions);
			yield return new JoinBlock<double, dynamic> (groupingDataflowBlockOptions);
			yield return new JoinBlock<object, char, byte> (
				groupingDataflowBlockOptions);
			yield return new TransformBlock<int, int> (
				i => i, executionDataflowBlockOptions);
			yield return new TransformManyBlock<double, dynamic> (
				x => null, executionDataflowBlockOptions);
			yield return new WriteOnceBlock<object> (x => x, dataflowBlockOptions);
		}

		static IEnumerable<IDataflowBlock> CreateBlocksWithNameFormat(string nameFormat)
		{
			var dataflowBlockOptions = new DataflowBlockOptions { NameFormat = nameFormat };
			var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions { NameFormat = nameFormat };
			var groupingDataflowBlockOptions = new GroupingDataflowBlockOptions { NameFormat = nameFormat };

			return CreateBlocksWithOptions (dataflowBlockOptions,
				executionDataflowBlockOptions, groupingDataflowBlockOptions);
		}

		static IEnumerable<IDataflowBlock> CreateBlocksWithCancellationToken(CancellationToken cancellationToken)
		{
			var dataflowBlockOptions = new DataflowBlockOptions { CancellationToken = cancellationToken};
			var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken};
			var groupingDataflowBlockOptions = new GroupingDataflowBlockOptions { CancellationToken = cancellationToken};

			return CreateBlocksWithOptions (dataflowBlockOptions,
				executionDataflowBlockOptions, groupingDataflowBlockOptions);
		}

		[Test]
		public void NameFormatTest ()
		{
			var constant = "constant";
			foreach (var block in CreateBlocksWithNameFormat (constant))
				Assert.AreEqual (constant, block.ToString ());

			foreach (var block in CreateBlocksWithNameFormat ("{0}"))
				Assert.AreEqual (block.GetType ().ToString (), block.ToString ());

			foreach (var block in CreateBlocksWithNameFormat ("{1}"))
				Assert.AreEqual (block.Completion.Id.ToString (), block.ToString ());
		}

		[Test]
		public void CancellationTest()
		{
			var source = new CancellationTokenSource ();
			var blocks = CreateBlocksWithCancellationToken (source.Token).ToArray ();

			foreach (var block in blocks)
				Assert.IsFalse (block.Completion.Wait (100));

			source.Cancel ();

			foreach (var block in blocks) {
				var ae =
					Assert.Throws<AggregateException> (() => block.Completion.Wait (100));
				Assert.AreEqual (1, ae.InnerExceptions.Count);
				Assert.IsInstanceOf<TaskCanceledException> (ae.InnerExceptions [0]);
				Assert.IsTrue (block.Completion.IsCanceled);
			}
		}

		static IEnumerable<int[]> GetTaskIdsForExecutionsOptions (
			ExecutionDataflowBlockOptions options)
		{
			var blockFactories =
				new Func<ConcurrentQueue<Tuple<int, int>>, ITargetBlock<int>>[]
				{
					q => new ActionBlock<int> (
						     i => q.Enqueue (Tuple.Create (i, Task.CurrentId.Value)), options),
					q => new TransformBlock<int, int> (i =>
					{
						q.Enqueue (Tuple.Create (i, Task.CurrentId.Value));
						return i;
					}, options),
					q => new TransformManyBlock<int, int> (i =>
					{
						q.Enqueue (Tuple.Create (i, Task.CurrentId.Value));
						return new[] { i };
					}, options)
				};

			foreach (var factory in blockFactories) {
				var queue = new ConcurrentQueue<Tuple<int, int>> ();
				var block = factory (queue);

				Assert.IsEmpty (queue);

				for (int i = 0; i < 100; i++)
					block.Post (i);

				block.Complete ();

				var source = block as ISourceBlock<int>;
				if (source != null) {
					Assert.IsFalse (block.Completion.Wait (100));

					source.LinkTo (new BufferBlock<int> ());
				}
				Assert.IsTrue (block.Completion.Wait (500));

				CollectionAssert.AreEquivalent (
					Enumerable.Range (0, 100), queue.Select (t => t.Item1));

				yield return queue.Select (t => t.Item2).ToArray ();
			}
		}

		static int CalculateDegreeOfParallelism(IEnumerable<int> taskIds)
		{
			var firsts = new Dictionary<int, int> ();
			var lasts = new Dictionary<int, int> ();

			int i = 0;
			foreach (var taskId in taskIds) {
				if (!firsts.ContainsKey (taskId))
					firsts.Add (taskId, i);

				lasts [taskId] = i;

				i++;
			}

			int maxTime = i;

			var times =
				Enumerable.Repeat (Tuple.Create<int?, int?> (null, null), maxTime).ToArray ();

			foreach (var first in firsts)
				times [first.Value] = Tuple.Create<int?, int?> (
					first.Key, times [first.Value].Item2);

			foreach (var last in lasts)
				times [last.Value] = Tuple.Create<int?, int?> (
					times [last.Value].Item1, last.Key);

			int maxDop = 0;
			int dop = 0;

			foreach (var time in times) {
				if (time.Item1 != null)
					dop++;

				if (dop > maxDop)
					maxDop = dop;

				if (time.Item2 != null)
					dop--;
			}

			return maxDop;
		}

		[Test]
		public void MaxDegreeOfParallelismTest()
		{
			// loop to better test for race conditions
			// some that showed in this test were quite rare
			for (int i = 0; i < 10; i++)
			{
				var options = new ExecutionDataflowBlockOptions ();
				foreach (var taskIds in GetTaskIdsForExecutionsOptions(options))
					Assert.AreEqual (1, CalculateDegreeOfParallelism (taskIds));

				options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 };
				foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
					Assert.LessOrEqual (CalculateDegreeOfParallelism (taskIds), 2);

				options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 };
				foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
					Assert.LessOrEqual (CalculateDegreeOfParallelism (taskIds), 4);

				options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = -1 };
				foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
					Assert.LessOrEqual (CalculateDegreeOfParallelism (taskIds), taskIds.Length);
			}
		}

		[Test]
		public void MaxMessagesPerTaskTest()
		{
			var options = new ExecutionDataflowBlockOptions ();
			foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
				Assert.GreaterOrEqual (taskIds.Distinct ().Count (), 1);

			options = new ExecutionDataflowBlockOptions { MaxMessagesPerTask = 1 };
			foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
				Assert.AreEqual (100, taskIds.Distinct ().Count ());

			options = new ExecutionDataflowBlockOptions { MaxMessagesPerTask = 2 };
			foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
				Assert.GreaterOrEqual (taskIds.Distinct ().Count (), taskIds.Length / 2);

			options = new ExecutionDataflowBlockOptions { MaxMessagesPerTask = 4 };
			foreach (var taskIds in GetTaskIdsForExecutionsOptions (options))
				Assert.GreaterOrEqual (taskIds.Distinct ().Count (), taskIds.Length / 4);
		}

		[Test]
		public void TaskSchedulerTest ()
		{
			var scheduler = new TestScheduler ();

			int n = 0;

			var action = new ActionBlock<int> (
				i => Interlocked.Increment (ref n),
				new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsTrue (action.Post (1));

			Assert.AreEqual (0, Thread.VolatileRead (ref n));

			Assert.AreEqual (1, scheduler.ExecuteAll ());
			Assert.AreEqual (1, Thread.VolatileRead (ref n));
		}

		[Test]
		public void DefaultSchedulerIsDefaultTest ()
		{
			var scheduler = new TestScheduler ();
			var factory = new TaskFactory (scheduler);

			ActionBlock<int> action = null;

			var task = factory.StartNew (() =>
			{
				Assert.AreEqual (scheduler, TaskScheduler.Current);

				action = new ActionBlock<int> (
					i => Assert.AreNotEqual (scheduler, TaskScheduler.Current));
				Assert.IsTrue (action.Post (1));
				action.Complete ();
			});

			Assert.AreEqual (1, scheduler.ExecuteAll ());

			Assert.NotNull (action);

			Assert.IsTrue (action.Completion.Wait (100));
			Assert.IsTrue (task.Wait (0));
		}

		[Test]
		public void MaxMessagesDirectTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			Assert.NotNull (
				source.LinkTo (target, new DataflowLinkOptions { MaxMessages = 1 }));

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (null, out item));
			Assert.AreEqual (42, item);

			Assert.IsTrue (source.Post (43));
			scheduler.ExecuteAll ();
			Assert.IsFalse (target.TryReceive (null, out item));
			Assert.IsTrue (source.TryReceive (null, out item));
			Assert.AreEqual (43, item);
		}

		[Test]
		public void MaxMessagesPostponedTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new BufferBlock<int> (
				new DataflowBlockOptions { TaskScheduler = scheduler, BoundedCapacity = 1 });
			Assert.NotNull (
				source.LinkTo (target, new DataflowLinkOptions { MaxMessages = 2 }));

			Assert.IsTrue (source.Post (42));
			Assert.IsTrue (source.Post (43));
			Assert.IsTrue (source.Post (44));
			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (target.TryReceive (null, out item));
			Assert.AreEqual (42, item);
			Assert.IsFalse (target.TryReceive (null, out item));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.TryReceive (null, out item));
			Assert.AreEqual (43, item);

			scheduler.ExecuteAll ();

			Assert.IsFalse (target.TryReceive (null, out item));
			Assert.IsTrue (source.TryReceive (null, out item));
			Assert.AreEqual (44, item);
		}

		[Test]
		public void MaxMessagesPostponedUnconsumedTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target =
				new BufferBlock<int> (
					new DataflowBlockOptions { TaskScheduler = scheduler, BoundedCapacity = 1 });
			Assert.NotNull (
				source.LinkTo (target, new DataflowLinkOptions { MaxMessages = 2 }));

			Assert.IsTrue (source.Post (42));
			Assert.IsTrue (source.Post (43));
			Assert.IsTrue (source.Post (44));
			Assert.IsTrue (source.Post (45));
			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (source.TryReceive (null, out item));
			Assert.AreEqual (43, item);

			Assert.IsTrue (target.TryReceive (null, out item));
			Assert.AreEqual (42, item);
			Assert.IsFalse (target.TryReceive (null, out item));

			scheduler.ExecuteAll ();

			Assert.IsTrue (target.TryReceive (null, out item));
			Assert.AreEqual (44, item);

			scheduler.ExecuteAll ();

			Assert.IsFalse (target.TryReceive (null, out item));
			Assert.IsTrue (source.TryReceive (null, out item));
			Assert.AreEqual (45, item);
		}
	}
}