// TaskSchedulerTest.cs
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
using System.Collections.Generic;

using NUnit.Framework;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskSchedulerTests
	{
		class LazyCatScheduler : TaskScheduler
		{
			public TaskStatus ExecuteInlineStatus
			{
				get;
				set;
			}

			protected override void QueueTask (Task task)
			{
				throw new NotImplementedException ();
			}

			protected override bool TryDequeue (Task task)
			{
				throw new NotImplementedException ();
			}

			protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
			{
				ExecuteInlineStatus = task.Status;
				return true;
			}

			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				throw new NotImplementedException ();
			}
		}

		class DefaultScheduler : TaskScheduler
		{
			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				throw new NotImplementedException ();
			}

			protected override void QueueTask (Task task)
			{
				throw new NotImplementedException ();
			}

			protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
			{
				throw new NotImplementedException ();
			}

			public void TestDefaultMethod ()
			{
				Assert.IsFalse (TryDequeue (null), "#1");
			}
		}

		[Test]
		public void FromCurrentSynchronizationContextTest_Invalid()
		{
			var c = SynchronizationContext.Current;
			try {
				SynchronizationContext.SetSynchronizationContext (null);
				TaskScheduler.FromCurrentSynchronizationContext ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			} finally {
				SynchronizationContext.SetSynchronizationContext (c);
			}
		}

		[Test]
		public void BasicRunSynchronouslyTest ()
		{
			bool ran = false;
			var t = new Task (() => ran = true);

			t.RunSynchronously ();
			Assert.IsTrue (t.IsCompleted);
			Assert.IsFalse (t.IsFaulted);
			Assert.IsFalse (t.IsCanceled);
			Assert.IsTrue (ran);
		}

		[Test]
		public void RunSynchronouslyButNoExecutionTest ()
		{
			TaskSchedulerException ex = null;

			var ts = new LazyCatScheduler ();
			Task t = new Task (() => {});

			try {
				t.RunSynchronously (ts);
			} catch (TaskSchedulerException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.IsNotNull (ex.InnerException);
			Assert.That (ex.InnerException, Is.TypeOf (typeof (InvalidOperationException)));
		}

		[Test]
		public void RunSynchronouslyTaskStatusTest ()
		{
			var ts = new LazyCatScheduler ();
			var t = new Task (() => { });

			try {
				t.RunSynchronously (ts);
			} catch {}
			Assert.AreEqual (TaskStatus.WaitingToRun, ts.ExecuteInlineStatus);
		}

		static int finalizerThreadId = -1;
	
		class FinalizerCatcher
		{
			~FinalizerCatcher ()
			{
				finalizerThreadId = Thread.CurrentThread.ManagedThreadId;
			}
		}

		[Test]
		public void DefaultBehaviourTest ()
		{
			var s = new DefaultScheduler ();
			s.TestDefaultMethod ();
		}

		// This test doesn't work if the GC uses multiple finalizer thread.
		// For now it's fine since only one thread is used
		[Test]
		// Depends on objects getting GCd plus installs an EH handler which catches
		// exceptions thrown by other tasks
		[Category ("NotWorking")]
		public void UnobservedTaskExceptionOnFinalizerThreadTest ()
		{
			var foo = new FinalizerCatcher ();
			foo = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			// Same than following test, if GC didn't run don't execute the rest of this test
			if (finalizerThreadId == -1)
				return;

			int evtThreadId = -2;
			TaskScheduler.UnobservedTaskException += delegate {
				evtThreadId = Thread.CurrentThread.ManagedThreadId;
			};
			var evt = new ManualResetEventSlim ();
			CreateAndForgetFaultedTask (evt);
 			evt.Wait (500);
			Thread.Sleep (100);
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			Assert.AreEqual (finalizerThreadId, evtThreadId, "Should be ran on finalizer thread");
		}

		[Test]
		// Depends on objects getting GCd plus installs an EH handler which catches
		// exceptions thrown by other tasks
		[Category ("NotWorking")]
		public void UnobservedTaskExceptionArgumentTest ()
		{
			bool ran = false;
			bool senderIsRight = false;
			UnobservedTaskExceptionEventArgs args = null;

			TaskScheduler.UnobservedTaskException += (o, a) => {
				senderIsRight = o.GetType ().ToString () == "System.Threading.Tasks.Task";
				args = a;
				ran = true;
			};

			var evt = new ManualResetEventSlim ();
			CreateAndForgetFaultedTask (evt);
			evt.Wait (500);
			Thread.Sleep (100);
			GC.Collect ();
			GC.WaitForPendingFinalizers ();

			// GC is too unreliable for some reason in that test, so backoff if finalizer wasn't ran
			// it needs to be run for the above test to work though (♥)
			if (!ran)
				return;

			Assert.IsNotNull (args.Exception);
			Assert.IsNotNull (args.Exception.InnerException);
			Assert.AreEqual ("foo", args.Exception.InnerException.Message);
			Assert.IsFalse (args.Observed);
			Assert.IsTrue (senderIsRight, "Sender is a task");
		}

		// We use this intermediary method to improve chances of GC kicking
		static void CreateAndForgetFaultedTask (ManualResetEventSlim evt)
		{
			Task.Factory.StartNew (() => { evt.Set (); throw new Exception ("foo"); });
		}
	}
}
#endif
