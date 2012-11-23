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

#if NET_2_0

using System;
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

	}
}

#endif

