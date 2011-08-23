// 
// DataflowBlockTest.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Jérémie "garuma" Laval
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow
{
	[TestFixture]
	public class DataflowBlockTest
	{
		[Test]
		public void ChooseTest ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();

			bool action1 = false;
			bool action2 = false;
			var completion = DataflowBlock.Choose (source1, (_) => action1 = true, source2, (_) => action2 = true);

			source1.Post (42);

			Thread.Sleep (1600);
			Assert.IsTrue (action1);
			Assert.IsFalse (action2);
			Assert.IsTrue (completion.IsCompleted);
			Assert.AreEqual (TaskStatus.RanToCompletion, completion.Status);
			Assert.AreEqual (0, completion.Result);
		}

		[Test]
		public void ChooseTest_3 ()
		{
			var source1 = new BufferBlock<int> ();
			var source2 = new BufferBlock<long> ();
			var source3 = new BufferBlock<object> ();

			bool action1 = false;
			bool action2 = false;
			bool action3 = false;
			var completion = DataflowBlock.Choose (source1, (_) => action1 = true, source2, (_) => action2 = true, source3, (_) => action3 = true);

			source3.Post (new object ());

			Thread.Sleep (1600);
			Assert.IsFalse (action1);
			Assert.IsFalse (action2);
			Assert.IsTrue (action3);
			Assert.IsTrue (completion.IsCompleted);
			Assert.AreEqual (TaskStatus.RanToCompletion, completion.Status);
			Assert.AreEqual (2, completion.Result);			
		}

		[Test]
		public void TryReceiveTest ()
		{
			var block = new BufferBlock<int> ();
			int value = -1;

			block.Post (42);
			Thread.Sleep (500);
			Assert.IsTrue (block.TryReceive (out value));
			Assert.AreEqual (42, value);
		}

		[Test]
		public void ReceiveTest ()
		{
			var block = new BufferBlock<int> ();
			Task.Factory.StartNew (() => { Thread.Sleep (1400); block.Post (42); });
			Assert.AreEqual (42, block.Receive ());
		}

		[Test]
		public void AsyncReceiveTest ()
		{
			int result = -1;
			var mre = new ManualResetEventSlim (false);

			var block = new WriteOnceBlock<int> (null);
			block.ReceiveAsync ().ContinueWith (i => { result = i.Result; mre.Set (); });
			Task.Factory.StartNew (() => { Thread.Sleep (600); block.Post (42); });
			mre.Wait ();

			Assert.AreEqual (42, result);
		}

		[Test]
		public void AsyncReceiveTestCanceled ()
		{
			var src = new CancellationTokenSource ();

			var block = new WriteOnceBlock<int> (null);
			var task = block.ReceiveAsync (src.Token);
			Task.Factory.StartNew (() => { Thread.Sleep (800); block.Post (42); });
			Thread.Sleep (50);
			src.Cancel ();

			AggregateException ex = null;

			try {
				task.Wait ();
			} catch (AggregateException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.IsNotNull (ex.InnerException);
			Assert.IsInstanceOfType (typeof (OperationCanceledException), ex.InnerException);
			Assert.IsTrue (task.IsCompleted);
			Assert.AreEqual (TaskStatus.Canceled, task.Status);
		}
	}
}
