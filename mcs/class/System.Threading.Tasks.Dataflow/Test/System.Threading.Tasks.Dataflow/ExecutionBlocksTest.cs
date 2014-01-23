// ExecutionBlocksTest.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class ExecutionBlocksTest {
		static IEnumerable<ITargetBlock<int>> GetExecutionBlocksWithAction(Action action)
		{
			yield return new ActionBlock<int> (i => action());
			yield return new TransformBlock<int, int> (i =>
			{
				action ();
				return i;
			});
			yield return new TransformManyBlock<int, int> (i =>
			{
				action ();
				return Enumerable.Empty<int> ();
			});
		}

		static IEnumerable<ITargetBlock<int>> GetExecutionBlocksWithAsyncAction (
			Func<int, Task> action, ExecutionDataflowBlockOptions options)
		{
			yield return new ActionBlock<int> (action, options);
			yield return new TransformBlock<int, int> (
				i => action (i).ContinueWith (
					t =>
					{
						t.Wait ();
						return i;
					}), options);
			yield return new TransformManyBlock<int, int> (
				i => action (i).ContinueWith (
					t =>
					{
						t.Wait ();
						return Enumerable.Empty<int> ();
					}), options);
		}

		[Test]
		public void ExceptionTest ()
		{
			var exception = new Exception ();
			var blocks = GetExecutionBlocksWithAction (() => { throw exception; });
			foreach (var block in blocks) {
				Assert.IsFalse (block.Completion.Wait (100));

				block.Post (1);

				var ae =
					AssertEx.Throws<AggregateException> (() => block.Completion.Wait (100));
				Assert.AreEqual (1, ae.InnerExceptions.Count);
				Assert.AreSame (exception, ae.InnerException);
			}
		}

		[Test]
		public void NoProcessingAfterFaultTest ()
		{
			int shouldRun = 1;
			int ranAfterFault = 0;
			var evt = new ManualResetEventSlim ();

			var blocks = GetExecutionBlocksWithAction (() =>
			{
				if (Thread.VolatileRead (ref shouldRun) == 0) {
					ranAfterFault++;
					return;
				}

				evt.Wait ();
			});

			foreach (var block in blocks) {
				shouldRun = 1;
				ranAfterFault = 0;
				evt.Reset ();

				Assert.IsTrue (block.Post (1));
				Assert.IsTrue (block.Post (2));

				Assert.IsFalse (block.Completion.Wait (100));
				Assert.AreEqual (0, ranAfterFault);

				block.Fault (new Exception ());

				Assert.IsFalse (block.Completion.Wait (100));

				shouldRun = 0;
				evt.Set ();

				AssertEx.Throws<AggregateException> (() => block.Completion.Wait (100));

				Thread.Sleep (100);

				Assert.AreEqual (0, Thread.VolatileRead (ref ranAfterFault));
			}
		}

		[Test]
		public void AsyncTest ()
		{
			var tcs = new TaskCompletionSource<int> ();
			int result = 0;

			var scheduler = new TestScheduler ();

			var blocks = GetExecutionBlocksWithAsyncAction (
				i =>
				tcs.Task.ContinueWith (t => Thread.VolatileWrite (ref result, i + t.Result)),
				new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			foreach (var block in blocks) {
				Assert.IsTrue (block.Post (1));

				scheduler.ExecuteAll ();
				Thread.Sleep (100);
				Thread.MemoryBarrier ();

				Assert.AreEqual (0, result);

				tcs.SetResult (10);

				Thread.Sleep (100);

				// the continuation should be executed on the configured TaskScheduler
				Assert.AreEqual (0, result);

				scheduler.ExecuteAll ();

				Assert.AreEqual (11, result);

				tcs = new TaskCompletionSource<int> ();
				Thread.VolatileWrite (ref result, 0);
			}
		}

		[Test]
		public void AsyncExceptionTest ()
		{
			var scheduler = new TestScheduler ();
			var exception = new Exception ();

			var blocks = GetExecutionBlocksWithAsyncAction (
				i =>
				{
					var tcs = new TaskCompletionSource<int> ();
					tcs.SetException (exception);
					return tcs.Task;
				},
				new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			foreach (var block in blocks) {
				Assert.IsTrue (block.Post (1));

				// the task should be executed on the configured TaskScheduler
				Assert.IsFalse (block.Completion.Wait (100));

				scheduler.ExecuteAll ();

				var ae =
					AssertEx.Throws<AggregateException> (() => block.Completion.Wait (100)).
						Flatten ();

				Assert.AreEqual (1, ae.InnerExceptions.Count);
				Assert.AreSame (exception, ae.InnerException);
			}
		}
	}
}