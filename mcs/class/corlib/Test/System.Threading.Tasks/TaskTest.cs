//
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

#if NET_4_0

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskTests
	{
		Task[]      tasks;
		static readonly int max = 3 * Environment.ProcessorCount;
		
		[SetUp]
		public void Setup()
		{
			tasks = new Task[max];			
		}
		
		void InitWithDelegate(Action action)
		{
			for (int i = 0; i < max; i++) {
				tasks[i] = Task.Factory.StartNew(action);
			}
		}
		
		[TestAttribute]
		public void WaitAnyTest()
		{
			ParallelTestHelper.Repeat (delegate {
				int flag = 0;
				int finished = 0;
				
				InitWithDelegate(delegate {
					int times = Interlocked.Exchange (ref flag, 1);
					if (times == 1) {
						SpinWait sw = new SpinWait ();
						while (finished == 0) sw.SpinOnce ();
					} else {
						Interlocked.Increment (ref finished);
					}
				});
				
				int index = Task.WaitAny(tasks);
				
				Assert.AreNotEqual (-1, index, "#3");
				Assert.AreEqual (1, flag, "#1");
				Assert.AreEqual (1, finished, "#2");
				
				Task.WaitAll (tasks);
			});
		}
		
		[TestAttribute]
		public void WaitAllTest()
		{
			ParallelTestHelper.Repeat (delegate {
				int achieved = 0;
				InitWithDelegate(delegate { Interlocked.Increment(ref achieved); });
				Task.WaitAll(tasks);
				Assert.AreEqual(max, achieved, "#1");
			});
		}
		
		[Test]
		public void CancelTestCase()
		{
			CancellationTokenSource src = new CancellationTokenSource ();
			
			Task t = new Task (delegate { }, src.Token);
			src.Cancel ();
			
			t.Start ();
			Exception ex = null;
			
			try {
				t.Wait ();
			} catch (Exception e) {
				ex = e;
			}
			
			Assert.IsNotNull (ex, "#1");
			Assert.IsInstanceOfType (typeof(AggregateException), ex, "#2");
			Assert.IsNull (t.Exception, "#3");
			
			AggregateException aggr = (AggregateException)ex;
			Assert.AreEqual (1, aggr.InnerExceptions.Count, "#4");
			Assert.IsInstanceOfType (typeof (OperationCanceledException), aggr.InnerExceptions[0], "#5");
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void CreationWhileInitiallyCanceled ()
		{
			var token = new CancellationToken (true);
			var task = new Task (() => { }, token);
			Assert.AreEqual (TaskStatus.Canceled, task.Status);
			task.Start ();
		}
		
		[Test]
		public void ContinueWithOnAnyTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool result = false;
				
				Task t = Task.Factory.StartNew(delegate { });
				Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationOptions.None);
				Assert.IsTrue (t.Wait (2000), "First wait, (status, {0})", t.Status);
				Assert.IsTrue (cont.Wait(2000), "Cont wait, (result, {0}) (parent status, {2}) (status, {1})", result, cont.Status, t.Status);
				Assert.IsNull(cont.Exception, "#1");
				Assert.IsNotNull(cont, "#2");
				Assert.IsTrue(result, "#3");
			});
		}
		
		[Test]
		public void ContinueWithOnCompletedSuccessfullyTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool result = false;
				
				Task t = Task.Factory.StartNew(delegate { });
				Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationOptions.OnlyOnRanToCompletion);
				t.Wait();
				cont.Wait();
				
				Assert.IsNull(cont.Exception, "#1");
				Assert.IsNotNull(cont, "#2");
				Assert.IsTrue(result, "#3");
			});
		}
		
		[Test]
		public void ContinueWithOnAbortedTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool result = false;
				bool taskResult = false;
				
				CancellationTokenSource src = new CancellationTokenSource ();
				Task t = new Task(delegate { taskResult = true; }, src.Token);
				src.Cancel ();
				
				Task cont = t.ContinueWith (delegate { result = true; },
				                            TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);

				t.Start();
				cont.Wait();
				
				Assert.IsFalse (taskResult, "#-1");
				Assert.AreEqual (TaskStatus.Canceled, t.Status, "#0");
				Assert.IsTrue (t.IsCanceled, "#0bis");
				
				Assert.IsNull(cont.Exception, "#1");
				Assert.IsNotNull(cont, "#2");
				Assert.IsTrue(result, "#3");
			});
		}
		
		[Test]
		public void ContinueWithOnFailedTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool result = false;
				
				Task t = Task.Factory.StartNew(delegate { throw new Exception("foo"); });	
				Task cont = t.ContinueWith(delegate { result = true; }, TaskContinuationOptions.OnlyOnFaulted);
			
				Assert.IsTrue (cont.Wait(1000), "#0");
				Assert.IsNotNull (t.Exception, "#1");
				Assert.IsNotNull (cont, "#2");
				Assert.IsTrue (result, "#3");
			});
		}

		[Test]
		public void ContinueWithWithStart ()
		{
			Task t = new Task<int> (() => 1);
			t = t.ContinueWith (l => { });
			try {
				t.Start ();
				Assert.Fail ();
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void ContinueWithChildren ()
		{
			ParallelTestHelper.Repeat (delegate {
			    bool result = false;

			    var t = Task.Factory.StartNew (() => Task.Factory.StartNew (() => {}, TaskCreationOptions.AttachedToParent));

				var mre = new ManualResetEvent (false);
			    t.ContinueWith (l => {
					result = true;
					mre.Set ();
				});

				Assert.IsTrue (mre.WaitOne (1000), "#1");
			    Assert.IsTrue (result, "#2");
			}, 2);
		}

		[TestAttribute]
		public void MultipleTaskTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool r1 = false, r2 = false, r3 = false;
				
				Task t1 = Task.Factory.StartNew(delegate {
					r1 = true;
				});
				Task t2 = Task.Factory.StartNew(delegate {
					r2 = true;
				});
				Task t3 = Task.Factory.StartNew(delegate {
					r3 = true;
				});
				
				t1.Wait();
				t2.Wait();
				t3.Wait();
				
				Assert.IsTrue(r1, "#1");
				Assert.IsTrue(r2, "#2");
				Assert.IsTrue(r3, "#3");
			});
		}
		
		[Test]
		public void WaitChildTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool r1 = false, r2 = false, r3 = false;
				
				Task t = Task.Factory.StartNew(delegate {
					Task.Factory.StartNew(delegate {
						Thread.Sleep(50);
						r1 = true;
					}, TaskCreationOptions.AttachedToParent);
					Task.Factory.StartNew(delegate {
						Thread.Sleep(300);
						
						r2 = true;
					}, TaskCreationOptions.AttachedToParent);
					Task.Factory.StartNew(delegate {
						Thread.Sleep(150);
						
						r3 = true;
					}, TaskCreationOptions.AttachedToParent);
				});
				
				t.Wait();
				Assert.IsTrue(r2, "#1");
				Assert.IsTrue(r3, "#2");
				Assert.IsTrue(r1, "#3");
				Assert.AreEqual (TaskStatus.RanToCompletion, t.Status, "#4");
				}, 10);
		}

		[Test]
		public void ExecuteSynchronouslyTest ()
		{
			var val = 0;
			Task t = new Task (() => { Thread.Sleep (100); val = 1; });
			t.RunSynchronously ();

			Assert.AreEqual (1, val);
		}

		[Test]
		public void RunSynchronouslyArgumentChecks ()
		{
			Task t = new Task (() => { });
			try {
				t.RunSynchronously (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void UnobservedExceptionOnFinalizerThreadTest ()
		{
			bool wasCalled = false;
			TaskScheduler.UnobservedTaskException += (o, args) => {
				wasCalled = true;
				args.SetObserved ();
			};
			var inner = new ApplicationException ();
			Task.Factory.StartNew (() => { throw inner; });
			Thread.Sleep (1000);
			GC.Collect ();
			Thread.Sleep (1000);
			GC.WaitForPendingFinalizers ();

			Assert.IsTrue (wasCalled);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void StartFinishedTaskTest ()
		{
			var t = Task.Factory.StartNew (delegate () { });
			t.Wait ();

			t.Start ();
		}

		[Test]
		public void Start_NullArgument ()
		{
			var t = Task.Factory.StartNew (delegate () { });
			try {
				t.Start (null);
				Assert.Fail ();
			} catch (ArgumentNullException) {
			}
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void DisposeUnstartedTest ()
		{
			var t = new Task (() => { });
			t.Dispose ();
		}

		[Test]
		public void ThrowingUnrelatedCanceledExceptionTest ()
		{
			Task t = new Task (() => {
				throw new TaskCanceledException ();
			});

			t.RunSynchronously ();
			Assert.IsTrue (t.IsFaulted);
			Assert.IsFalse (t.IsCanceled);
		}

#if NET_4_5
		[Test]
		public void FromResult ()
		{
			var t = Task.FromResult<object> (null);
			Assert.IsTrue (t.IsCompleted, "#1");
			Assert.AreEqual (null, t.Result, "#2");
			t.Dispose ();
			t.Dispose ();
		}

		[Test]
		public void Run_ArgumentCheck ()
		{
			try {
				Task.Run (null as Action);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Run ()
		{
			var t = Task.Run (delegate { });
			Assert.AreEqual (TaskCreationOptions.DenyChildAttach, t.CreationOptions, "#1");
			t.Wait ();
		}

		[Test]
		public void Run_Cancel ()
		{
			var t = Task.Run (() => 1, new CancellationToken (true));
			try {
				var r = t.Result;
				Assert.Fail ("#1");
			} catch (AggregateException) {
			}

			Assert.IsTrue (t.IsCanceled, "#2");
		}

		[Test]
		public void Run_ExistingTask ()
		{
			var t = new Task<int> (() => 5);
			var t2 = Task.Run (() => { t.Start (); return t; });

			Assert.IsTrue (t2.Wait (1000), "#1");
			Assert.AreEqual (5, t2.Result, "#2");
		}
#endif
	}
}
#endif
