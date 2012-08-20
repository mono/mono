// OutputAvailableTest.cs
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
using System.Threading;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class OutputAvailableTest {
		[Test]
		public void ImmediateTest()
		{
			var scheduler = new TestScheduler();
			var source = new BufferBlock<int>(new DataflowBlockOptions{TaskScheduler = scheduler});

			Assert.IsTrue(source.Post(1));

			var task = source.OutputAvailableAsync();

			// not necessary in MS.NET
			scheduler.ExecuteAll();

			Assert.IsTrue(task.IsCompleted);
			Assert.IsTrue(task.Result);
		}

		[Test]
		public void PostponedTest()
		{
			var source = new BufferBlock<int>(new DataflowBlockOptions());

			var task = source.OutputAvailableAsync();

			Assert.IsFalse(task.Wait(100));

			Assert.IsTrue(source.Post(1));

			Assert.IsTrue(task.Wait(100));
			Assert.IsTrue(task.Result);
		}

		[Test]
		public void CompletedImmediateTest()
		{
			var source = new BufferBlock<int>();
			source.Complete();

			var task = source.OutputAvailableAsync();

			Assert.IsTrue(task.Wait(100));
			Assert.IsFalse(task.Result);
		}

		[Test]
		public void CompletedPostponedTest()
		{
			var source = new BufferBlock<int>(new DataflowBlockOptions());

			var task = source.OutputAvailableAsync();

			Assert.IsFalse(task.Wait(100));

			source.Complete();

			Assert.IsTrue(task.Wait(100));
			Assert.IsFalse(task.Result);
		}

		[Test]
		public void FaultedImmediateTest()
		{
			var source = new BufferBlock<int>();
			((IDataflowBlock)source).Fault(new Exception());

			var task = source.OutputAvailableAsync();

			Assert.IsTrue(task.Wait(100));
			Assert.IsFalse(task.Result);
		}

		[Test]
		public void FaultedPostponedTest()
		{
			var source = new BufferBlock<int>();

			var task = source.OutputAvailableAsync();

			Assert.IsFalse(task.Wait(100));

			((IDataflowBlock)source).Fault(new Exception());

			Assert.IsTrue(task.Wait(100));
			Assert.IsFalse(task.Result);
		}

		[Test]
		public void WithTargetTest()
		{
			var source = new BufferBlock<int>();
			var target = new BufferBlock<int>();
			source.LinkTo(target);

			var task = source.OutputAvailableAsync();

			Assert.IsTrue(source.Post(1));

			Assert.IsFalse(task.Wait(100));
		}

		[Test]
		public void WithDecliningTargetTest()
		{
			var source = new BufferBlock<int>();
			var target = new BufferBlock<int>();
			source.LinkTo(target, i => false);

			var task = source.OutputAvailableAsync();

			Assert.IsTrue(source.Post(1));

			Assert.IsTrue(task.Wait(100));
			Assert.IsTrue(task.Result);
		}

		[Test]
		public void CancellationTest()
		{
			var source = new BufferBlock<int>();

			var tokenSource = new CancellationTokenSource();
			var task = source.OutputAvailableAsync(tokenSource.Token);

			tokenSource.Cancel();

			AssertEx.Throws<AggregateException>(() => task.Wait(100));
			Assert.IsTrue(task.IsCanceled);
		}
	}
}
