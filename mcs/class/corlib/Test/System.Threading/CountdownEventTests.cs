// CountdownEventTests.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using MonoTests.System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	[TestFixtureAttribute]
	public class CountdownEventTests
	{
		[Test]
		public void Constructor_Invalid ()
		{
			try {
				new CountdownEvent (-2);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Constructor_Zero ()
		{
			var ce = new CountdownEvent (0);
			Assert.IsTrue (ce.IsSet, "#1");
			Assert.AreEqual (0, ce.InitialCount, "#2");
			Assert.IsTrue (ce.Wait (0), "#3");
		}

		[Test]
		public void Constructor_Max ()
		{
			new CountdownEvent (int.MaxValue);
		}

		[Test]
		public void AddCount_Invalid ()
		{
			var ev = new CountdownEvent (1);
			try {
				ev.AddCount (0);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				ev.AddCount (-1);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void AddCount_HasBeenSet ()
		{
			var ev = new CountdownEvent (0);
			try {
				ev.AddCount (1);
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			ev = new CountdownEvent (1);
			Assert.IsTrue (ev.Signal (), "#2");
			try {
				ev.AddCount (1);
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void AddCountSignalStressTestCase ()
		{
			var evt = new CountdownEvent (5);

			int count = 0;
			ParallelTestHelper.ParallelStressTest (evt, delegate (CountdownEvent e) {
				int num = Interlocked.Increment (ref count);
				if (num % 2 == 0)
					e.AddCount ();
				else
					e.Signal ();
			}, 7);

			Assert.AreEqual (4, evt.CurrentCount, "#1");
			Assert.IsFalse (evt.IsSet, "#2");
		}
		
		[Test]
		public void InitialTestCase()
		{
			var evt = new CountdownEvent (5);

			Assert.AreEqual(5, evt.InitialCount, "#1");
			evt.AddCount();
			evt.Signal(3);
			Assert.AreEqual(5, evt.InitialCount, "#2");
		}
		
		[Test]
		public void CurrentCountTestCase()
		{
			var evt = new CountdownEvent (5);

			Assert.AreEqual(5, evt.CurrentCount, "#1");
			
			evt.AddCount();
			Assert.AreEqual(6, evt.CurrentCount, "#2");
			
			evt.TryAddCount(2);
			Assert.AreEqual(8, evt.CurrentCount, "#3");
			
			evt.Signal(4);
			Assert.AreEqual(4, evt.CurrentCount, "#4");
			
			evt.Reset();
			Assert.AreEqual(5, evt.CurrentCount, "#5");
		}

		[Test]
		public void Dispose ()
		{
			var ce = new CountdownEvent (1);
			ce.Dispose ();
			Assert.AreEqual (1, ce.CurrentCount, "#0a");
			Assert.AreEqual (1, ce.InitialCount, "#0b");
			Assert.IsFalse (ce.IsSet, "#0c");

			try {
				ce.AddCount ();
				Assert.Fail ("#1");
			} catch (ObjectDisposedException) {
			}

			try {
				ce.Reset ();
				Assert.Fail ("#2");
			} catch (ObjectDisposedException) {
			}

			try {
				ce.Signal ();
				Assert.Fail ("#3");
			} catch (ObjectDisposedException) {
			}

			try {
				ce.TryAddCount ();
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			try {
				ce.Wait (5);
				Assert.Fail ("#4");
			} catch (ObjectDisposedException) {
			}

			try {
				var v = ce.WaitHandle;
				Assert.Fail ("#5");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Dispose_Double ()
		{
			var ce = new CountdownEvent (1);
			ce.Dispose ();
			ce.Dispose ();
		}
		
		[Test]
		public void IsSetTestCase()
		{
			var evt = new CountdownEvent (5);

			Assert.IsFalse(evt.IsSet, "#1");
			
			evt.Signal(5);
			Assert.IsTrue(evt.IsSet, "#2");
			
			evt.Reset();
			Assert.IsFalse(evt.IsSet, "#3");
		}

		[Test]
		public void Reset_Invalid ()
		{
			var ev = new CountdownEvent (1);
			try {
				ev.Reset (-1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
				Assert.AreEqual (1, ev.CurrentCount, "#1a");
			}
		}

		[Test]
		public void Reset_FullInitialized ()
		{
			var ev = new CountdownEvent (0);
			Assert.IsTrue (ev.IsSet, "#1");
			Assert.AreEqual (0, ev.CurrentCount, "#2");

			ev.Reset (4);
			Assert.IsFalse (ev.IsSet, "#3");
			Assert.AreEqual (4, ev.CurrentCount, "#4");
			Assert.IsFalse (ev.Wait (0), "#5");
		}

		[Test]
		public void Reset_Zero ()
		{
			var ev = new CountdownEvent (1);
			Assert.IsFalse (ev.IsSet, "#1");

			ev.Reset (0);
			Assert.IsTrue (ev.IsSet, "#2");
			Assert.IsTrue (ev.Wait (0), "#3");
			Assert.AreEqual (0, ev.CurrentCount, "#4");
		}

		[Test]
		public void Signal_Invalid ()
		{
			var ev = new CountdownEvent (1);
			try {
				ev.Signal (0);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
				Assert.AreEqual (1, ev.CurrentCount, "#1a");
			}

			try {
				ev.Signal (-1);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
				Assert.AreEqual (1, ev.CurrentCount, "#2a");
			}
		}

		[Test]
		public void Signal_Negative ()
		{
			var ev = new CountdownEvent (1);
			try {
				ev.Signal (2);
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
				Assert.AreEqual (1, ev.CurrentCount, "#1a");
			}

			ev.Signal ();
			try {
				ev.Signal ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
				Assert.AreEqual (0, ev.CurrentCount, "#2a");
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void Signal_Concurrent ()
		{
			for (int r = 0; r < 100; ++r) {
				using (var ce = new CountdownEvent (500)) {
					for (int i = 0; i < ce.InitialCount; ++i) {
						ThreadPool.QueueUserWorkItem (delegate {
							ce.Signal ();
						});
					}

					Assert.IsTrue (ce.Wait (10000), "#1");
				}
			}
		}
		
		[Test]
		public void TryAddCountTestCase()
		{
			var evt = new CountdownEvent (5);

			Assert.IsTrue(evt.TryAddCount(2), "#1");
			evt.Signal(7);
			Assert.IsFalse(evt.TryAddCount(), "#2");
		}

		[Test]
		public void TryAddCount_Invalid ()
		{
			var ev = new CountdownEvent (1);
			try {
				ev.TryAddCount (0);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				ev.TryAddCount (-1);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TryAddCount_HasBeenSet ()
		{
			var ev = new CountdownEvent (0);
			Assert.IsFalse (ev.TryAddCount (1), "#1");

			ev = new CountdownEvent (1);
			ev.Signal ();
			Assert.IsFalse (ev.TryAddCount (1), "#2");

			ev = new CountdownEvent (2);
			ev.Signal (2);
			Assert.IsFalse (ev.TryAddCount (66), "#3");
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void WaitTestCase()
		{
			var evt = new CountdownEvent (5);

			int count = 0;
			bool s = false;
			
			ParallelTestHelper.ParallelStressTest(evt, delegate (CountdownEvent e) {
				if (Interlocked.Increment(ref count) % 2 == 0) {
					Thread.Sleep(100);
					while(!e.IsSet)
						e.Signal();
				} else {
					e.Wait();
					s = true;
				}
			}, 3);
			
			Assert.IsTrue(s, "#1");
			Assert.IsTrue(evt.IsSet, "#2");
		}

		[Test]
		public void ResetTest ()
		{
			var evt = new CountdownEvent (5);

			Assert.AreEqual (5, evt.CurrentCount);
			evt.Signal ();
			Assert.AreEqual (4, evt.CurrentCount);
			evt.Reset ();
			Assert.AreEqual (5, evt.CurrentCount);
			Assert.AreEqual (5, evt.InitialCount);
			evt.Signal ();
			evt.Signal ();
			Assert.AreEqual (3, evt.CurrentCount);
			Assert.AreEqual (5, evt.InitialCount);
			evt.Reset (10);
			Assert.AreEqual (10, evt.CurrentCount);
			Assert.AreEqual (10, evt.InitialCount);
		}
	}
}
