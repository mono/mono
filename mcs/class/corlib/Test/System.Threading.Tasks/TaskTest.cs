#if NET_4_0
// TaskTest.cs
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

namespace ParallelFxTests
{
	[TestFixture()]
	public class TaskTest
	{
		Task[]      tasks;
		//IScheduler  mockScheduler;
		//DynamicMock mock;
		static readonly int max = 3 * Environment.ProcessorCount;
		const int testRun = 10;
		
		[SetUp]
		public void Setup()
		{
			/*mock = new DynamicMock(typeof(IScheduler));
			mockScheduler = (IScheduler)mock.MockInstance;*/
			tasks = new Task[max];
			//TaskManager.Current = new TaskManager(new TaskManagerPolicy(), mockScheduler);
			
		}
		
		void InitWithDelegate(Action<object> action)
		{
			for (int i = 0; i < max; i++) {
				tasks[i] = Task.StartNew(action);
			}
		}
		
		void InitWithDelegate(Action<object> action, int startIndex)
		{
			for (int i = startIndex; i < max; i++) {
				tasks[i] = Task.StartNew(action);
			}
		}
		
		[TestAttribute]
		public void WaitAnyTest()
		{
			int achieved = 0;
			tasks[0] = Task.StartNew(delegate {
				Interlocked.Increment(ref achieved);
			});
			InitWithDelegate(delegate {
				Thread.Sleep(1000);
				Interlocked.Increment(ref achieved);
			}, 1);
			int index = Task.WaitAny(tasks);
			Assert.AreNotEqual(0, achieved, "#1");
			Assert.Less(index, max, "#3");
			Assert.GreaterOrEqual(index, 0, "#2");
		}
		
		[TestAttribute]
		public void WaitAllTest()
		{
			int achieved = 0;
			InitWithDelegate(delegate { Interlocked.Increment(ref achieved); });
			Task.WaitAll(tasks);
			Assert.AreEqual(max, achieved, "#1");
		}
		
		[Test]
		public void CancelTestCase()
		{
			bool result = false;
			
			Task t = new Task(TaskManager.Current, delegate {
				result = true;
			}, null, TaskCreationOptions.None);
			t.Cancel();
			t.Schedule();
			
			Assert.IsInstanceOfType(typeof(TaskCanceledException), t.Exception, "#1");
			TaskCanceledException ex = (TaskCanceledException)t.Exception;
			Assert.AreEqual(t, ex.Task, "#2");
			Assert.IsFalse(result, "#3");
		}
		
		[Test]
		public void ContinueWithOnAnyTestCase()
		{
			bool result = false;
			
			Task t = Task.StartNew(delegate { });
			Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationKind.OnAny);
			t.Wait();
			cont.Wait();
			
			Assert.IsNull(cont.Exception, "#1");
			Assert.IsNotNull(cont, "#2");
			Assert.IsTrue(result, "#3");
		}
		
		[Test]
		public void ContinueWithOnCompletedSuccessfullyTestCase()
		{
			bool result = false;
			
			Task t = Task.StartNew(delegate { });
			Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationKind.OnCompletedSuccessfully);
			t.Wait();
			cont.Wait();
			
			Assert.IsNull(cont.Exception, "#1");
			Assert.IsNotNull(cont, "#2");
			Assert.IsTrue(result, "#3");
		}
		
		[Test]
		public void ContinueWithOnAbortedTestCase()
		{
			bool result = false;
			
			Task t = new Task(TaskManager.Current, delegate { }, null, TaskCreationOptions.None);
			t.Cancel();
			t.Schedule();
			
			Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationKind.OnAborted);
			t.Wait();
			cont.Wait();
			
			Assert.IsNull(cont.Exception, "#1");
			Assert.IsNotNull(cont, "#2");
			Assert.IsTrue(result, "#3");
		}
		
		[Test]
		public void ContinueWithOnFailedTestCase()
		{
			bool result = false;
			
			Task t = Task.StartNew(delegate {throw new Exception("foo"); });
			Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationKind.OnFailed);
			t.Wait();
			cont.Wait();
			
			Assert.IsNotNull(t.Exception, "#1");
			Assert.IsNotNull(cont, "#2");
			Assert.IsTrue(result, "#3");
		}

		[TestAttribute]
		public void MultipleTaskTestCase()
		{
			bool r1 = false, r2 = false, r3 = false;

			Task t1 = Task.StartNew(delegate {
				r1 = true;
			});
			Task t2 = Task.StartNew(delegate {
				r2 = true;
			});
			Task t3 = Task.StartNew(delegate {
				r3 = true;
			});
			
			t1.Wait();
			t2.Wait();
			t3.Wait();

			Assert.IsTrue(r1, "#1");
			Assert.IsTrue(r2, "#2");
			Assert.IsTrue(r3, "#3");
		}
		
		[Test]
		public void WaitChildTestCase()
		{
			bool r1 = false, r2 = false, r3 = false;

			Task.StartNew(delegate { Console.WriteLine("foo"); });

			//Console.WriteLine("bar");
			
			Task t = Task.StartNew(delegate {
				//Console.WriteLine("foobar");
				Task.StartNew(delegate {
					Thread.Sleep(50);
					r1 = true;
					Console.WriteLine("finishing 1");
				});
				Task.StartNew(delegate {
					Thread.Sleep(1000);
					r2 = true;
					Console.WriteLine("finishing 2");
				});
				Task.StartNew(delegate {
					Thread.Sleep(150);
					r3 = true;
					Console.WriteLine("finishing 3");
				});
			});
			
			t.Wait();
			Assert.IsTrue(r3, "#1");
			Assert.IsTrue(r2, "#2");
			Assert.IsTrue(r1, "#1");
		}
	}
}
#endif
