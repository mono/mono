// 
// TransformBlockTest.cs
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
	public class TransformBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			int[] array = new int[10];
			var evt = new CountdownEvent (10);
			var action = new ActionBlock<int> (i =>
			{
				array [Math.Abs (i)] = i;
				evt.Signal ();
			});
			var block = new TransformBlock<int, int> (i => -i);
			block.LinkTo (action);

			for (int i = 0; i < array.Length; ++i)
				Assert.IsTrue (block.Post (i), "Not accepted");

			evt.Wait ();

			CollectionAssert.AreEqual (
				new[] { 0, -1, -2, -3, -4, -5, -6, -7, -8, -9 }, array);
		}

		[Test]
		public void DeferredUsageTest ()
		{
			int[] array = new int[10];
			var action = new ActionBlock<int> (i => array[Math.Abs (i)] = i);
			var block = new TransformBlock<int, int> (i => -i);

			for (int i = 0; i < array.Length; ++i)
				Assert.IsTrue (block.Post (i), "Not accepted");

			Thread.Sleep (600);
			block.LinkTo (action);
			Thread.Sleep (300);

			CollectionAssert.AreEqual (new[] { 0, -1, -2, -3, -4, -5, -6, -7, -8, -9 }, array);
		}

		[Test]
		public void AsyncNullTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new TransformBlock<int, int> (
				i => null,
				new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsFalse (block.Completion.Wait (100));

			block.Complete ();

			Assert.IsTrue (block.Completion.Wait (1000));
		}

		[Test]
		public void AsyncCancelledTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new TransformBlock<int, int> (
				i =>
				{
					var tcs = new TaskCompletionSource<int> ();
					tcs.SetCanceled ();
					return tcs.Task;
				}, new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsFalse (block.Completion.Wait (100));
		}
	}
}