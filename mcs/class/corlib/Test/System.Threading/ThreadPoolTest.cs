//
// ThreadPoolTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class ThreadPoolTests
	{
		int minWorkerThreads;
		int minCompletionPortThreads;
		int maxWorkerThreads;
		int maxCompletionPortThreads;

		[SetUp]
		public void SetUp ()
		{
			ThreadPool.GetMinThreads (out minWorkerThreads, out minCompletionPortThreads);
			ThreadPool.GetMaxThreads (out maxWorkerThreads, out maxCompletionPortThreads);
		}

		[TearDown]
		public void TearDown ()
		{
			ThreadPool.SetMinThreads (minWorkerThreads, minCompletionPortThreads);
			ThreadPool.SetMaxThreads (maxWorkerThreads, maxCompletionPortThreads);
		}

		[Test]
		public void RegisterWaitForSingleObject_InvalidArguments ()
		{
			try {
				ThreadPool.RegisterWaitForSingleObject (null, delegate {}, new object (), 100, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				ThreadPool.RegisterWaitForSingleObject (new Mutex (), null, new object (), 100, false);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}			
		}

		[Test]
		public void UnsafeQueueUserWorkItem_InvalidArguments ()
		{
			try {
				ThreadPool.UnsafeQueueUserWorkItem (null, 1);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void QueueUserWorkItem ()
		{
			int n = 100000;
			int total = 0, sum = 0;
			for (int i = 0; i < n; ++i) {
				if (i % 2 == 0)
					ThreadPool.QueueUserWorkItem (_ => { Interlocked.Decrement (ref sum); Interlocked.Increment (ref total); });
				else
					ThreadPool.QueueUserWorkItem (_ => { Interlocked.Increment (ref sum); Interlocked.Increment (ref total); });
			}
			var sw = Stopwatch.StartNew ();
			while ((total != n || sum != 0) && sw.Elapsed.TotalSeconds < 60)
				Thread.Sleep (1000);
			Assert.IsTrue (total == n, "#1");
			Assert.IsTrue (sum   == 0, "#2");
		}

		event WaitCallback e;

		[Test]
		[Category ("MultiThreaded")]
		public void UnsafeQueueUserWorkItem_MulticastDelegate ()
		{
			CountdownEvent ev = new CountdownEvent (2);

			e += delegate {
				ev.Signal ();
			};

			e += delegate {
				ev.Signal ();
			};

			ThreadPool.UnsafeQueueUserWorkItem (e, null);
			Assert.IsTrue (ev.Wait (3000));
		}

		[Test]
		[Category ("MultiThreaded")]
		public void SetAndGetMinThreads ()
		{
			int workerThreads, completionPortThreads;
			int workerThreads_new, completionPortThreads_new;

			ThreadPool.GetMinThreads (out workerThreads, out completionPortThreads);
			Assert.IsTrue (workerThreads > 0, "#1");
			Assert.IsTrue (completionPortThreads > 0, "#2");

			workerThreads_new = workerThreads == 1 ? 2 : 1;
			completionPortThreads_new = completionPortThreads == 1 ? 2 : 1;
			ThreadPool.SetMinThreads (workerThreads_new, completionPortThreads_new);

			ThreadPool.GetMinThreads (out workerThreads, out completionPortThreads);
			Assert.IsTrue (workerThreads == workerThreads_new, "#3");
			Assert.IsTrue (completionPortThreads == completionPortThreads_new, "#4");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void SetAndGetMaxThreads ()
		{
			int cpuCount = Environment.ProcessorCount;
			int workerThreads, completionPortThreads;
			int workerThreads_new, completionPortThreads_new;

			ThreadPool.GetMaxThreads (out workerThreads, out completionPortThreads);
			Assert.IsTrue (workerThreads > 0, "#1");
			Assert.IsTrue (completionPortThreads > 0, "#2");

			workerThreads_new = workerThreads == cpuCount ? cpuCount + 1 : cpuCount;
			completionPortThreads_new = completionPortThreads == cpuCount ? cpuCount + 1 : cpuCount;
			ThreadPool.SetMaxThreads (workerThreads_new, completionPortThreads_new);

			ThreadPool.GetMaxThreads (out workerThreads, out completionPortThreads);
			Assert.IsTrue (workerThreads == workerThreads_new, "#3");
			Assert.IsTrue (completionPortThreads == completionPortThreads_new, "#4");
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void SetMaxPossibleThreads ()
		{
			var maxPossibleThreads = 0x7fff;
			int maxWt, macCpt;

			ThreadPool.SetMaxThreads (maxPossibleThreads, maxPossibleThreads);
			ThreadPool.GetMaxThreads (out maxWt, out macCpt);
			Assert.AreEqual (maxPossibleThreads, maxWt);
			Assert.AreEqual (maxPossibleThreads, macCpt);

			ThreadPool.SetMaxThreads (maxPossibleThreads + 1, maxPossibleThreads + 1);
			ThreadPool.GetMaxThreads (out maxWt, out macCpt);
			Assert.AreEqual (maxPossibleThreads, maxWt);
			Assert.AreEqual (maxPossibleThreads, macCpt);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void GetAvailableThreads ()
		{
			int cpuCount = Environment.ProcessorCount;

			if (cpuCount > 16)
				Assert.Inconclusive ("This test doesn't work well with a high number of processor cores.");

			ManualResetEvent mre = new ManualResetEvent (false);
			var sw = Stopwatch.StartNew ();
			int i, workerThreads, completionPortThreads;

			try {
				Assert.IsTrue (ThreadPool.SetMaxThreads (cpuCount, cpuCount));

				while (true) {
					ThreadPool.GetAvailableThreads (out workerThreads, out completionPortThreads);
					if (workerThreads == 0)
						break;

					if (sw.Elapsed.TotalSeconds >= 30) {
						Console.WriteLine ("workerThreads = {0}, completionPortThreads = {1}", workerThreads, completionPortThreads);
						Assert.Fail ("did not reach 0 available threads");
					}

					ThreadPool.QueueUserWorkItem (GetAvailableThreads_Callback, mre);
					Thread.Sleep (1);
				}
			} finally {
				mre.Set ();
			}
		}

		void GetAvailableThreads_Callback (object state)
		{
			ManualResetEvent mre = (ManualResetEvent) state;

			if (mre.WaitOne (0))
				return;

			ThreadPool.QueueUserWorkItem (GetAvailableThreads_Callback, mre);
			ThreadPool.QueueUserWorkItem (GetAvailableThreads_Callback, mre);
			ThreadPool.QueueUserWorkItem (GetAvailableThreads_Callback, mre);
			ThreadPool.QueueUserWorkItem (GetAvailableThreads_Callback, mre);

			mre.WaitOne ();
		}

		[Test]
		[Category ("MultiThreaded")]
		public void AsyncLocalCapture ()
		{
			var asyncLocal = new AsyncLocal<int>();
			asyncLocal.Value = 1;
			int var_0, var_1, var_2, var_3;
			var_0 = var_1 = var_2 = var_3 = 99;
			var cw = new CountdownEvent (4);

			var evt = new AutoResetEvent (false);
			ThreadPool.QueueUserWorkItem(state => {
				var_0 = asyncLocal.Value;
				cw.Signal ();
			}, null);

			ThreadPool.UnsafeQueueUserWorkItem(state => {
				var_1 = asyncLocal.Value;
				cw.Signal ();
			}, null);

			ThreadPool.RegisterWaitForSingleObject (evt, (state, to) => {
				var_2 = asyncLocal.Value;
				cw.Signal ();
			}, null, millisecondsTimeOutInterval: 1, executeOnlyOnce: true);

			ThreadPool.UnsafeRegisterWaitForSingleObject (evt, (state, to) => {
				var_3 = asyncLocal.Value;
				cw.Signal ();
			}, null, millisecondsTimeOutInterval: 1, executeOnlyOnce: true);

			Assert.IsTrue (cw.Wait (2000), "cw_wait");

			Assert.AreEqual (1, var_0, "var_0");
			Assert.AreEqual (0, var_1, "var_1");
			Assert.AreEqual (1, var_2, "var_2");
			Assert.AreEqual (0, var_3, "var_3");
		}

#if !MOBILE
		// This is test related to bug https://bugzilla.xamarin.com/show_bug.cgi?id=41294.
		// The bug is that the performance counters return 0.
		// "Work Items Added" and "# of Threads" are fixed, the others are not.
		[Test]
		public  void PerformanceCounter_WorkItems ()
		{
			var workItems = new PerformanceCounter ("Mono Threadpool", "Work Items Added");
			var threads   = new PerformanceCounter ("Mono Threadpool", "# of Threads");

			var workItems0 = workItems.NextValue();

			int N = 99;
			for (var i = 0; i < N; i++)
				ThreadPool.QueueUserWorkItem (_ => {});

			var workItems1 = workItems.NextValue();
			var threads0 = threads.NextValue();

			//Console.WriteLine ("workItems0:{0} workItems1:{1}", workItems0, workItems1);
			//Console.WriteLine ("threads:{0}",  threads0);

			AssertHelper.GreaterOrEqual ((int)(workItems1 - workItems0), N, "#1");
			Assert.IsTrue (threads0 > 0, "#2");
		}
#endif

		[Test]
		[Category ("MultiThreaded")]
		public void SetMinThreads ()
		{
			int workerThreads, cpThreads;
			int expectedWt = 64, expectedCpt = 64;
			bool set = ThreadPool.SetMinThreads (expectedWt, expectedCpt);
			ThreadPool.GetMinThreads (out workerThreads, out cpThreads);
			Assert.IsTrue (set, "#1");
			Assert.AreEqual (expectedWt, workerThreads, "#2");
			Assert.AreEqual (expectedCpt, cpThreads, "#3");
		}
	}
}
