//
// TaskFactoryTest.cs
//
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar <marek.safar@gmail.com>
// 
// Copyright (c) 2010 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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
	public class TaskFactoryTests
	{
		class CompletedAsyncResult : IAsyncResult
		{
			public object AsyncState
			{
				get { throw new NotImplementedException (); }
			}

			public WaitHandle AsyncWaitHandle
			{
				get { throw new NotImplementedException (); }
			}

			public bool CompletedSynchronously
			{
				get { throw new NotImplementedException (); }
			}

			public bool IsCompleted
			{
				get { return true; }
			}
		}

		class TestAsyncResult : IAsyncResult
		{
			WaitHandle wh = new ManualResetEvent (true);

			public object AsyncState
			{
				get { throw new NotImplementedException (); }
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					return wh;
				}
			}

			public bool CompletedSynchronously
			{
				get { throw new NotImplementedException (); }
			}

			public bool IsCompleted
			{
				get { return false; }
			}
		}

		class TestScheduler : TaskScheduler
		{
			public bool ExecutedInline { get; set; }

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
				if (taskWasPreviouslyQueued)
					throw new ArgumentException ("taskWasPreviouslyQueued");

				if (task.Status != TaskStatus.WaitingToRun)
					throw new ArgumentException ("task.Status");

				ExecutedInline = true;
				return TryExecuteTask (task);
			}

			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				throw new NotImplementedException ();
			}
		}


		TaskFactory factory;

		[SetUp]
		public void Setup ()
		{
			this.factory = Task.Factory;
		}

		[Test]
		public void StartNewTest ()
		{
			bool result = false;
			factory.StartNew (() => result = true).Wait ();
			Assert.IsTrue (result);
		}

		[Test]
		public void NoDefaultScheduler ()
		{
			Assert.IsNull (factory.Scheduler, "#1");
		}

		[Test]
		public void ContinueWhenAll_Simple ()
		{
			var mre = new ManualResetEventSlim (false);

			Task[] tasks = new Task[3];
			tasks[0] = new Task (() => { Thread.Sleep (0); Assert.IsTrue (mre.Wait (3000)); });
			tasks[1] = new Task (() => { Assert.IsTrue (mre.Wait (3000)); });
			tasks[2] = new Task (() => { Assert.IsTrue (mre.Wait (3000)); });

			bool ran = false;
			Task cont = factory.ContinueWhenAll (tasks, ts => {
				Assert.AreEqual (tasks, ts, "#0");
				ran = true;
			});

			foreach (Task t in tasks)
				t.Start ();

			mre.Set ();

			Assert.IsTrue (cont.Wait (1000), "#1");
			Assert.IsTrue (ran, "#2");
		}

		[Test]
		public void ContinueWhenAll_WithMixedCompletionState ()
		{
			var mre = new ManualResetEventSlim ();
			var task = Task.Factory.StartNew (() => mre.Wait (200));
			var contFailed = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnFaulted);
			var contCanceled = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnCanceled);
			var contSuccess = task.ContinueWith (t => {}, TaskContinuationOptions.OnlyOnRanToCompletion);
			bool ran = false;

			var cont = Task.Factory.ContinueWhenAll (new Task[] { contFailed, contCanceled, contSuccess }, _ => ran = true);

			mre.Set ();
			cont.Wait (200);

			Assert.IsTrue (ran);
			Assert.AreEqual (TaskStatus.RanToCompletion, cont.Status);
		}

		[Test]
		public void ContinueWhenAll_InvalidArguments ()
		{
			try {
				factory.ContinueWhenAll (null, delegate { });
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				factory.ContinueWhenAll (new Task[0], delegate { });
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				factory.ContinueWhenAll (new Task[] { null }, delegate { });
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			var tasks = new Task [] {
				factory.StartNew (delegate {})
			};

			try {
				factory.ContinueWhenAll (tasks, null);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			try {
				factory.ContinueWhenAll (tasks, delegate { }, CancellationToken.None, TaskContinuationOptions.None, null);
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}

			try {
				factory.ContinueWhenAll (tasks, delegate { }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, null);
				Assert.Fail ("#6");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void ContinueWhenAll_WithExceptions ()
		{
			var t1 = Task.Factory.StartNew (() => { throw new ApplicationException ("Foo"); });
			var t2 = Task.Factory.StartNew (() => { throw new ApplicationException ("Bar"); });

			var cont = Task.Factory.ContinueWhenAll (new[] { t1, t2 }, delegate {});
			cont.Wait (200);

			Assert.IsTrue (t1.IsFaulted);
			Assert.IsTrue (t2.IsFaulted);
			Assert.AreEqual (TaskStatus.RanToCompletion, cont.Status);
		}

		[Test]
		public void ContinueWhenAny_Simple ()
		{
			var t1 = new ManualResetEvent (false);
			var t2 = new ManualResetEvent (false);

			var tasks = new Task[2] {
				Task.Factory.StartNew (() => { t1.WaitOne (3000); }),
				Task.Factory.StartNew (() => { t2.WaitOne (3000); })
			};

			bool ran = false;
			var ct = new CancellationToken ();
			Task cont = factory.ContinueWhenAny (tasks, t => {
				Assert.AreEqual (tasks[0], t, "#1");
				ran = true;
			}, ct);

			Assert.AreEqual (TaskStatus.WaitingForActivation, cont.Status, "#2");

			t1.Set ();

			Assert.IsTrue (cont.Wait (2000), "#10");
			Assert.IsTrue (ran, "#11");

			t2.Set ();
		}

		[Test]
		public void ContinueWhenAny_InvalidArguments ()
		{
			try {
				factory.ContinueWhenAny (null, delegate { });
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				factory.ContinueWhenAny (new Task[0], delegate { });
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				factory.ContinueWhenAny (new Task[] { null }, delegate { });
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			var tasks = new Task [] {
				factory.StartNew (delegate {})
			};

			try {
				factory.ContinueWhenAny (tasks, null);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			try {
				factory.ContinueWhenAny (tasks, delegate { }, CancellationToken.None, TaskContinuationOptions.None, null);
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}

			try {
				factory.ContinueWhenAny (tasks, delegate { }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, null);
				Assert.Fail ("#6");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void FromAsyncBeginInvoke_WithResult ()
		{
			bool result = false;

			Func<int, int> func = (i) => {
				Assert.IsTrue (Thread.CurrentThread.IsThreadPoolThread);
				result = true; return i + 3;
			};

			var task = factory.FromAsync<int, int> (func.BeginInvoke, func.EndInvoke, 1, "state", TaskCreationOptions.AttachedToParent);
			Assert.IsTrue (task.Wait (5000), "#1");
			Assert.IsTrue (result, "#2");
			Assert.AreEqual (4, task.Result, "#3");
			Assert.AreEqual ("state", (string) task.AsyncState, "#4");
			Assert.AreEqual (TaskCreationOptions.AttachedToParent, task.CreationOptions, "#5");
		}

		[Test]
		public void FromAsyncBeginMethod_DirectResult ()
		{
			bool result = false;
			bool continuationTest = false;

			Func<int, int> func = (i) => { result = true; return i + 3; };
			Task<int> task = factory.FromAsync<int> (func.BeginInvoke (1, delegate { }, null), func.EndInvoke);
			var cont = task.ContinueWith (_ => continuationTest = true, TaskContinuationOptions.ExecuteSynchronously);
			task.Wait ();
			cont.Wait ();

			Assert.IsTrue (result);
			Assert.IsTrue (continuationTest);
			Assert.AreEqual (4, task.Result);
		}

		[Test]
		public void FromAsyncBeginMethod_Exception ()
		{
			bool result = false;
			bool continuationTest = false;

			Func<int, int> func = (i) => { result = true; throw new ApplicationException ("bleh"); };
			Task<int> task = factory.FromAsync<int, int> (func.BeginInvoke, func.EndInvoke, 1, null);
			var cont = task.ContinueWith (_ => continuationTest = true, TaskContinuationOptions.ExecuteSynchronously);
			try {
				task.Wait (2000);
			} catch { }
			Assert.IsTrue (cont.Wait (2000), "#1");

			Assert.IsTrue (result);
			Assert.IsTrue (continuationTest);
			Assert.IsNotNull (task.Exception);
			var agg = task.Exception;
			Assert.AreEqual (1, agg.InnerExceptions.Count);
			Assert.That (agg.InnerExceptions[0], Is.TypeOf (typeof (ApplicationException)));
			Assert.AreEqual (TaskStatus.Faulted, task.Status);

			try {
				var a = task.Result;
				Assert.Fail ();
			} catch (AggregateException) {
			}
		}

		[Test]
		public void FromAsync_ArgumentsCheck ()
		{
			var result = new CompletedAsyncResult ();
			try {
				factory.FromAsync (null, l => { });
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync (result, null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync (result, l => { }, TaskCreationOptions.LongRunning);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				factory.FromAsync (result, l => { }, TaskCreationOptions.PreferFairness);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				factory.FromAsync (result, l => { }, TaskCreationOptions.None, null);
				Assert.Fail ("#5");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync (null, l => { }, null, TaskCreationOptions.None);
				Assert.Fail ("#6");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync ((a, b) => null, l => { }, null, TaskCreationOptions.LongRunning);
				Assert.Fail ("#7");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void FromAsync_Completed ()
		{
			var completed = new CompletedAsyncResult ();
			bool? valid = null;

			Action<IAsyncResult> end = l => {
				Assert.IsFalse (Thread.CurrentThread.IsThreadPoolThread, "#2");
				valid = l == completed;
			};
			Task task = factory.FromAsync (completed, end);
			Assert.IsTrue (valid == true, "#1");
		}

		[Test]
		public void FromAsync_CompletedWithException ()
		{
			var completed = new CompletedAsyncResult ();

			Action<IAsyncResult> end = l => {
				throw new ApplicationException ();
			};
			Task task = factory.FromAsync (completed, end);
			Assert.AreEqual (TaskStatus.Faulted, task.Status, "#1");
		}

		[Test]
		public void FromAsync_CompletedCanceled ()
		{
			var completed = new CompletedAsyncResult ();

			Action<IAsyncResult> end = l => {
				throw new OperationCanceledException ();
			};
			Task task = factory.FromAsync (completed, end);
			Assert.AreEqual (TaskStatus.Canceled, task.Status, "#1");
			Assert.IsNull (task.Exception, "#2");
		}

		[Test]
		public void FromAsync_SimpleAsyncResult ()
		{
			var result = new TestAsyncResult ();
			bool called = false;

			var task = factory.FromAsync (result, l => {
				called = true;
			});

			Assert.IsTrue (task.Wait (1000), "#1");
			Assert.IsTrue (called, "#2");
		}

		[Test]
		public void FromAsync_ResultException ()
		{
			var result = new TestAsyncResult ();

			var task = factory.FromAsync (result, l => {
				throw new ApplicationException ();
			});

			try {
				Assert.IsFalse (task.Wait (1000), "#1");
			} catch (AggregateException) {
			}

			Assert.AreEqual (TaskStatus.Faulted, task.Status, "#2");
		}

		[Test]
		public void FromAsync_ReturnInt ()
		{
			var result = new TestAsyncResult ();
			bool called = false;

			var task = factory.FromAsync<int> (result, l => {
				called = true;
				return 4;
			});

			Assert.IsTrue (task.Wait (1000), "#1");
			Assert.IsTrue (called, "#2");
			Assert.AreEqual (4, task.Result, "#3");
		}

		[Test]
		public void FromAsync_Scheduler_Explicit ()
		{
			var result = new TestAsyncResult ();
			bool called = false;
			var scheduler = new TestScheduler ();

			var task = factory.FromAsync (result, l => {
				called = true;
			}, TaskCreationOptions.None, scheduler);

			Assert.IsTrue (task.Wait (5000), "#1");
			Assert.IsTrue (called, "#2");
			Assert.IsTrue (scheduler.ExecutedInline, "#3");
		}

		[Test]
		public void FromAsync_Scheduler_Implicit ()
		{
			var result = new TestAsyncResult ();
			bool called = false;
			var scheduler = new TestScheduler ();

			factory = new TaskFactory (scheduler);

			Task task = factory.FromAsync (result, l => {
				Assert.IsTrue (Thread.CurrentThread.IsThreadPoolThread, "#6");
				called = true;
			}, TaskCreationOptions.AttachedToParent);

			Assert.AreEqual (TaskCreationOptions.AttachedToParent, task.CreationOptions, "#1");
			Assert.IsNull (task.AsyncState, "#2");
			Assert.IsTrue (task.Wait (5000), "#3");
			Assert.IsTrue (called, "#4");
			Assert.IsTrue (scheduler.ExecutedInline, "#5");
		}

		[Test]
		public void FromAsync_BeginCallback ()
		{
			bool called = false;
			bool called2 = false;

			var task = factory.FromAsync (
				(a, b, c) => {
					if (a != "h")
						Assert.Fail ("#10");

					if ((TaskCreationOptions) c != TaskCreationOptions.AttachedToParent)
						Assert.Fail ("#11");

					Assert.IsFalse (Thread.CurrentThread.IsThreadPoolThread, "#12");

					called2 = true;
					b.Invoke (null);
					return null;
				},
				l => {
					called = true;
				},
				"h", TaskCreationOptions.AttachedToParent);

			Assert.AreEqual (TaskCreationOptions.None, task.CreationOptions, "#1");
			Assert.AreEqual (TaskCreationOptions.AttachedToParent, (TaskCreationOptions) task.AsyncState, "#2");
			Assert.IsTrue (task.Wait (5000), "#3");
			Assert.IsTrue (called, "#4");
			Assert.IsTrue (called2, "#5");
		}

		[Test]
		public void StartNewCancelled ()
		{
			var cts = new CancellationTokenSource ();
			cts.Cancel ();

			var task = factory.StartNew (() => Assert.Fail ("Should never be called"), cts.Token);
			try {
				task.Start ();
			} catch (InvalidOperationException) {
			}

			Assert.IsTrue (task.IsCanceled, "#2");
		}
	}
}
#endif
