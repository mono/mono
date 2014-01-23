// 
// TransformManyBlockTest.cs
//  
// Authors:
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class TransformManyBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			int insIndex = -1;
			int[] array = new int[5 + 3];
			var evt = new CountdownEvent (array.Length);

			var block = new ActionBlock<int> (i => { array[Interlocked.Increment (ref insIndex)] = i; evt.Signal (); });
			var trsm = new TransformManyBlock<int, int> (i => Enumerable.Range (0, i));
			trsm.LinkTo (block);

			trsm.Post (5);
			trsm.Post (3);

			evt.Wait ();
			
			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 0, 1, 2 }, array);
		}

		[Test]
		public void DeferredUsageTest ()
		{
			int insIndex = -1;
			int[] array = new int[5 + 3];
			var evt = new CountdownEvent (array.Length);

			var block = new ActionBlock<int> (i => { array[Interlocked.Increment (ref insIndex)] = i; evt.Signal (); });
			var trsm = new TransformManyBlock<int, int> (i => Enumerable.Range (0, i));

			trsm.Post (5);
			trsm.Post (3);

			trsm.LinkTo (block);
			evt.Wait ();

			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 0, 1, 2 }, array);
		}

		[Test]
		public void NullResultTest ()
		{
			bool received = false;

			var transformMany =
				new TransformManyBlock<int, int> (i => (IEnumerable<int>)null);
			var action = new ActionBlock<int> (i => received = true);
			transformMany.LinkTo (action);

			Assert.IsTrue (transformMany.Post (1), "#1");

			transformMany.Complete ();
			Assert.IsTrue (transformMany.Completion.Wait (100), "#2");
			Assert.IsFalse (received, "#3");
		}

		[Test]
		public void AsyncNullTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new TransformManyBlock<int, int> (
				i => (Task<IEnumerable<int>>)null,
				new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsFalse (block.Completion.Wait (100));

			block.Complete ();

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void AsyncCancelledTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new TransformManyBlock<int, int> (
				i =>
				{
					var tcs = new TaskCompletionSource<IEnumerable<int>> ();
					tcs.SetCanceled ();
					return tcs.Task;
				}, new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsFalse (block.Completion.Wait (100));
		}
	}
}