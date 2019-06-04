//
// WaitHandleTest.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class WaitHandleTest {

		TimeSpan Infinite = new TimeSpan (-10000);	// -10000 ticks == -1 ms
		TimeSpan SmallNegative = new TimeSpan (-2);	// between 0 and -1.0 (infinite) ms
		TimeSpan Negative = new TimeSpan (-20000);	// really negative

		WaitHandle [] TooLarge = new Mutex [65];
		WaitHandle [] Empty = new Mutex [1];
		WaitHandle [] Single = new Mutex [1] { new Mutex (true) };


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAny_WaitHandle_Null ()
		{
			WaitHandle.WaitAny (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WaitAny_WaitHandle_TooLarge ()
		{
			WaitHandle.WaitAny (TooLarge);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAny_WaitHandle_Empty ()
		{
			WaitHandle.WaitAny (Empty);
		}

		[Test]
		public void WaitAny_WaitHandle ()
		{
			Assert.AreEqual (0, WaitHandle.WaitAny (Single), "WaitAny");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAny_WaitHandleNull_Int ()
		{
			WaitHandle.WaitAny (null, -1);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WaitAny_WaitHandle_TooLarge_Int ()
		{
			WaitHandle.WaitAny (TooLarge, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAny_WaitHandle_Empty_Int ()
		{
			WaitHandle.WaitAny (Empty, -1);
		}

		[Test]
		public void WaitAny_WaitHandle_Int ()
		{
			// -1 is infinite
			Assert.AreEqual (0, WaitHandle.WaitAny (Single, -1), "WaitAny");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitAny_WaitHandle_Int_Negative ()
		{
			Assert.AreEqual (0, WaitHandle.WaitAny (Single, -2), "WaitAny");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAny_WaitHandleNull_TimeSpan ()
		{
			WaitHandle.WaitAny (null, Infinite);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WaitAny_WaitHandle_TooLarge_TimeSpan ()
		{
			WaitHandle.WaitAny (TooLarge, Infinite);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAny_WaitHandle_Empty_TimeSpan ()
		{
			WaitHandle.WaitAny (Empty, Infinite);
		}

		[Test]
		public void WaitAny_WaitHandle_TimeSpan ()
		{
			Assert.AreEqual (Timeout.Infinite, (int) Infinite.TotalMilliseconds, "Infinite");
			Assert.AreEqual (0, WaitHandle.WaitAny (Single, Infinite), "WaitAny-Infinite");
			Assert.AreEqual (0, WaitHandle.WaitAny (Single, SmallNegative), "WaitAny-SmallNegative");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitAny_WaitHandle_TimeSpan_Negative ()
		{
			Assert.AreEqual (0, WaitHandle.WaitAny (Single, Negative), "WaitAny");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitAny_WaitHandle_TimeSpan_MaxValue ()
		{
			Assert.AreEqual (0, WaitHandle.WaitAny (Single, TimeSpan.MaxValue), "WaitAny");
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_WaitHandle_Null ()
		{
			WaitHandle.WaitAll (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WaitAll_WaitHandle_TooLarge ()
		{
			WaitHandle.WaitAll (TooLarge);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_WaitHandle_Empty ()
		{
			WaitHandle.WaitAll (Empty);
		}

		[Test]
		public void WaitAll_WaitHandle ()
		{
			Assert.IsTrue (WaitHandle.WaitAll (Single), "WaitAll");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_WaitHandleNull_Int ()
		{
			WaitHandle.WaitAll (null, -1);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WaitAll_WaitHandle_TooLarge_Int ()
		{
			WaitHandle.WaitAll (TooLarge, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_WaitHandle_Empty_Int ()
		{
			WaitHandle.WaitAll (Empty, -1);
		}

		[Test]
		public void WaitAll_WaitHandle_Int ()
		{
			// -1 is infinite
			Assert.IsTrue (WaitHandle.WaitAll (Single, -1), "WaitAll");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitAll_WaitHandle_Int_Negative ()
		{
			Assert.IsTrue (WaitHandle.WaitAll (Single, -2), "WaitAll");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_WaitHandleNull_TimeSpan ()
		{
			WaitHandle.WaitAll (null, Infinite);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WaitAll_WaitHandle_TooLarge_TimeSpan ()
		{
			WaitHandle.WaitAll (TooLarge, Infinite);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_WaitHandle_Empty_TimeSpan ()
		{
			WaitHandle.WaitAll (Empty, Infinite);
		}

		[Test]
		public void WaitAll_WaitHandle_TimeSpan ()
		{
			Assert.AreEqual (Timeout.Infinite, (int) Infinite.TotalMilliseconds, "Infinite");
			Assert.IsTrue (WaitHandle.WaitAll (Single, Infinite), "WaitAll-Infinite");
			Assert.IsTrue (WaitHandle.WaitAll (Single, SmallNegative), "WaitAll-SmallNegative");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitAll_WaitHandle_TimeSpan_Negative ()
		{
			Assert.IsTrue (WaitHandle.WaitAll (Single, Negative), "WaitAll");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitAll_WaitHandle_TimeSpan_MaxValue ()
		{
			Assert.IsTrue (WaitHandle.WaitAll (Single, TimeSpan.MaxValue), "WaitAll");
		}


		[Test]
		public void WaitOne ()
		{
			Assert.IsTrue (Single [0].WaitOne (), "WaitOne");
		}

		[Test]
		public void WaitOne_Int ()
		{
			// -1 is infinite
			Assert.IsTrue (Single [0].WaitOne (-1), "WaitOne");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitOne_Int_Negative ()
		{
			Assert.IsTrue (Single [0].WaitOne (-2), "WaitOne");
		}

		[Test]
		public void WaitOne_TimeSpan ()
		{
			Assert.AreEqual (Timeout.Infinite, (int) Infinite.TotalMilliseconds, "Infinite");
			Assert.IsTrue (Single [0].WaitOne (Infinite), "WaitOne-Infinite");
			Assert.IsTrue (Single [0].WaitOne (SmallNegative), "WaitOne-SmallNegative");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitOne_TimeSpan_Negative ()
		{
			Assert.IsTrue (Single [0].WaitOne (Negative), "WaitOne");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void WaitOne_TimeSpan_MaxValue ()
		{
			Assert.IsTrue (Single [0].WaitOne (TimeSpan.MaxValue), "WaitOne");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WaitAll_Empty ()
		{
			WaitHandle.WaitAll (new WaitHandle [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WaitAny_Empty ()
		{
			WaitHandle.WaitAny (new WaitHandle [0]);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void InterrupedWaitAny ()
		{
			using (var m1 = new Mutex (true)) {
				using (var m2 = new Mutex (true)) {
					using (var done = new ManualResetEvent (false)) {
						var thread = new Thread (() =>
						{
							try {
								WaitHandle.WaitAny (new WaitHandle [] { m1, m2 });
							} catch (ThreadInterruptedException) {
								done.Set ();
							}
						});
						thread.Start ();
						Thread.Sleep (100); // wait a bit so the thread can enter its wait
						thread.Interrupt ();

						Assert.IsTrue (thread.Join (1000), "Join");
						Assert.IsTrue (done.WaitOne (1000), "done");

						m1.ReleaseMutex ();
						m2.ReleaseMutex ();
					}
				}
			}
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void InterrupedWaitAll ()
		{
			using (var m1 = new Mutex (true)) {
				using (var m2 = new Mutex (true)) {
					using (var done = new ManualResetEvent (false)) {
						var thread = new Thread (() =>
						                         {
							try {
								WaitHandle.WaitAll (new WaitHandle [] { m1, m2 });
							} catch (ThreadInterruptedException) {
								done.Set ();
							}
						});
						thread.Start ();
						Thread.Sleep (100); // wait a bit so the thread can enter its wait
						thread.Interrupt ();

						Assert.IsTrue (thread.Join (1000), "Join");
						Assert.IsTrue (done.WaitOne (1000), "done");

						m1.ReleaseMutex ();
						m2.ReleaseMutex ();
					}
				}
			}
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void InterrupedWaitOne ()
		{
			using (var m1 = new Mutex (true)) {
				using (var done = new ManualResetEvent (false)) {
					var thread = new Thread (() =>
					                         {
						try {
							m1.WaitOne ();
						} catch (ThreadInterruptedException) {
							done.Set ();
						}
					});
					thread.Start ();
					Thread.Sleep (100); // wait a bit so the thread can enter its wait
					thread.Interrupt ();

					Assert.IsTrue (thread.Join (1000), "Join");
					Assert.IsTrue (done.WaitOne (1000), "done");

					m1.ReleaseMutex ();
				}
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void WaitOneWithAbandonedMutex ()
		{
			using (var m = new Mutex (false)) {
				var thread1 = new Thread (() => {
					m.WaitOne ();
				});
				thread1.Start ();
				Assert.IsTrue (thread1.Join (Timeout.Infinite), "thread1.Join");
				try {
					m.WaitOne ();
					Assert.Fail ("Expected AbandonedMutexException");
				} catch (AbandonedMutexException) {
				}
				// Current thread should own the Mutex now
				var signalled = false;
				var thread2 = new Thread (() => {
					signalled = m.WaitOne (100);
				});
				thread2.Start ();
				Assert.IsTrue (thread2.Join (Timeout.Infinite), "thread2.Join");
				Assert.IsFalse (signalled);

				// Since this thread owns the Mutex releasing it shouldn't fail
				m.ReleaseMutex ();
				// The Mutex should now be unowned
				try {
					m.ReleaseMutex ();
					Assert.Fail ("Expected ApplicationException");
				} catch (ApplicationException) {
				}
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void WaitOneWithAbandonedMutexAndMultipleThreads ()
		{
			using (var m = new Mutex (true)) {
				var nonAbandoned = 0;
				var abandoned = 0;
				var n = 0;
				var threads = new List<Thread> ();
				for (int i = 0; i < 50; i++) {
					var thread = new Thread (() => {
						try {
							m.WaitOne ();
							nonAbandoned++;
						} catch (AbandonedMutexException) {
							abandoned++;
						}
						if (((n++) % 5) != 0)
							m.ReleaseMutex ();
					});
					thread.Start ();
					threads.Add (thread);
				}
				m.ReleaseMutex ();
				foreach (var thread in threads) {
					if (!thread.Join (1000)) {
						Assert.Fail ("Timed out");
					}
				}
				Assert.AreEqual (40, nonAbandoned);
				Assert.AreEqual (10, abandoned);
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void WaitAnyWithSecondMutexAbandoned ()
		{
			using (var m1 = new Mutex (false)) {
				using (var m2 = new Mutex (false)) {
					var mainProceed = false;
					var thread2Proceed = false;
					var thread1 = new Thread (() => {
						m2.WaitOne ();
					});
					var thread2 = new Thread (() => {
						m1.WaitOne ();
						mainProceed = true;
						while (!thread2Proceed) {
							Thread.Sleep (10);
						}
						m1.ReleaseMutex ();
					});
					thread1.Start ();
					Assert.IsTrue (thread1.Join (Timeout.Infinite), "thread1.Join");
					thread2.Start ();
					while (!mainProceed) {
						Thread.Sleep (10);
					}
					try {
						WaitHandle.WaitAny (new WaitHandle [] { m1, m2 });
						Assert.Fail ("Expected AbandonedMutexException");
					} catch (AbandonedMutexException e) {
						Assert.AreEqual (1, e.MutexIndex);
						Assert.AreEqual (m2, e.Mutex);
					} finally {
						thread2Proceed = true;
						Assert.IsTrue (thread2.Join (Timeout.Infinite), "thread2.Join");
					}

					// Current thread should own the second Mutex now
					var signalled = -1;
					var thread3 = new Thread (() => {
						signalled = WaitHandle.WaitAny (new WaitHandle [] { m1, m2 }, 0);
					});
					thread3.Start ();
					Assert.IsTrue (thread3.Join (Timeout.Infinite), "thread3.Join");
					Assert.AreEqual (0, signalled);

					// Since this thread owns the second Mutex releasing it shouldn't fail
					m2.ReleaseMutex ();
					// Second Mutex should now be unowned
					try {
						m2.ReleaseMutex ();
						Assert.Fail ("Expected ApplicationException");
					} catch (ApplicationException) {
					}
					// .NET allows the first Mutex which is now abandoned to be released multiple times by this thread
					m1.ReleaseMutex ();
					m1.ReleaseMutex ();
				}
			}
		}

		[Test]
		[ExpectedException (typeof (AbandonedMutexException))]
		[Category ("MultiThreaded")]
		public void WaitAllWithOneAbandonedMutex ()
		{
			using (var m1 = new Mutex (false)) {
				using (var m2 = new Mutex (false)) {
					var thread = new Thread (() => {
						m1.WaitOne ();
					});
					thread.Start ();
					Assert.IsTrue (thread.Join (Timeout.Infinite), "thread.Join");
					WaitHandle.WaitAll (new WaitHandle [] { m1, m2 });
				}
			}
		}

#if MONO_FEATURE_THREAD_SUSPEND_RESUME
		[Test]
		[Category ("MultiThreaded")]
		public void WaitOneWithTimeoutAndSpuriousWake ()
		{
			/* This is to test that WaitEvent.WaitOne is not going to wait largely
			 * more than its timeout. In this test, it shouldn't wait more than
			 * 1500 milliseconds, with its timeout being 1000ms */

			using (ManualResetEvent mre = new ManualResetEvent (false))
			using (ManualResetEvent ready = new ManualResetEvent (false)) {
				var thread = new Thread (() => {
					ready.Set ();
					mre.WaitOne (1000);
				});

				thread.Start ();
				ready.WaitOne ();

				Thread.Sleep (10); // wait a bit so we enter mre.WaitOne

				var sw = Stopwatch.StartNew ();
				while (sw.ElapsedMilliseconds <= 500) {
					thread.Suspend ();
					thread.Resume ();
				}

				Assert.IsTrue (thread.Join (1000), "#1");
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void WaitAnyWithTimeoutAndSpuriousWake ()
		{
			/* This is to test that WaitEvent.WaitAny is not going to wait largely
			 * more than its timeout. In this test, it shouldn't wait more than
			 * 1500 milliseconds, with its timeout being 1000ms */

			using (ManualResetEvent mre1 = new ManualResetEvent (false))
			using (ManualResetEvent mre2 = new ManualResetEvent (false))
			using (ManualResetEvent ready = new ManualResetEvent (false)) {
				var thread = new Thread (() => {
					ready.Set ();
					WaitHandle.WaitAny (new [] { mre1, mre2 }, 1000);
				});

				thread.Start ();
				ready.WaitOne ();

				Thread.Sleep (10); // wait a bit so we enter WaitHandle.WaitAny ({mre1, mre2})

				var sw = Stopwatch.StartNew ();
				while (sw.ElapsedMilliseconds <= 500) {
					thread.Suspend ();
					thread.Resume ();
				}

				Assert.IsTrue (thread.Join (1000), "#1");
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void WaitAllWithTimeoutAndSpuriousWake ()
		{
			/* This is to test that WaitEvent.WaitAll is not going to wait largely
			 * more than its timeout. In this test, it shouldn't wait more than
			 * 1500 milliseconds, with its timeout being 1000ms */

			using (ManualResetEvent mre1 = new ManualResetEvent (false))
			using (ManualResetEvent mre2 = new ManualResetEvent (false))
			using (ManualResetEvent ready = new ManualResetEvent (false)) {
				var thread = new Thread (() => {
					ready.Set ();
					WaitHandle.WaitAll (new [] { mre1, mre2 }, 1000);
				});

				thread.Start ();
				ready.WaitOne ();

				Thread.Sleep (10); // wait a bit so we enter WaitHandle.WaitAll ({mre1, mre2})

				var sw = Stopwatch.StartNew ();
				while (sw.ElapsedMilliseconds <= 500) {
					thread.Suspend ();
					thread.Resume ();
				}

				Assert.IsTrue (thread.Join (1000), "#1");
			}
		}
#endif // MONO_FEATURE_THREAD_SUSPEND_RESUME

		[Test]
		public static void SignalAndWait()
		{
			using (var eventToSignal = new AutoResetEvent (false))
			using (var eventToWait = new AutoResetEvent (false))
			{
				eventToWait.Set ();

				Assert.IsTrue (WaitHandle.SignalAndWait (eventToSignal, eventToWait), "#1");
				Assert.IsTrue (eventToSignal.WaitOne (), "#2");
			}
		}

		// https://github.com/mono/mono/issues/9089
		// Duplication is ok for WaitAny, exception for WaitAll.
		// System.DuplicateWaitObjectException: Duplicate objects in argument.
		[Test]
		public static void DuplicateWaitAny ()
		{
			using (var a = new ManualResetEvent (true))
			{
				var b = new ManualResetEvent [ ] { a, a };
				Assert.AreEqual (0, WaitHandle.WaitAny (b), "#1");
			}
		}

		// https://github.com/mono/mono/issues/9089
		// Duplication is ok for WaitAny, exception for WaitAll.
		// System.DuplicateWaitObjectException: Duplicate objects in argument.
		[Test]
		public static void DuplicateWaitAll ()
		{
			using (var a = new ManualResetEvent (true))
			{
				var b = new ManualResetEvent [ ] { a, a };
				try {
					WaitHandle.WaitAll (b);
					Assert.Fail ("Expected System.DuplicateWaitObjectException");
				} catch (DuplicateWaitObjectException) {
				}
			}
		}
	}
}
