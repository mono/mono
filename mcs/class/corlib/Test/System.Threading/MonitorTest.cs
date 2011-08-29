//
// MonitorTest.cs - NUnit test cases for System.Threading.Monitor
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2005, 2009 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class MonitorTest {

		TimeSpan Infinite = new TimeSpan (-10000);	// -10000 ticks == -1 ms
		TimeSpan SmallNegative = new TimeSpan (-2);	// between 0 and -1.0 (infinite) ms
		TimeSpan Negative = new TimeSpan (-20000);	// really negative
		TimeSpan MaxValue = TimeSpan.FromMilliseconds ((long) Int32.MaxValue);
		TimeSpan TooLarge = TimeSpan.FromMilliseconds ((long) Int32.MaxValue + 1);

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		[Category ("NotWorking")] // test fails under MS FX 2.0 - maybe that worked on 1.x ?
		public void ExitNoEnter ()
		{
			object o = new object ();
			Monitor.Exit (o);
		}

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		[Category ("NotWorking")] // test fails under MS FX 2.0 - maybe that worked on 1.x ?
		public void OneEnterSeveralExits ()
		{
			object o = new object ();
			Monitor.Enter (o);
			Monitor.Exit (o);
			// fails here
			Monitor.Exit (o);
			Monitor.Exit (o);
			Monitor.Exit (o);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Enter_Null ()
		{
			Monitor.Enter (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Exit_Null ()
		{
			Monitor.Exit (null);
		}

		[Test]
		public void Enter_Exit ()
		{
			object o = new object ();
			Monitor.Enter (o);
			try {
				Assert.IsNotNull (o);
			}
			finally {
				Monitor.Exit (o);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Pulse_Null ()
		{
			Monitor.Pulse (null);
		}

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		public void Pulse_Unlocked ()
		{
			object o = new object ();
			Monitor.Pulse (o);
		}

		[Test]
		public void Pulse ()
		{
			object o = new object ();
			lock (o) {
				Monitor.Pulse (o);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PulseAll_Null ()
		{
			Monitor.PulseAll (null);
		}

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		public void PulseAll_Unlocked ()
		{
			object o = new object ();
			Monitor.PulseAll (o);
		}

		[Test]
		public void PulseAll ()
		{
			object o = new object ();
			lock (o) {
				Monitor.PulseAll (o);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryEnter_Null ()
		{
			Monitor.TryEnter (null);
		}

		[Test]
		public void TryEnter ()
		{
			object o = new object ();
			Assert.IsTrue (Monitor.TryEnter (o), "TryEnter");
			Assert.IsTrue (Monitor.TryEnter (o), "TryEnter-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryEnter_Null_Int ()
		{
			Monitor.TryEnter (null, Timeout.Infinite);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TryEnter_Int_Negative ()
		{
			object o = new object ();
			Monitor.TryEnter (o, -2);
		}

		[Test]
		public void TryEnter_Int ()
		{
			object o = new object ();
			Assert.IsTrue (Monitor.TryEnter (o, 1), "TryEnter");
			Assert.IsTrue (Monitor.TryEnter (o, 2), "TryEnter-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryEnter_Null_TimeSpan ()
		{
			Monitor.TryEnter (null, Timeout.Infinite);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryEnter_TimeSpan_Negative ()
		{
			object o = new object ();
			Monitor.TryEnter (o, Negative);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryEnter_TimeSpan_TooLarge ()
		{
			object o = new object ();
			Monitor.TryEnter (o, TooLarge);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TryEnter_Null_TimeSpan_TooLarge ()
		{
			// exception ordering test
			Monitor.TryEnter (null, TooLarge);
		}

		[Test]
		public void TryEnter_TimeSpan ()
		{
			object o = new object ();
			Assert.IsTrue (Monitor.TryEnter (o, Infinite), "TryEnter");
			Assert.IsTrue (Monitor.TryEnter (o, SmallNegative), "TryEnter-2");
			Assert.IsTrue (Monitor.TryEnter (o, MaxValue), "TryEnter-3");
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Wait_Null ()
		{
			Monitor.Wait (null);
		}

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		public void Wait_Unlocked ()
		{
			object o = new object ();
			Assert.IsTrue (Monitor.Wait (o), "Wait");
		}

		// [Test] that would be Infinite
		public void Wait ()
		{
			object o = new object ();
			lock (o) {
				Assert.IsFalse (Monitor.Wait (o), "Wait");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Wait_Null_Int ()
		{
			Monitor.Wait (null, Timeout.Infinite);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Wait_Int_Negative ()
		{
			object o = new object ();
			Monitor.Wait (o, -2);
		}

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		public void Wait_Int_Unlocked ()
		{
			object o = new object ();
			Assert.IsTrue (Monitor.Wait (o, 1), "Wait");
		}

		[Test]
		public void Wait_Int ()
		{
			object o = new object ();
			lock (o) {
				Assert.IsFalse (Monitor.Wait (o, 1), "Wait");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Wait_Null_TimeSpan ()
		{
			Monitor.Wait (null, Timeout.Infinite);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Wait_TimeSpan_Negative ()
		{
			object o = new object ();
			Monitor.Wait (o, Negative);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Wait_TimeSpan_TooLarge ()
		{
			object o = new object ();
			Monitor.Wait (o, TooLarge);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Wait_Null_TimeSpan_TooLarge ()
		{
			// exception ordering test
			Monitor.Wait (null, TooLarge);
		}

		[Test]
		[ExpectedException (typeof (SynchronizationLockException))]
		public void Wait_TimeSpan_Unlocked ()
		{
			object o = new object ();
			Assert.IsTrue (Monitor.Wait (o, Infinite), "Wait");
		}

		[Test]
		public void Wait_TimeSpan ()
		{
			object o = new object ();
			lock (o) {
				Assert.IsFalse (Monitor.Wait (o, SmallNegative), "Wait");
			}
		}
#if NET_4_0
		[Test]
		public void Enter_bool ()
		{
			object o = new object ();
			bool taken = false;
			Monitor.Enter (o, ref taken);
			Assert.IsTrue (taken, "Monitor.Enter (obj, ref taken)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Enter_bool_argcheck ()
		{
			object o = new object ();
			bool taken = true;
			Monitor.Enter (o, ref taken);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Enter_bool_argcheck_fastpath ()
		{
			object o = new object ();
			bool taken = false;
			Monitor.Enter (o, ref taken);
			taken = true;
			Monitor.Enter (o, ref taken);
		}

#endif

	}
}

