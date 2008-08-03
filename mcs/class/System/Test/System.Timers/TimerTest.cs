//
// TimerTest.cs - NUnit Test Cases for System.Timers.Timer
//
// Author:
//   Kornél Pál <http://www.kornelpal.hu/>
//   Robert Jordan <robertj@gmx.net>
//
// Copyright (C) 2005 Kornél Pál
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

using NUnit.Framework;
using System;
using System.Timers;
using ST = System.Threading;

namespace MonoTests.System.Timers
{
	[TestFixture]
	public class TimerTest
	{
		Timer timer;
		int _elapsedCount;

		[SetUp]
		public void SetUp ()
		{
			timer = new Timer ();
		}

		[TearDown]
		public void TearDown ()
		{
			timer.Close ();
		}

		[Test]
		public void Constructor0 ()
		{
			Assert.IsTrue (timer.AutoReset, "#1");
			Assert.IsFalse (timer.Enabled, "#2");
			Assert.AreEqual (100, timer.Interval, "#3");
			Assert.IsNull (timer.SynchronizingObject, "#4");
		}

		[Test]
		public void Constructor1 ()
		{
			timer = new Timer (1);
			Assert.IsTrue (timer.AutoReset, "#A1");
			Assert.IsFalse (timer.Enabled, "#A2");
			Assert.AreEqual (1, timer.Interval, "#A3");
			Assert.IsNull (timer.SynchronizingObject, "#A4");

			timer = new Timer (int.MaxValue);
			Assert.IsTrue (timer.AutoReset, "#B1");
			Assert.IsFalse (timer.Enabled, "#B2");
			Assert.AreEqual (int.MaxValue, timer.Interval, "#B3");
			Assert.IsNull (timer.SynchronizingObject, "#B4");
		}

		[Test]
		public void Constructor1_Interval_Negative ()
		{
			try {
				new Timer (-1);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value -1 for parameter interval
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Constructor1_Interval_Zero ()
		{
			try {
				new Timer (0);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Invalid value 0 for parameter interval
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Constructor1_Interval_Max ()
		{
#if NET_2_0
			try {
				new Timer (0x80000000);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Invalid value 2147483648 for parameter interval
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				new Timer (double.MaxValue);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Invalid value 1.79769313486232E+308 for parameter interval
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
#else
			timer = new Timer (0x80000000);
			Assert.AreEqual (0x80000000, timer.Interval, "#1");
			timer = new Timer (double.MaxValue);
			Assert.AreEqual (double.MaxValue, timer.Interval, "#2");
#endif
		}

		[Test]
		public void AutoReset ()
		{
			Assert.IsTrue (timer.AutoReset, "#1");
			timer.AutoReset = false;
			Assert.IsFalse (timer.AutoReset, "#2");
		}

		[Test]
		public void Interval ()
		{
			timer.Interval = 1;
			Assert.AreEqual (1, timer.Interval, "#1");
			timer.Interval = 500;
			Assert.AreEqual (500, timer.Interval, "#2");
			timer.Interval = double.MaxValue;
			Assert.AreEqual (double.MaxValue, timer.Interval, "#3");
		}

		[Test]
		public void Interval_Negative ()
		{
			try {
				timer.Interval = -1;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// '0' is not a valid value for 'Interval'. 'Interval' must be greater than 0
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Interval_Zero ()
		{
			try {
				timer.Interval = 0;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// '0' is not a valid value for 'Interval'. 'Interval' must be greater than 0
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void StartStopEnabled ()
		{
			timer.Start ();
			Assert.IsTrue (timer.Enabled, "#1");
			timer.Stop ();
			Assert.IsFalse (timer.Enabled, "#2");
		}

		[Test]
		public void CloseEnabled ()
		{
			Assert.IsFalse (timer.Enabled, "#1");
			timer.Enabled = true;
			Assert.IsTrue (timer.Enabled, "#2");
			timer.Close ();
			Assert.IsFalse (timer.Enabled, "#3");
		}

		[Test] // bug #325368
		public void EnabledInElapsed ()
		{
			_elapsedCount = 0;
			timer = new Timer (50);
			timer.AutoReset = false;
			timer.Elapsed += new ElapsedEventHandler (EnabledInElapsed_Elapsed);
			timer.Start ();

			ST.Thread.Sleep (200);
			timer.Stop ();

			Assert.IsTrue (_elapsedCount == 2,  "#1 loss of events");
		}

		[Test]
		public void TestRaceCondition ()
		{
			Assert.IsTrue (new RaceTest (true).Success, "#1");
			Assert.IsTrue (new RaceTest (false).Success, "#2");
		}

		void EnabledInElapsed_Elapsed (object sender, ElapsedEventArgs e)
		{
			_elapsedCount++;
			Timer t = sender as Timer;
			if (_elapsedCount == 1)
				t.Enabled = true;
		}
	}

	class RaceTest
	{
		const int Threads = 2;
		const int Loops = 100;

		object locker = new object ();
		Timer timer;
		int counter;

		public bool Success {
			get { return counter > Loops * Threads; }
		}

		public RaceTest (bool autoReset)
		{
			timer = new Timer ();
			timer.AutoReset = autoReset;
			timer.Interval = 100;
			timer.Elapsed += new ElapsedEventHandler (Tick);
			timer.Start ();

			ST.Thread[] tl = new ST.Thread [Threads];

			for (int i = 0; i < Threads; i++) {
				tl [i] = new ST.Thread (new ST.ThreadStart (Run));
				tl [i].Start ();
			}

			for (int i = 0; i < Threads; i++) {
				tl [i].Join ();
			}

			ST.Thread.Sleep (1000);
		}

		void Restart ()
		{
			lock (locker) {
				timer.Stop ();
				timer.Start ();
			}
			ST.Interlocked.Increment (ref counter);
		}

		void Tick (object sender, ElapsedEventArgs e)
		{
			Restart ();
		}

		void Run ()
		{
			for (int i = 0; i < Loops; i++) {
				ST.Thread.Sleep (0);
				Restart ();
			}
		}
	}
}
