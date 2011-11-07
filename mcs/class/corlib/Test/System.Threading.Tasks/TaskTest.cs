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
		Task[] tasks;
		const int max = 6;
		
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
		
		[Test]
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
				
				int index = Task.WaitAny(tasks, 1000);
				
				Assert.AreNotEqual (-1, index, "#3");
				Assert.AreEqual (1, flag, "#1");
				Assert.AreEqual (1, finished, "#2");
			});
		}

		[Test]
		public void WaitAny_Empty ()
		{
			Assert.AreEqual (-1, Task.WaitAny (new Task[0]));
		}

		[Test]
		public void WaitAny_Zero ()
		{
			Assert.AreEqual (-1, Task.WaitAny (new[] { new Task (delegate { })}, 0), "#1");
			Assert.AreEqual (-1, Task.WaitAny (new[] { new Task (delegate { }) }, 20), "#1");
		}

		[Test]
		public void WaitAny_Cancelled ()
		{
			var cancelation = new CancellationTokenSource ();
			var tasks = new Task[] {
				new Task (delegate { }),
				new Task (delegate { }, cancelation.Token)
			};

			cancelation.Cancel ();

			Assert.AreEqual (1, Task.WaitAny (tasks, 1000), "#1");
			Assert.IsTrue (tasks[1].IsCompleted, "#2");
			Assert.IsTrue (tasks[1].IsCanceled, "#3");
		}

		[Test]
		public void WaitAny_CancelledWithoutExecution ()
		{
			var cancelation = new CancellationTokenSource ();
			var tasks = new Task[] {
				new Task (delegate { }),
				new Task (delegate { })
			};

			int res = 0;
			var mre = new ManualResetEventSlim (false);
			ThreadPool.QueueUserWorkItem (delegate {
				res = Task.WaitAny (tasks, 20);
				mre.Set ();
			});

			cancelation.Cancel ();
			Assert.IsTrue (mre.Wait (1000), "#1");
			Assert.AreEqual (-1, res);
		}

		[Test]
		public void WaitAny_OneException ()
		{
			var mre = new ManualResetEventSlim (false);
			var tasks = new Task[] {
				Task.Factory.StartNew (delegate { mre.Wait (1000); }),
				Task.Factory.StartNew (delegate { throw new ApplicationException (); })
			};

			Assert.AreEqual (1, Task.WaitAny (tasks, 1000), "#1");
			Assert.IsFalse (tasks[0].IsCompleted, "#2");
			Assert.IsTrue (tasks[1].IsFaulted, "#3");

			mre.Set ();
		}

		[Test]
		public void WaitAny_SingleCanceled ()
		{
			var src = new CancellationTokenSource ();
			var t = Task.Factory.StartNew (() => { Thread.Sleep (200); src.Cancel (); src.Token.ThrowIfCancellationRequested (); }, src.Token);
			Assert.AreEqual (0, Task.WaitAny (new [] { t }));
		}

		public void WaitAny_ManyExceptions ()
		{
			CountdownEvent cde = new CountdownEvent (3);
			var tasks = new [] {
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } }),
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } }),
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } })
			};

			Assert.IsTrue (cde.Wait (1000), "#1");

			try {
				Assert.IsTrue (Task.WaitAll (tasks, 1000), "#2");
			} catch (AggregateException e) {
				Assert.AreEqual (3, e.InnerExceptions.Count, "#3");
			}
		}

		[Test]
		public void WaitAny_ManyCanceled ()
		{
			var cancellation = new CancellationToken (true);
			var tasks = new[] {
				Task.Factory.StartNew (delegate { }, cancellation),
				Task.Factory.StartNew (delegate { }, cancellation),
				Task.Factory.StartNew (delegate { }, cancellation)
			};

			try {
				Assert.IsTrue (Task.WaitAll (tasks, 1000), "#1");
			} catch (AggregateException e) {
				Assert.AreEqual (3, e.InnerExceptions.Count, "#2");
			}
		}
		
		[Test]
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
		public void WaitAll_ManyTasks ()
		{
			for (int r = 0; r < 2000; ++r) {
				var tasks = new Task[60];

				for (int i = 0; i < tasks.Length; i++) {
					tasks[i] = Task.Factory.StartNew (delegate { Thread.Sleep (0); });
				}

				Assert.IsTrue (Task.WaitAll (tasks, 2000));
			}
		}

		[Test]
		public void WaitAll_Zero ()
		{
			Assert.IsFalse (Task.WaitAll (new Task[1] { new Task (delegate { }) }, 0), "#0");
			Assert.IsFalse (Task.WaitAll (new Task[1] { new Task (delegate { }) }, 10), "#1");
		}

		[Test]
		public void WaitAll_WithExceptions ()
		{
			InitWithDelegate (delegate { throw new ApplicationException (); });

			try {
				Task.WaitAll (tasks);
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.AreEqual (6, e.InnerExceptions.Count, "#2");
			}

			Assert.IsNotNull (tasks[0].Exception, "#3");
		}

		[Test]
		public void WaitAll_TimeoutWithExceptionsAfter ()
		{
			CountdownEvent cde = new CountdownEvent (2);
			var mre = new ManualResetEvent (false);
			var tasks = new[] {
				Task.Factory.StartNew (delegate { mre.WaitOne (); }),
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } }),
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } })
			};

			Assert.IsTrue (cde.Wait (100), "#1");
			Assert.IsFalse (Task.WaitAll (tasks, 100), "#2");

			mre.Set ();

			try {
				Assert.IsTrue (Task.WaitAll (tasks, 1000), "#3");
				Assert.Fail ("#4");
			} catch (AggregateException e) {
				Assert.AreEqual (2, e.InnerExceptions.Count, "#5");
			}
		}

		[Test]
		public void WaitAll_TimeoutWithExceptionsBefore ()
		{
			CountdownEvent cde = new CountdownEvent (2);
			var mre = new ManualResetEvent (false);
			var tasks = new[] {
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } }),
				Task.Factory.StartNew (delegate { try { throw new ApplicationException (); } finally { cde.Signal (); } }),
				Task.Factory.StartNew (delegate { mre.WaitOne (); })
			};

			Assert.IsTrue (cde.Wait (100), "#1");
			Assert.IsFalse (Task.WaitAll (tasks, 100), "#2");

			mre.Set ();

			try {
				Assert.IsTrue (Task.WaitAll (tasks, 1000), "#3");
				Assert.Fail ("#4");
			} catch (AggregateException e) {
				Assert.AreEqual (2, e.InnerExceptions.Count, "#5");
			}
		}

		[Test]
		public void WaitAll_Cancelled ()
		{
			var cancelation = new CancellationTokenSource ();
			var tasks = new Task[] {
				new Task (delegate { cancelation.Cancel (); }),
				new Task (delegate { }, cancelation.Token)
			};

			tasks[0].Start ();

			try {
				Task.WaitAll (tasks);
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				var inner = (TaskCanceledException) e.InnerException;
				Assert.AreEqual (tasks[1], inner.Task, "#2");
			}

			Assert.IsTrue (tasks[0].IsCompleted, "#3");
			Assert.IsTrue (tasks[1].IsCanceled, "#4");
		}

		[Test]
		public void WaitAllExceptionThenCancelled ()
		{
			var cancelation = new CancellationTokenSource ();
			var tasks = new Task[] {
				new Task (delegate { cancelation.Cancel (); throw new ApplicationException (); }),
				new Task (delegate { }, cancelation.Token)
			};

			tasks[0].Start ();

			try {
				Task.WaitAll (tasks);
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				Assert.IsInstanceOfType (typeof (ApplicationException), e.InnerException, "#2");
				var inner = (TaskCanceledException) e.InnerExceptions[1];
				Assert.AreEqual (tasks[1], inner.Task, "#3");
			}

			Assert.IsTrue (tasks[0].IsCompleted, "#4");
			Assert.IsTrue (tasks[1].IsCanceled, "#5");
		}

		[Test]
		public void WaitAll_StartedUnderWait ()
		{
			var task1 = new Task (delegate { });

			ThreadPool.QueueUserWorkItem (delegate {
				// Sleep little to let task to start and hit internal wait
				Thread.Sleep (20);
				task1.Start ();
			});

			Assert.IsTrue (Task.WaitAll (new [] { task1 }, 1000), "#1");
		}

		[Test]
		public void CancelBeforeStart ()
		{
			var src = new CancellationTokenSource ();

			Task t = new Task (delegate { }, src.Token);
			src.Cancel ();
			Assert.AreEqual (TaskStatus.Canceled, t.Status, "#1");

			try {
				t.Start ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void Wait_CancelledTask ()
		{
			var src = new CancellationTokenSource ();

			Task t = new Task (delegate { }, src.Token);
			src.Cancel ();

			try {
				t.Wait (1000);
				Assert.Fail ("#1");
			} catch (AggregateException e) {
				var details = (TaskCanceledException) e.InnerException;
				Assert.AreEqual (t, details.Task, "#1e");
			}

			try {
				t.Wait ();
				Assert.Fail ("#2");
			} catch (AggregateException e) {
				var details = (TaskCanceledException) e.InnerException;
				Assert.AreEqual (t, details.Task, "#2e");
				Assert.IsNull (details.Task.Exception, "#2e2");
			}
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
		public void ContinueWithInvalidArguments ()
		{
			var task = new Task (() => { });
			try {
				task.ContinueWith (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				task.ContinueWith (delegate { }, null);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				task.ContinueWith (delegate { }, TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.NotOnCanceled);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				task.ContinueWith (delegate { }, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.NotOnRanToCompletion);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}
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
				Assert.IsTrue (t.Wait(1000), "#4");
				Assert.IsTrue (cont.Wait(1000), "#5");
				
				Assert.IsNull(cont.Exception, "#1");
				Assert.IsNotNull(cont, "#2");
				Assert.IsTrue(result, "#3");
			});
		}
		
		[Test]
		public void ContinueWithOnAbortedTestCase()
		{
			bool result = false;
			bool taskResult = false;

			CancellationTokenSource src = new CancellationTokenSource ();
			Task t = new Task (delegate { taskResult = true; }, src.Token);

			Task cont = t.ContinueWith (delegate { result = true; },
				TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);

			src.Cancel ();

			Assert.AreEqual (TaskStatus.Canceled, t.Status, "#1a");
			Assert.IsTrue (cont.IsCompleted, "#1b");
			Assert.IsTrue (result, "#1c");

			try {
				t.Start ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}

			Assert.IsTrue (cont.Wait (1000), "#3");

			Assert.IsFalse (taskResult, "#4");

			Assert.IsNull (cont.Exception, "#5");
			Assert.AreEqual (TaskStatus.RanToCompletion, cont.Status, "#6");
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

		[Test]
		public void MultipleTasks()
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
				
				t1.Wait(2000);
				t2.Wait(2000);
				t3.Wait(2000);
				
				Assert.IsTrue(r1, "#1");
				Assert.IsTrue(r2, "#2");
				Assert.IsTrue(r3, "#3");
			}, 100);
		}
		
		[Test]
		public void WaitChildTestCase()
		{
			ParallelTestHelper.Repeat (delegate {
				bool r1 = false, r2 = false, r3 = false;
				var mre = new ManualResetEvent (false);
				
				Task t = Task.Factory.StartNew(delegate {
					Task.Factory.StartNew(delegate {
						r1 = true;
						mre.Set ();
					}, TaskCreationOptions.AttachedToParent);
					Task.Factory.StartNew(delegate {
						Assert.IsTrue (mre.WaitOne (1000), "#0");
						
						r2 = true;
					}, TaskCreationOptions.AttachedToParent);
					Task.Factory.StartNew(delegate {
						Assert.IsTrue (mre.WaitOne (1000), "#0");
						
						r3 = true;
					}, TaskCreationOptions.AttachedToParent);
				});
				
				Assert.IsTrue (t.Wait(2000), "#0");
				Assert.IsTrue(r2, "#1");
				Assert.IsTrue(r3, "#2");
				Assert.IsTrue(r1, "#3");
				Assert.AreEqual (TaskStatus.RanToCompletion, t.Status, "#4");
				}, 10);
		}

		[Test]
		public void DoubleWaitTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				Console.WriteLine ("run");
				var evt = new ManualResetEventSlim ();
				var t = Task.Factory.StartNew (() => evt.Wait (2000));
				var cntd = new CountdownEvent (2);

				bool r1 = false, r2 = false;
				ThreadPool.QueueUserWorkItem (delegate { cntd.Signal (); r1 = t.Wait (1000); Console.WriteLine ("out 1 {0}", r1); cntd.Signal (); });
				ThreadPool.QueueUserWorkItem (delegate { cntd.Signal (); r2 = t.Wait (1000); Console.WriteLine ("out 2 {0}", r2); cntd.Signal (); });

				cntd.Wait (2000);
				cntd.Reset ();
				evt.Set ();
				cntd.Wait (2000);
				Assert.IsTrue (r1);
				Assert.IsTrue (r2);
			}, 5);
		}

		[Test]
		public void DoubleTimeoutedWaitTest ()
		{
			var evt = new ManualResetEventSlim ();
			var t = new Task (delegate { });
			var cntd = new CountdownEvent (2);

			bool r1 = false, r2 = false;
			ThreadPool.QueueUserWorkItem (delegate { r1 = !t.Wait (100); cntd.Signal (); });
			ThreadPool.QueueUserWorkItem (delegate { r2 = !t.Wait (100); cntd.Signal (); });

			cntd.Wait (2000);
			Assert.IsTrue (r1);
			Assert.IsTrue (r2);
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
		public void WaitAny_WithNull ()
		{
			var tasks = new [] {
				Task.FromResult (2),
				null
			};

			try {
				Task.WaitAny (tasks);
				Assert.Fail ();
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void ContinueWith_StateValue ()
		{
			var t = Task.Factory.StartNew (l => {
				Assert.AreEqual (1, l, "a-1");
			}, 1);

			var c = t.ContinueWith ((a, b) => {
				Assert.AreEqual (t, a, "c-1");
				Assert.AreEqual (2, b, "c-2");
			}, 2);

			var d = t.ContinueWith ((a, b) => {
				Assert.AreEqual (t, a, "d-1");
				Assert.AreEqual (3, b, "d-2");
				return 77;
			}, 3);

			Assert.IsTrue (d.Wait (1000), "#1");

			Assert.AreEqual (1, t.AsyncState, "#2");
			Assert.AreEqual (2, c.AsyncState, "#3");
			Assert.AreEqual (3, d.AsyncState, "#4");
		}

		[Test]
		public void ContinueWith_StateValueGeneric ()
		{
			var t = Task<int>.Factory.StartNew (l => {
				Assert.AreEqual (1, l, "a-1");
				return 80;
			}, 1);

			var c = t.ContinueWith ((a, b) => {
				Assert.AreEqual (t, a, "c-1");
				Assert.AreEqual (2, b, "c-2");
				return "c";
			}, 2);

			var d = t.ContinueWith ((a, b) => {
				Assert.AreEqual (t, a, "d-1");
				Assert.AreEqual (3, b, "d-2");
				return 'd';
			}, 3);

			Assert.IsTrue (d.Wait (1000), "#1");

			Assert.AreEqual (1, t.AsyncState, "#2");
			Assert.AreEqual (80, t.Result, "#2r");
			Assert.AreEqual (2, c.AsyncState, "#3");
			Assert.AreEqual ("c", c.Result, "#3r");
			Assert.AreEqual (3, d.AsyncState, "#4");
			Assert.AreEqual ('d', d.Result, "#3r");
		}

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
