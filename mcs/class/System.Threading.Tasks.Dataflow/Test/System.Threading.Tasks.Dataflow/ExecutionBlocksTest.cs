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
using System.Threading;
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
				return new int[0];
			});
		}
			
		[Test]
		public void ExceptionTest ()
		{
			var blocks = GetExecutionBlocksWithAction (() => { throw new Exception (); });
			foreach (var block in blocks) {
				Assert.IsFalse (block.Completion.Wait (100));

				block.Post (1);

				var ae =
					Assert.Throws<AggregateException> (() => block.Completion.Wait (100));
				Assert.AreEqual (1, ae.InnerExceptions.Count);
				Assert.AreEqual (typeof(Exception), ae.InnerException.GetType ());
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

				Assert.Throws<AggregateException> (() => block.Completion.Wait (100));

				Thread.Sleep (100);

				Assert.AreEqual (0, Thread.VolatileRead (ref ranAfterFault));
			}
		}
	}
}