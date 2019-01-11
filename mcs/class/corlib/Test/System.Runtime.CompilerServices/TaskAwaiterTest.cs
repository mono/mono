//
// TaskAwaiterTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;

namespace MonoTests.System.Runtime.CompilerServices
{
	[TestFixture]
	public class TaskAwaiterTest
	{
		class Scheduler : TaskScheduler
		{
			string name;
			int ic, qc;

			public Scheduler (string name)
			{
				this.name = name;
			}

			public int InlineCalls { get { return ic; } }
			public int QueueCalls { get { return qc; } }

			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				throw new NotImplementedException ();
			}

			protected override void QueueTask (Task task)
			{
				Interlocked.Increment (ref qc);
				ThreadPool.QueueUserWorkItem (o => {
					TryExecuteTask (task);
				});
			}

			protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
			{
				Interlocked.Increment (ref ic);
				return false;
			}

			public override string ToString ()
			{
				return "Scheduler-" + name;
			}
		}

		class SingleThreadSynchronizationContext : SynchronizationContext
		{
			readonly Queue _queue = new Queue ();

			public void RunOnCurrentThread ()
			{
				while (_queue.Count != 0) {
					var workItem = (KeyValuePair<SendOrPostCallback, object>) _queue.Dequeue ();
					workItem.Key (workItem.Value);
				}
			}
				
			public override void Post (SendOrPostCallback d, object state)
			{
				if (d == null) {
					throw new ArgumentNullException ("d");
				}

				_queue.Enqueue (new KeyValuePair<SendOrPostCallback, object> (d, state));
			}

			public override void Send (SendOrPostCallback d, object state)
			{
				throw new NotSupportedException ("Synchronously sending is not supported.");
			}
		}

		class NestedSynchronizationContext : SynchronizationContext
		{
			Thread thread;
			readonly ConcurrentQueue<Tuple<SendOrPostCallback, object, ExecutionContext>> workQueue = new ConcurrentQueue<Tuple<SendOrPostCallback, object, ExecutionContext>> ();
			readonly AutoResetEvent workReady = new AutoResetEvent (false);

			public NestedSynchronizationContext ()
			{
				thread = new Thread (WorkerThreadProc) { IsBackground = true };
				thread.Start ();
			}

			public override void Post (SendOrPostCallback d, object state)
			{
				var context = ExecutionContext.Capture ();
				workQueue.Enqueue (Tuple.Create (d, state, context));
				workReady.Set ();
			}

			void WorkerThreadProc ()
			{
				if (!workReady.WaitOne (10000))
					return;

				Tuple<SendOrPostCallback, object, ExecutionContext> work;

				while (workQueue.TryDequeue (out work)) {
					ExecutionContext.Run (work.Item3, _ => {
						var oldSyncContext = SynchronizationContext.Current;
						SynchronizationContext.SetSynchronizationContext (this);
						work.Item1 (_);
						SynchronizationContext.SetSynchronizationContext (oldSyncContext);
					}, work.Item2);	
				}
			}
		}

		string progress;
		SynchronizationContext sc;
		ManualResetEvent mre;

		[SetUp]
		public void Setup ()
		{
			sc = SynchronizationContext.Current;
		}

