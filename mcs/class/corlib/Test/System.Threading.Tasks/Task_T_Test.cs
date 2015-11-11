// Task_T_Tests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//


using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class Task_T_Tests
	{
		Task<int> InitTestTask()
		{
			return Task.Factory.StartNew<int> (() => 5);
		}
		
		[Test]
		public void SimpleTaskTestCase ()
		{
			Task<int> f = InitTestTask ();
			
			Assert.IsNotNull(f, "#1");
			Assert.AreEqual(5, f.Result, "#2");
		}
		
		[Test]
		public void TaskContinueWithTestCase ()
		{
			bool result = false;
			
			Task<int> f = InitTestTask ();
			Task<int> cont = f.ContinueWith ((future) => { result = true; return future.Result * 2; });
			f.Wait ();
			cont.Wait ();
			
			Assert.IsNotNull (cont, "#1");
			Assert.IsTrue (result, "#2");
			Assert.AreEqual (10, cont.Result);
		}

		static Task<int> CreateNestedFuture(int level)
		{
			if (level == 0)
				return Task.Factory.StartNew(() => { Thread.Sleep (1); return 1; });

			var t = CreateNestedFuture(level - 1);
			return Task.Factory.StartNew(() => t.Result + 1);
		}

		[Test]
		public void NestedFutureTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				var t = CreateNestedFuture(10);
				var t2 = CreateNestedFuture(100);
				var t3 = CreateNestedFuture(100);

				Assert.AreEqual (11, t.Result);
				Assert.AreEqual (101, t2.Result);
				Assert.AreEqual (101, t3.Result);
		   }, 50);
		}

		[Test]
		public void FaultedFutureTest ()
		{
			var thrown = new ApplicationException ();
			var f = Task<int>.Factory.StartNew (() => { throw thrown; });
			AggregateException ex = null;
			try {
				f.Wait ();
			} catch (AggregateException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.AreEqual (thrown, ex.InnerException);
			Assert.AreEqual (thrown, f.Exception.InnerException);
			Assert.AreEqual (TaskStatus.Faulted, f.Status);

			ex = null;
			try {
				var result = f.Result;
			} catch (AggregateException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.AreEqual (TaskStatus.Faulted, f.Status);
			Assert.AreEqual (thrown, f.Exception.InnerException);
			Assert.AreEqual (thrown, ex.InnerException);
		}
	}
}
