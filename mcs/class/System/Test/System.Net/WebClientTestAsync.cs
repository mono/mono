//
// System.Net.WebClientTestAsync
//
// Authors:
//      Martin Baulig (martin.baulig@googlemail.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
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
#if NET_4_5
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class WebClientTestAsync
	{
		[Test]
		[Category("Async")]
		public void DownloadData ()
		{
			SyncContextTest.Run ();
		}

		[Test]
		[Category("Async")]
		public void DownloadFileTaskAsync ()
		{
			WebClient wc = new WebClient ();
			string filename = Path.GetTempFileName ();

			var task = wc.DownloadFileTaskAsync ("http://www.mono-project.com/", filename);
			task.Wait ();
			
			Assert.IsTrue (task.IsCompleted);
			
			File.Delete (filename);
		}

		[Test]
		[Category("Async")]
		public void Cancelation ()
		{
			WebClient wc = new WebClient ();
			var progress = new ManualResetEvent (false);
			wc.DownloadProgressChanged += delegate {
				progress.Set ();
			};

			// Try downloading some large file, so we don't finish early.
			var url = "http://download.mono-project.com/archive/2.10.9/macos-10-x86/11/MonoFramework-MDK-2.10.9_11.macos10.xamarin.x86.dmg";
			var task = wc.DownloadDataTaskAsync (url);
			Assert.IsTrue (progress.WaitOne (10000), "#1");
			wc.CancelAsync ();

			try {
				task.Wait ();
				Assert.Fail ("#2");
			} catch (Exception ex) {
				if (ex is AggregateException)
					ex = ((AggregateException)ex).InnerException;
				Assert.IsInstanceOfType (typeof (OperationCanceledException), ex, "#3");
				Assert.IsTrue (task.IsCanceled, "#4");
			}
		}

		[Test]
		[Category("Async")]
		public void DownloadMultiple ()
		{
			WebClient wc = new WebClient ();
			var t1 = wc.OpenReadTaskAsync ("http://www.google.com/");
			t1.Wait ();
			Assert.IsTrue (t1.IsCompleted, "#1");

			var t2 = wc.OpenReadTaskAsync ("http://www.mono-project.com/");
			t2.Wait ();
			Assert.IsTrue (t2.IsCompleted, "#2");

			var t3 = wc.DownloadStringTaskAsync ("http://www.google.com/");
			t3.Wait ();
			Assert.IsTrue (t3.IsCompleted, "#3");
		}

		private class SyncContextTest
		{
			SyncContext ctx;
			WebClient wc;
			bool progress_changed;
			bool completed;
			bool progress_changed_error;
			bool completed_error;
			bool done;
			bool done_error;
			int thread_id;

			SyncContextTest ()
			{
				ctx = new SyncContext ();
				wc = new WebClient ();

				wc.DownloadProgressChanged += HandleDownloadProgressChanged;
				wc.DownloadDataCompleted += HandleDownloadDataCompleted;
			}

			void HandleDownloadDataCompleted (object sender, DownloadDataCompletedEventArgs e)
			{
				completed = true;
				if (Thread.CurrentThread.ManagedThreadId != thread_id)
					completed_error = true;
				CheckCompleted ();
			}

			void HandleDownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e)
			{
				progress_changed = true;
				if (Thread.CurrentThread.ManagedThreadId != thread_id)
					progress_changed_error = true;
				CheckCompleted ();
			}

			void CheckCompleted ()
			{
				if (!progress_changed || !completed || !done)
					return;
				ThreadPool.QueueUserWorkItem (state => {
					Thread.Sleep (250);
					ctx.Cancel ();
				});
			}

			void SetupTimeoutHandler ()
			{
				ThreadPool.QueueUserWorkItem (state => {
					Thread.Sleep (30000);
					wc.CancelAsync ();
					ctx.Cancel ();
				});
			}

			public static void Run ()
			{
				SyncContextTest test = new SyncContextTest ();
				test.DoRun ();
			}

			void DoRun ()
			{
				SetupTimeoutHandler ();

				var old_ctx = SynchronizationContext.Current;
				try {
					SynchronizationContext.SetSynchronizationContext (ctx);

					thread_id = Thread.CurrentThread.ManagedThreadId;

					ctx.Post (obj => DownloadDataAsync (), null);

					ctx.RunMessagePump ();
				} catch (Exception ex) {
					Assert.Fail ("Unknown exception: {0}", ex);
				} finally {
					SynchronizationContext.SetSynchronizationContext (old_ctx);
				}

				Assert.IsTrue (progress_changed, "#1");
				Assert.IsFalse (progress_changed_error, "#2");
				Assert.IsTrue (completed, "#3");
				Assert.IsFalse (completed_error, "#4");
				Assert.IsTrue (done, "#5");
				Assert.IsFalse (done_error, "#6");
			}

			async void DownloadDataAsync ()
			{
				var url = Assembly.GetExecutingAssembly ().CodeBase;
				await wc.DownloadDataTaskAsync (url);
				done = true;
				if (Thread.CurrentThread.ManagedThreadId != thread_id)
					done_error = true;
				CheckCompleted ();
			}
		}

		public class SyncContext : SynchronizationContext
		{
			private delegate void MyAction ();

			private readonly Queue<MyAction> queue = new Queue<MyAction> ();
			private readonly object sync = new object ();
			private bool running = true;

			public override void Send (SendOrPostCallback d, object state)
			{
				throw new InvalidOperationException ();
			}

			public override void Post (SendOrPostCallback d, object state)
			{
				lock (sync) {
					queue.Enqueue (() => d (state));
					Monitor.Pulse (sync);
				}
			}

			public void RunMessagePump ()
			{
				while (running) {
					MyAction action;
					lock (sync) {
						while (queue.Count == 0) {
							if (!running)
								return;
							Monitor.Wait (sync);
						}
						action = queue.Dequeue ();
					}
					action ();
				}
			}

			public void Cancel ()
			{
				lock (sync) {
					running = false;
					Monitor.Pulse (sync);
				}
			}
		}
	}
}
#endif
