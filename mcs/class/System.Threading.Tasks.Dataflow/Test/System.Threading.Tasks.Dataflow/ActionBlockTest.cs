// 
// ActionBlockTest.cs
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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class ActionBlockTest {
		[Test]
		public void BasicUsageTest ()
		{
			bool[] array = new bool[3];
			var evt = new CountdownEvent (array.Length);
			var block = new ActionBlock<int> (i => { array[i] = true; evt.Signal (); });

			for (int i = 0; i < array.Length; ++i)
				Assert.IsTrue (block.Post (i), "Not accepted");

			Assert.IsTrue (evt.Wait (500));
			
			Assert.IsTrue (array.All (b => b), "Some false");
		}

		[Test]
		public void CompleteTest ()
		{
			var block = new ActionBlock<int> (i => Thread.Sleep (100));

			for (int i = 0; i < 10; i++)
				Assert.IsTrue (block.Post (i), "Not Accepted");

			block.Complete ();
			// Still element to be processed so Completion should be false
			Assert.IsFalse (block.Completion.IsCompleted);
			block.Completion.Wait ();
			Assert.IsTrue (block.Completion.IsCompleted);
		}

		[Test]
		public void AsyncNullTest()
		{
			var scheduler = new TestScheduler ();
			var block = new ActionBlock<int> (
				i => null,
				new ExecutionDataflowBlockOptions { TaskScheduler = scheduler });

			Assert.IsTrue (block.Post (1));

			scheduler.ExecuteAll ();

			Assert.IsFalse (block.Completion.Wait (100));

			block.Complete ();

			Assert.IsTrue (block.Completion.Wait (100));
		}

		[Test]
		public void AsyncCancelledTest()
		{
			var scheduler = new TestScheduler ();
			var block = new ActionBlock<int> (
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