		[TearDown]
		public void TearDown ()
		{
			SynchronizationContext.SetSynchronizationContext (sc);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void GetResultFaulted ()
		{
			TaskAwaiter awaiter;

			var task = new Task (() => { throw new ApplicationException (); });
			awaiter = task.GetAwaiter ();
			task.RunSynchronously (TaskScheduler.Current);


			Assert.IsTrue (awaiter.IsCompleted);

			try {
				awaiter.GetResult ();
				Assert.Fail ();
			} catch (ApplicationException) {
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void GetResultCanceled ()
		{
			TaskAwaiter awaiter;

			var token = new CancellationToken (true);
			var task = new Task (() => { }, token);
			awaiter = task.GetAwaiter ();

			try {
				awaiter.GetResult ();
				Assert.Fail ();
			} catch (TaskCanceledException) {
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void GetResultWaitOnCompletion ()
		{
			TaskAwaiter awaiter;
				
			var task = Task.Delay (30);
			awaiter = task.GetAwaiter ();
				
			awaiter.GetResult ();
			Assert.AreEqual (TaskStatus.RanToCompletion, task.Status);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void CustomScheduler ()
		{
			// some test runners (e.g. Touch.Unit) will execute this on the main thread and that would lock them
			if (!Thread.CurrentThread.IsBackground)
				Assert.Ignore ("Current thread is not running in the background.");

			var a = new Scheduler ("a");
			var b = new Scheduler ("b");

			var t = TestCS (a, b);
			Assert.IsTrue (t.Wait (3000), "#0");
			Assert.AreEqual (0, t.Result, "#1");
			Assert.AreEqual (0, b.InlineCalls, "#2b");
			Assert.IsTrue (a.QueueCalls == 1 || a.QueueCalls == 2, "#3a");
			Assert.AreEqual (1, b.QueueCalls, "#3b");
		}

		static async Task<int> TestCS (TaskScheduler schedulerA, TaskScheduler schedulerB)
		{
			var res = await Task.Factory.StartNew (async () => {
				if (TaskScheduler.Current != schedulerA)
					return 1;

				await Task.Factory.StartNew (
					() => {
						if (TaskScheduler.Current != schedulerB)
							return 2;

						return 0;
					}, CancellationToken.None, TaskCreationOptions.None, schedulerB);

				if (TaskScheduler.Current != schedulerA)
					return 3;

				return 0;
			}, CancellationToken.None, TaskCreationOptions.None, schedulerA);

			return res.Result;
		}

		[Test]
		[Ignore ("Incompatible with nunitlite")]
		[Category ("MultiThreaded")]
		public void FinishedTaskOnCompleted ()
		{
			var mres = new ManualResetEvent (false);
			var mres2 = new ManualResetEvent (false);

			var tcs = new TaskCompletionSource<object> ();
			tcs.SetResult (null);
			var task = tcs.Task;

			var awaiter = task.GetAwaiter ();
			Assert.IsTrue (awaiter.IsCompleted, "#1");

			awaiter.OnCompleted(() => { 
				if (mres.WaitOne (1000))
					mres2.Set ();
			});

			mres.Set ();
			// this will only terminate correctly if the test was not executed from the main thread
			// e.g. nunitlite/Touch.Unit defaults to run tests on the main thread and this will return false
			Assert.AreEqual (Thread.CurrentThread.IsBackground, mres2.WaitOne (2000), "#2");;
		}

		[Test]
		[Category ("MultiThreaded")]
		public void CompletionOnSameCustomSynchronizationContext ()
		{
			progress = "";
			var syncContext = new SingleThreadSynchronizationContext ();
			SynchronizationContext.SetSynchronizationContext (syncContext);

			syncContext.Post (delegate {
				Go (syncContext);
			}, null);

			// Custom message loop
			var cts = new CancellationTokenSource ();
			cts.CancelAfter (5000);
			while (progress.Length != 3 && !cts.IsCancellationRequested) {
				syncContext.RunOnCurrentThread ();
				Thread.Sleep (0);
			}

			Assert.AreEqual ("123", progress);
		}

		async void Go (SynchronizationContext ctx)
		{
			await Wait (ctx);

			progress += "2";
		}

		async Task Wait (SynchronizationContext ctx)
		{
			await Task.Delay (10); // Force block suspend/return

			ctx.Post (l => progress += "3", null);

			progress += "1";

			// Exiting same context - no need to post continuation
		}

		[Test]
		[Category ("MultiThreaded")]
		public void CompletionOnDifferentCustomSynchronizationContext ()
		{
			mre = new ManualResetEvent (false);
			progress = "";
			var syncContext = new SingleThreadSynchronizationContext ();
			SynchronizationContext.SetSynchronizationContext (syncContext);

			syncContext.Post (delegate {
				Task t = new Task (delegate() { });
				Go2 (syncContext, t);
				t.Start ();
			}, null);

			// Custom message loop
			var cts = new CancellationTokenSource ();
			cts.CancelAfter (5000);
			while (progress.Length != 3 && !cts.IsCancellationRequested) {
				syncContext.RunOnCurrentThread ();
				Thread.Sleep (0);
			}

			Assert.AreEqual ("13xa2", progress);
		}

		async void Go2 (SynchronizationContext ctx, Task t)
		{
			await Wait2 (ctx, t);

			progress += "a";

			if (mre.WaitOne (5000))
				progress += "2";
			else
				progress += "b";
		}

		async Task Wait2 (SynchronizationContext ctx, Task t)
		{
			await t; // Force block suspend/return

			ctx.Post (l => {
				progress += "3";
				mre.Set ();
				progress += "x";
			}, null);

			progress += "1";

			SynchronizationContext.SetSynchronizationContext (null);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void NestedLeakingSynchronizationContext ()
		{
			var sc = SynchronizationContext.Current;
			if (sc == null)
				Assert.IsTrue (NestedLeakingSynchronizationContext_MainAsync (sc).Wait (5000), "#1");
			else
				Assert.Ignore ("NestedSynchronizationContext may never complete on custom context");
		}

		static async Task NestedLeakingSynchronizationContext_MainAsync (SynchronizationContext sc)
		{
			Assert.AreSame (sc, SynchronizationContext.Current, "#1");
			await NestedLeakingSynchronizationContext_DoWorkAsync ();
			Assert.AreSame (sc, SynchronizationContext.Current, "#2");
		}

		static async Task NestedLeakingSynchronizationContext_DoWorkAsync ()
		{
			var sc = new NestedSynchronizationContext ();
			SynchronizationContext.SetSynchronizationContext (sc);

			Assert.AreSame (sc, SynchronizationContext.Current);
			await Task.Yield ();
			Assert.AreSame (sc, SynchronizationContext.Current);
		}
	}
}

