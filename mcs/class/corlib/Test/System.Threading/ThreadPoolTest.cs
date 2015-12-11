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
			var start = DateTime.Now;
			while ((total != n || sum != 0) && (DateTime.Now - start).TotalSeconds < 60)
				Thread.Sleep (1000);
			Assert.IsTrue (total == n, "#1");
			Assert.IsTrue (sum   == 0, "#2");
		}

		event WaitCallback e;

		[Test]
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
		public void GetAvailableThreads ()
		{
			ManualResetEvent mre = new ManualResetEvent (false);
			DateTime start = DateTime.Now;
			int i, workerThreads, completionPortThreads;

			try {
				Assert.IsTrue (ThreadPool.SetMaxThreads (Environment.ProcessorCount, Environment.ProcessorCount));

				while (true) {
					ThreadPool.GetAvailableThreads (out workerThreads, out completionPortThreads);
					if (workerThreads == 0)
						break;

					Console.WriteLine ("workerThreads = {0}, completionPortThreads = {1}", workerThreads, completionPortThreads);

					if ((DateTime.Now - start).TotalSeconds >= 10)
						Assert.Fail ("did not reach 0 available threads");

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
	}
}