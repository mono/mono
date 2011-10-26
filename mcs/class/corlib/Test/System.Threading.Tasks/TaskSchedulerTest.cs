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

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskSchedulerTests
	{
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

		class LazyCatScheduler : TaskScheduler
		{
			public TaskStatus ExecuteInlineStatus {
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
			Assert.IsInstanceOfType (typeof (InvalidOperationException), ex.InnerException);
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
		public void UnobservedTaskExceptionOnFinalizerThreadTest ()
		{
			var foo = new FinalizerCatcher ();
			foo = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			Assert.Greater (finalizerThreadId, -1, finalizerThreadId.ToString ());

			int evtThreadId = -2;
			TaskScheduler.UnobservedTaskException += delegate {
				evtThreadId = Thread.CurrentThread.ManagedThreadId;
			};
			var evt = new ManualResetEventSlim ();
			var bar = Task.Factory.StartNew (() => { evt.Set (); throw new Exception ("foo"); });
			evt.Wait (500);
			Thread.Sleep (100);
			bar = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			Assert.AreEqual (finalizerThreadId, evtThreadId, "Should be ran on finalizer thread");
		}

		[Test]
		public void UnobservedTaskExceptionArgumentTest ()
		{
			bool ran = false;
			bool senderIsRight = false;
			bool argsExceptionNotNull = false;
			bool argsMessageRight = false;
			bool argsObserved = false;
			UnobservedTaskExceptionEventArgs args = null;

			TaskScheduler.UnobservedTaskException += (o, a) => {
				senderIsRight = o.GetType ().ToString () == "System.Threading.Tasks.Task";
				args = a;
				ran = true;
			};

			var evt = new ManualResetEventSlim ();
			var bar = Task.Factory.StartNew (() => { evt.Set (); throw new Exception ("foo"); });
			evt.Wait (500);
			Thread.Sleep (100);
			bar = null;
			GC.Collect ();
			GC.WaitForPendingFinalizers ();

			// GC is too unreliable for some reason in that test, so backoff if finalizer wasn't ran
			// it needs to be run for the above test to work though (♥)
			if (!ran)
				return;

			if (args != null) {
				argsExceptionNotNull = args.Exception != null;
				if (argsExceptionNotNull)
					argsMessageRight = args.Exception.InnerException.Message == "foo";
				argsObserved = !args.Observed;
			}

			Assert.IsTrue (argsExceptionNotNull, "Exception not null");
			Assert.IsTrue (argsMessageRight, "Right exception is registered");
			Assert.IsTrue (argsObserved, "Task exception wasn't observed");
			Assert.IsTrue (senderIsRight, "Sender is a task");
		}
	}
}
#endif
