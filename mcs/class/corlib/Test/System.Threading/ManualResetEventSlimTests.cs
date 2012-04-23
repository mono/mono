// ManualResetEventSlimTests.cs
//
// Authors:
//       Marek Safar (marek.safar@gmail.com)
//       Jeremie Laval (jeremie.laval@gmail.com)
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright (c) 2012 Xamarin, Inc (http://www.xamarin.com)
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

using NUnit.Framework;

using MonoTests.System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	
	[TestFixture]
	public class ManualResetEventSlimTests
	{
		ManualResetEventSlim mre;
		
		[SetUp]
		public void Setup()
		{
			mre = new ManualResetEventSlim();
		}

		[Test]
		public void Constructor_Defaults ()
		{
			Assert.IsFalse (mre.IsSet, "#1");
			Assert.AreEqual (10, mre.SpinCount, "#2");
		}

		[Test]
		public void Constructor_Invalid ()
		{
			try {
				new ManualResetEventSlim (true, -1);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new ManualResetEventSlim (true, 2048);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}
		
		[Test]
		public void IsSetTestCase()
		{
			Assert.IsFalse(mre.IsSet, "#1");
			mre.Set();
			Assert.IsTrue(mre.IsSet, "#2");
			mre.Reset();
			Assert.IsFalse(mre.IsSet, "#3");
		}
		
		[Test]
		public void WaitTest()
		{
			int count = 0;
			bool s = false;
			
			ParallelTestHelper.ParallelStressTest(mre, delegate (ManualResetEventSlim m) {
				if (Interlocked.Increment(ref count) % 2 == 0) {
					Thread.Sleep(50);
					for (int i = 0; i < 10; i++) {
						if (i % 2 == 0)
							m.Reset();
						else
							m.Set();
					}
				} else {
					m.Wait();
					s = true;
				}
			}, 2);	
			
			Assert.IsTrue(s, "#1");
			Assert.IsTrue(mre.IsSet, "#2");
		}

		[Test]
		public void Wait_SetConcurrent ()
		{
			for (int i = 0; i < 10000; ++i) {
				var mre = new ManualResetEventSlim ();
				bool b = true;

				ThreadPool.QueueUserWorkItem (delegate {
					mre.Set ();
				});

				ThreadPool.QueueUserWorkItem (delegate {
					b &= mre.Wait (1000);
				});

				Assert.IsTrue (mre.Wait (1000), i.ToString ());
				Assert.IsTrue (b, i.ToString ());
			}
		}

		[Test]
		public void Wait_DisposeWithCancel ()
		{
			var token = new CancellationTokenSource ();
			ThreadPool.QueueUserWorkItem (delegate {
				Thread.Sleep (10);
				mre.Dispose ();
				token.Cancel ();
			});

			try {
				mre.Wait (10000, token.Token);
				Assert.Fail ("#0");
			} catch (OperationCanceledException e) {
			}
		}

		[Test]
		public void Wait_Expired ()
		{
			Assert.IsFalse (mre.Wait (10));
		}

		[Test, ExpectedException (typeof (ObjectDisposedException))]
		public void WaitAfterDisposeTest ()
		{
			mre.Dispose ();
			mre.Wait ();
		}

		[Test]
		public void SetAfterDisposeTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				Exception disp = null, setting = null;

				CountdownEvent evt = new CountdownEvent (2);
				CountdownEvent evtFinish = new CountdownEvent (2);

				ThreadPool.QueueUserWorkItem (delegate {
					try {
						evt.Signal ();
						evt.Wait (1000);
						mre.Dispose ();
					} catch (Exception e) {
						disp = e;
					}
					evtFinish.Signal ();
				});
				ThreadPool.QueueUserWorkItem (delegate {
					try {
						evt.Signal ();
						evt.Wait (1000);
						mre.Set ();
					} catch (Exception e) {
						setting = e;
					}
					evtFinish.Signal ();
				});

				bool bb = evtFinish.Wait (1000);
				if (!bb)
					Assert.AreEqual (true, evtFinish.IsSet);

				Assert.IsTrue (bb, "#0");
				Assert.IsNull (disp, "#1");
				Assert.IsNull (setting, "#2");

				evt.Dispose ();
				evtFinish.Dispose ();
			});
		}

		[Test]
		public void WaitHandle_Initialized ()
		{
			var mre = new ManualResetEventSlim (true);
			Assert.IsTrue (mre.WaitHandle.WaitOne (0), "#1");
			mre.Reset ();
			Assert.IsFalse (mre.WaitHandle.WaitOne (0), "#2");
			Assert.AreEqual (mre.WaitHandle, mre.WaitHandle, "#3");
		}

		[Test]
		public void WaitHandle_NotInitialized ()
		{
			var mre = new ManualResetEventSlim (false);
			Assert.IsFalse (mre.WaitHandle.WaitOne (0), "#1");
			mre.Set ();
			Assert.IsTrue (mre.WaitHandle.WaitOne (0), "#2");
		}

		[Test]
		public void WaitHandleConsistencyTest ()
		{
			var mre = new ManualResetEventSlim ();
			mre.WaitHandle.WaitOne (0);

			for (int i = 0; i < 10000; i++) {
				int count = 2;
				SpinWait wait = new SpinWait ();

				ThreadPool.QueueUserWorkItem (_ => { mre.Set (); Interlocked.Decrement (ref count); });
				ThreadPool.QueueUserWorkItem (_ => { mre.Reset (); Interlocked.Decrement (ref count); });

				while (count > 0)
					wait.SpinOnce ();
				Assert.AreEqual (mre.IsSet, mre.WaitHandle.WaitOne (0));
			}
		}

		[Test]
		public void WaitWithCancellationTokenAndNotImmediateSetTest ()
		{
			var mres = new ManualResetEventSlim ();
			var cts = new CancellationTokenSource ();
			ThreadPool.QueueUserWorkItem(x => { Thread.Sleep (1000); mres.Set (); });
			Assert.IsTrue (mres.Wait (TimeSpan.FromSeconds (10), cts.Token), "Wait returned false despite event was set.");
		}

		[Test]
		public void WaitWithCancellationTokenAndCancel ()
		{
			var mres = new ManualResetEventSlim ();
			var cts = new CancellationTokenSource ();
			ThreadPool.QueueUserWorkItem(x => { Thread.Sleep (1000); cts.Cancel (); });
			try {
				mres.Wait (TimeSpan.FromSeconds (10), cts.Token);
				Assert.Fail ("Wait did not throw an exception despite cancellation token was cancelled.");
			} catch (OperationCanceledException) {
			}
		}

		[Test]
		public void WaitWithCancellationTokenAndTimeout ()
		{
			var mres = new ManualResetEventSlim ();
			var cts = new CancellationTokenSource ();
			Assert.IsFalse (mres.Wait (TimeSpan.FromSeconds (1), cts.Token), "Wait returned true despite timeout.");
		}

		[Test]
		public void Dispose ()
		{
			var mre = new ManualResetEventSlim (false);
			mre.Dispose ();
			Assert.IsFalse (mre.IsSet, "#0a");

			try {
			    mre.Reset ();
			    Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			mre.Set ();

			try {
				mre.Wait (0);
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				var v = mre.WaitHandle;
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Dispose_Double ()
		{
			var mre = new ManualResetEventSlim ();
			mre.Dispose ();
			mre.Dispose ();
		}
	}
}
#endif
