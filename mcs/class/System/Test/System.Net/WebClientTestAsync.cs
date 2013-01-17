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
			WebClient wc;
			bool progress_changed = false;
			bool completed = false;
			bool progress_changed_error = false;
			bool completed_error = false;

			int thread_id = Thread.CurrentThread.ManagedThreadId;

			wc = new WebClient ();

			wc.DownloadProgressChanged += delegate {
				progress_changed = true;
				if (Thread.CurrentThread.ManagedThreadId != thread_id)
					progress_changed_error = true;
			};
			wc.DownloadDataCompleted += delegate {
				completed = true;
				if (Thread.CurrentThread.ManagedThreadId != thread_id)
					completed_error = true;
			};

			MessagePumpSyncContext.Run (async () => {
				var url = Assembly.GetExecutingAssembly ().CodeBase;
				await wc.DownloadDataTaskAsync (url);
				Assert.AreEqual (Thread.CurrentThread.ManagedThreadId, thread_id);
			}, () => progress_changed && completed, 10000);

			Assert.IsTrue (progress_changed, "#1");
			Assert.IsFalse (progress_changed_error, "#2");
			Assert.IsTrue (completed, "#3");
			Assert.IsFalse (completed_error, "#4");
		}

		[Test]
		[Category("InetAccess")]
		public void DownloadFileTaskAsync ()
		{
			WebClient wc = new WebClient ();
			string filename = Path.GetTempFileName ();

			var task = wc.DownloadFileTaskAsync ("http://www.mono-project.com/", filename);
			Assert.IsTrue (task.Wait (15000));
			Assert.IsTrue (task.IsCompleted);
			
			File.Delete (filename);
		}

		[Test]
		[Category("InetAccess")]
		public void Cancellation ()
		{
			WebClient wc = new WebClient ();
			var progress = new ManualResetEvent (false);
			wc.DownloadProgressChanged += delegate {
				progress.Set ();
			};

			// Try downloading some large file, so we don't finish early.
			var url = "http://download.mono-project.com/archive/2.10.9/macos-10-x86/11/MonoFramework-MDK-2.10.9_11.macos10.xamarin.x86.dmg";
			var task = wc.DownloadDataTaskAsync (url);
			Assert.IsTrue (progress.WaitOne (15000), "#1");
			wc.CancelAsync ();

			try {
				task.Wait ();
				Assert.Fail ("#2");
			} catch (Exception ex) {
				if (ex is AggregateException)
					ex = ((AggregateException)ex).InnerException;
				Assert.That (ex is WebException || ex is OperationCanceledException, "#4");
				Assert.IsTrue (task.IsCanceled || task.IsFaulted, "#5");
			}
		}

		[Test]
		[Category("InetAccess")]
		public void DownloadMultiple ()
		{
			WebClient wc = new WebClient ();
			var t1 = wc.OpenReadTaskAsync ("http://www.google.com/");
			Assert.That (t1.Wait (15000));
			Assert.IsTrue (t1.IsCompleted, "#1");

			var t2 = wc.OpenReadTaskAsync ("http://www.mono-project.com/");
			Assert.That (t2.Wait (15000));
			Assert.IsTrue (t2.IsCompleted, "#2");

			var t3 = wc.DownloadStringTaskAsync ("http://www.google.com/");
			Assert.That (t3.Wait (15000));
			Assert.IsTrue (t3.IsCompleted, "#3");
		}

		[Test]
		[Category("InetAccess")]
		public void DownloadMultiple2 ()
		{
			WebClient wc = new WebClient ();

			MessagePumpSyncContext.Run (async () => {
				await wc.DownloadStringTaskAsync ("http://www.google.com/");
				await wc.DownloadDataTaskAsync ("http://www.mono-project.com/");
			}, null, 15000);
		}

		[Test]
		[Category("InetAccess")]
		public void DownloadMultiple3 ()
		{
			WebClient wc = new WebClient ();
			int thread_id = Thread.CurrentThread.ManagedThreadId;
			bool data_completed = false;
			bool string_completed = false;
			bool error = false;

			wc.DownloadDataCompleted += delegate {
				if (data_completed || (Thread.CurrentThread.ManagedThreadId != thread_id))
					error = true;
				data_completed = true;
			};
			wc.DownloadStringCompleted += delegate {
				if (string_completed || (Thread.CurrentThread.ManagedThreadId != thread_id))
					error = true;
				string_completed = true;
			};

			MessagePumpSyncContext.Run (async () => {
				await wc.DownloadStringTaskAsync ("http://www.google.com/");
				await wc.DownloadDataTaskAsync ("http://www.mono-project.com/");
			}, () => data_completed && string_completed, 15000);

			Assert.IsTrue (data_completed, "#1");
			Assert.IsTrue (string_completed, "#2");
			Assert.IsFalse (error, "#3");
		}

		public sealed class MessagePumpSyncContext : SynchronizationContext
		{
			private delegate void MyAction ();

			private readonly Queue<MyAction> queue = new Queue<MyAction> ();
			private readonly object sync = new object ();
			private readonly Func<bool> completed;
			private readonly int timeout;
			private bool running = true;

			MessagePumpSyncContext (Func<bool> completed, int timeout)
			{
				this.completed = completed;
				this.timeout = timeout;
			}

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

			bool IsCompleted {
				get {
					if (running)
						return false;
					if (completed != null)
						return completed ();
					return true;
				}
			}

			void RunMessagePump ()
			{
				while (running) {
					MyAction action;
					lock (sync) {
						while (queue.Count == 0) {
							if (IsCompleted)
								return;
							if (!Monitor.Wait (sync, timeout))
								throw new TimeoutException ();
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

			public static void Run (Func<Task> action, Func<bool> completed, int timeout)
			{
				var old_ctx = SynchronizationContext.Current;

				var ctx = new MessagePumpSyncContext (completed, timeout);
				try {
					SynchronizationContext.SetSynchronizationContext (ctx);

					var thread_id = Thread.CurrentThread.ManagedThreadId;

					var task = action ();
					task.ContinueWith ((t) => {
						ctx.running = false;
					}, TaskScheduler.FromCurrentSynchronizationContext ());

					ctx.RunMessagePump ();

					if (task.IsFaulted)
						throw task.Exception;
				} finally {
					SynchronizationContext.SetSynchronizationContext (old_ctx);
				}
			}

		}
	}
}
#endif
