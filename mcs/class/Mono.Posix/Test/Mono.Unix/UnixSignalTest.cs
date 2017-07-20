//
// UnixSignalTest.cs - NUnit Test Cases for Mono.Unix.UnixSignal
//
// Authors:
//	Jonathan Pryor  <jonpryor@vt.edu>
//
// (C) 2008 Jonathan Pryor
//

using NUnit.Framework;

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Android;
using Mono.Unix.Native;

namespace MonoTests.Mono.Unix {

	[TestFixture, Category ("NotOnWindows")]
	public class UnixSignalTest {

		// helper method to create a thread waiting on a UnixSignal
		static Thread CreateWaitSignalThread (UnixSignal signal, int timeout)
		{
			Thread t1 = new Thread(delegate() {
						var sw = Stopwatch.StartNew ();
						bool r = signal.WaitOne (timeout, false);
						sw.Stop ();
						Assert.AreEqual (signal.Count, 1);
						Assert.AreEqual (r, true);
						if (sw.Elapsed > new TimeSpan (0, 0, timeout/1000))
							throw new InvalidOperationException ("Signal slept too long");
					});
			return t1;
		}

		// helper method to create a two-thread test
		static void MultiThreadTest (UnixSignal signal, int timeout, ThreadStart tstart)
		{
			Thread t1 = CreateWaitSignalThread (signal, timeout);
			Thread t2 = new Thread (tstart);
			t1.Start ();
			t2.Start ();
			t1.Join ();
			t2.Join ();
		}

		[Test]
		public void TestNestedInvocation()
		{
			UnixSignal s = new UnixSignal(Signum.SIGINT);
			Thread a = new Thread(delegate() {
					bool r = s.WaitOne (1000, false);
      });
			Thread b = new Thread(delegate() {
					bool r = s.WaitOne (500, false);
      });
			a.Start();
			b.Start();
			a.Join();
			b.Join();
		}

		[Test]
		public void TestWaitAnyFailsWithMore64Signals()
		{
			UnixSignal s1 = new UnixSignal(Signum.SIGINT);
			UnixSignal[] signals = new UnixSignal[65];
			for (int i=0; i<65; ++i)
				signals[i] = s1;
			
			Assert.That(UnixSignal.WaitAny(signals, new TimeSpan(0,0,1)), Is.EqualTo(-1));
		}

		[Test]
		public void TestConcurrentWaitOne()
		{
			UnixSignal s1 = new UnixSignal(Signum.SIGINT);
			UnixSignal s2 = new UnixSignal(Signum.SIGINT);
			Thread a = CreateWaitSignalThread(s1, 10000);
			Thread b = CreateWaitSignalThread(s2, 5000);
			Thread c = new Thread (delegate () {
					Thread.Sleep (1000);
					Stdlib.raise (Signum.SIGINT);
			});
			a.Start();
			b.Start();
			c.Start();
			a.Join();
			b.Join();
			c.Join();
			Assert.That(s1.Count, Is.EqualTo(1), "Expected 1 signal raised");
			Assert.That(s2.Count, Is.EqualTo(1), "Expected 1 signal raised");
		}

		[Test]
		public void TestConcurrentWaitOneSameInstance()
		{
			UnixSignal s1 = new UnixSignal(Signum.SIGINT);
			Thread a = CreateWaitSignalThread(s1, 10000);
			Thread b = CreateWaitSignalThread(s1, 10000);
			Thread c = new Thread (delegate () {
					Thread.Sleep (500);
					Stdlib.raise (Signum.SIGINT);
			});
			a.Start();
			b.Start();
			c.Start();
			a.Join();
			b.Join();
			c.Join();
		}

		[Test]
		[Category ("AndroidNotWorking")] // Crashes (silently) the runtime in similar fashion to real-time signals
		public void TestSignumProperty ()
		{
			UnixSignal signal1 = new UnixSignal (Signum.SIGSEGV);
			Assert.That (signal1.Signum, Is.EqualTo (Signum.SIGSEGV));
		}
	
		[Test]
		[Category ("NotOnMac")]
		public void TestRealTimeCstor ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
				return;
			RealTimeSignum rts = new RealTimeSignum (0);
			using (UnixSignal s = new UnixSignal (rts))
			{
				Assert.That(s.IsRealTimeSignal);
				Assert.That(s.RealTimeSignum, Is.EqualTo (rts));
			}
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestSignumPropertyThrows ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
				return;

			Assert.Throws<InvalidOperationException> (() => {
				UnixSignal signal1 = new UnixSignal (new RealTimeSignum (0));
				Signum s = signal1.Signum;
			});
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestRealTimeSignumProperty ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
				return;
			RealTimeSignum rts = new RealTimeSignum (0);
			UnixSignal signal1 = new UnixSignal (rts);
			Assert.That (signal1.RealTimeSignum, Is.EqualTo (rts));
		}
	
		[Test]
		[Category ("NotOnMac")]
		public void TestRealTimePropertyThrows ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
					return;

			Assert.Throws<InvalidOperationException> (() => {
				UnixSignal signal1 = new UnixSignal (Signum.SIGSEGV);
				RealTimeSignum s = signal1.RealTimeSignum;
			});
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestRaiseRTMINSignal ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
				return;
			RealTimeSignum rts = new RealTimeSignum (0);
			using (UnixSignal signal = new UnixSignal (rts))
			{
				MultiThreadTest (signal, 5000, delegate() {
					Thread.Sleep (1000);
					Stdlib.raise (rts);
					});
			}
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestRaiseRTMINPlusOneSignal ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
				return;
			/*this number is a guestimate, but it's ok*/
			for (int i = 1; i < 10; ++i) {
				RealTimeSignum rts = new RealTimeSignum (i);
				UnixSignal signal;
				try {
					signal  = new UnixSignal (rts);
				} catch (ArgumentException) { /*skip the ones that are unavailable*/
					continue;
				}
				using (signal)
				{
					MultiThreadTest (signal, 5000, delegate() {
						Thread.Sleep(1000);
						Stdlib.raise(rts);
						});
				}
				return;
			}
			Assert.IsTrue (false, "#1 No available RT signal");
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestCanRegisterRTSignalMultipleTimes ()
		{
			if (!TestHelper.CanUseRealTimeSignals ())
				return;
			/*this number is a guestimate, but it's ok*/
			for (int i = 1; i < 10; ++i) {
				RealTimeSignum rts = new RealTimeSignum (i);
				UnixSignal signal;
				try {
					signal  = new UnixSignal (rts);
				} catch (ArgumentException) { /*skip the ones that are unavailable*/
					continue;
				}
				try {
					using (UnixSignal signal2 =  new UnixSignal (rts))
					{
						//ok
						return;
					}
				} catch (ArgumentException) { /*skip the ones that are unavailable*/
						Assert.IsTrue (false, "#1 Could not register second signal handler");
				}
			}
			Assert.IsTrue (false, "#2 No available RT signal");
		}

		[Test]
		public void TestRaise ()
		{
			Thread t1 = new Thread (delegate () {
					using (UnixSignal a = new UnixSignal (Signum.SIGINT)) {
						var sw = Stopwatch.StartNew ();
						bool r = a.WaitOne (5000, false);
						sw.Stop ();
						Assert.AreEqual (a.Count, 1);
						Assert.AreEqual (r, true);
						if (sw.Elapsed > new TimeSpan (0, 0, 5))
							throw new InvalidOperationException ("Signal slept too long");
					}
			});
			Thread t2 = new Thread (delegate () {
					Thread.Sleep (1000);
					Stdlib.raise (Signum.SIGINT);
			});
			t1.Start ();
			t2.Start ();
			t1.Join ();
			t2.Join ();
		}

		[Test]
		public void TestRaiseAny ()
		{
			Thread t1 = new Thread (delegate () {
					using (UnixSignal a = new UnixSignal (Signum.SIGINT)) {
						var sw = Stopwatch.StartNew ();
						int idx = UnixSignal.WaitAny (new UnixSignal[]{a}, 5000);
						sw.Stop ();
						Assert.AreEqual (idx, 0);
						Assert.AreEqual (a.Count, 1);
						if (sw.Elapsed > new TimeSpan (0, 0, 5))
							throw new InvalidOperationException ("Signal slept too long");
					}
			});
			Thread t2 = new Thread (delegate () {
					Thread.Sleep (1000);
					Stdlib.raise (Signum.SIGINT);
			});
			t1.Start ();
			t2.Start ();
			t1.Join ();
			t2.Join ();
		}

		[Test]
		public void TestSeparation ()
		{
			Thread t1 = new Thread (delegate () {
					using (UnixSignal a = new UnixSignal (Signum.SIGINT))
					using (UnixSignal b = new UnixSignal (Signum.SIGTERM)) {
						var sw = Stopwatch.StartNew ();
						int idx = UnixSignal.WaitAny (new UnixSignal[]{a, b}, 5000);
						sw.Stop ();
						Assert.AreEqual (idx, 1);
						Assert.AreEqual (a.Count, 0);
						Assert.AreEqual (b.Count, 1);
						if (sw.Elapsed > new TimeSpan (0, 0, 5))
							throw new InvalidOperationException ("Signal slept too long");
					}
			});
			Thread t2 = new Thread (delegate () {
					Thread.Sleep (1000);
					Stdlib.raise (Signum.SIGTERM);
			});
			t1.Start ();
			t2.Start ();
			t1.Join ();
			t2.Join ();
		}

		[Test]
		public void TestNoEmit ()
		{
			using (UnixSignal u = new UnixSignal (Signum.SIGINT)) {
				var sw = Stopwatch.StartNew ();
				bool r = u.WaitOne (5100, false);
				Assert.AreEqual (r, false);
				sw.Stop ();
				if (sw.Elapsed < new TimeSpan (0, 0, 5))
					throw new InvalidOperationException ("Signal didn't block for 5s; blocked for " + sw.Elapsed.TotalSeconds.ToString());
			}
		}

		[Test]
		public void TestNoEmitAny ()
		{
			using (UnixSignal u = new UnixSignal (Signum.SIGINT)) {
				int idx = UnixSignal.WaitAny (new UnixSignal[]{u}, 5100);
				Assert.AreEqual (idx, 5100);
			}
		}

		[Test]
		public void TestDispose1 ()
		{
			UnixSignal a = new UnixSignal (Signum.SIGINT);
			UnixSignal b = new UnixSignal (Signum.SIGINT);

			Stdlib.raise (Signum.SIGINT);
			SleepUntilSignaled (a);

			Assert.AreEqual (a.Count, 1);
			Assert.AreEqual (b.Count, 1);

			a.Close ();
			b.Reset ();

			Stdlib.raise (Signum.SIGINT);
			SleepUntilSignaled (b);
			Assert.AreEqual (b.Count, 1);

			b.Close ();
		}

		static void SleepUntilSignaled (UnixSignal s)
		{
			for (int i = 0; i < 10; ++i) {
				if (s.Count > 0)
					break;
				Thread.Sleep (100);
			}
		}

		[Test]
		public void TestDispose2 ()
		{
			UnixSignal a = new UnixSignal (Signum.SIGINT);
			UnixSignal b = new UnixSignal (Signum.SIGINT);

			Stdlib.raise (Signum.SIGINT);
			SleepUntilSignaled (a);

			Assert.AreEqual (a.Count, 1);
			Assert.AreEqual (b.Count, 1);

			b.Close ();
			a.Reset ();

			Stdlib.raise (Signum.SIGINT);
			SleepUntilSignaled (a);
			Assert.AreEqual (a.Count, 1);

			a.Close ();
		}

		[Test]
		[Category ("AndroidNotWorking")] // Android 4.4.4 doesn't have signal(2)
		public void TestSignalActionInteraction ()
		{
			using (UnixSignal a = new UnixSignal (Signum.SIGINT)) {
				Stdlib.SetSignalAction (Signum.SIGINT, SignalAction.Ignore);
				Stdlib.raise (Signum.SIGINT);
				Assert.AreEqual (a.Count, 0); // never invoked
			}
		}

		static readonly Signum[] signals = new Signum[] {
			Signum.SIGHUP, Signum.SIGINT, Signum.SIGTERM, Signum.SIGCONT,
		};

		const int StormCount = 100000;

		[Test]
		public void TestRaiseStorm ()
		{
			UnixSignal[] usignals = CreateSignals (signals);
			Thread[] threads = new Thread[]{
				CreateRaiseStormThread (StormCount/4),
				CreateRaiseStormThread (StormCount/4),
				CreateRaiseStormThread (StormCount/4),
				CreateRaiseStormThread (StormCount/4),
			};
			foreach (Thread t in threads)
				t.Start ();
			foreach (Thread t in threads)
				t.Join ();
			AssertCountSet (usignals);
			// signal delivery might take some time, wait a bit before closing
			// the UnixSignal so we can ignore it and not terminate the process
			// when a SIGHUP/SIGTERM arrives afterwards
			Thread.Sleep (1000);
			CloseSignals (usignals);
		}

		static void AssertCount (UnixSignal[] usignals)
		{
			int sum = 0;
			foreach (UnixSignal s in usignals)
				sum += s.Count;
			Assert.AreEqual (sum, StormCount);
		}

		static void AssertCountSet (UnixSignal[] usignals)
		{
			foreach (UnixSignal s in usignals) {
				Assert.IsTrue (s.Count > 0);
			}
		}

		static UnixSignal[] CreateSignals (Signum[] signals)
		{
			UnixSignal[] s = new UnixSignal [signals.Length];
			for (int i = 0; i < signals.Length; ++i)
				s [i] = new UnixSignal (signals [i]);
			return s;
		}

		static void CloseSignals (UnixSignal[] signals)
		{
			foreach (UnixSignal s in signals)
				s.Close ();
		}

		// Create thread that issues many signals from a set of harmless signals
		static Thread CreateRaiseStormThread (int max)
		{
			return new Thread (delegate () {
				Random r = new Random (Environment.TickCount);
				for (int i = 0; i < max; ++i) {
					int n = r.Next (0, signals.Length);
					Stdlib.raise (signals [n]);
				}
			});
		}

		[Test]
		public void TestAddRemove ()
		{
			UnixSignal[] usignals = CreateSignals (signals);

			Thread[] threads = new Thread[]{
				CreateRaiseStormThread (StormCount),
				CreateSignalCreatorThread (),
			};

			foreach (Thread t in threads)
				t.Start ();
			foreach (Thread t in threads)
				t.Join ();

			AssertCountSet (usignals);
			CloseSignals (usignals);
		}

		// Create thread that repeatedly registers then unregisters signal handlers
		static Thread CreateSignalCreatorThread ()
		{
			return new Thread (delegate () {
				Random r = new Random (Environment.TickCount << 4);
				for (int i = 0; i < StormCount; ++i) {
					int n = r.Next (0, signals.Length);
					using (new UnixSignal (signals [n]))
					using (new UnixSignal (signals [(n+1)%signals.Length]))
					using (new UnixSignal (signals [(n+2)%signals.Length]))
					using (new UnixSignal (signals [(n+3)%signals.Length])) {
					}
				}
			});
		}

		[Test]
		public void TestWaitAny ()
		{
			UnixSignal[] usignals = CreateSignals (signals);

			Thread[] threads = new Thread[]{
				CreateRaiseStormThread (StormCount),
				CreateSignalCreatorThread (),
				CreateWaitAnyThread (usignals [0], usignals [2]),
				CreateWaitAnyThread (usignals [1], usignals [3]),
				CreateWaitAnyThread (usignals [1], usignals [2]),
			};

			foreach (Thread t in threads)
				t.Start ();
			foreach (Thread t in threads)
				t.Join ();

			AssertCountSet (usignals);
			CloseSignals (usignals);
		}

		// Create thread that blocks until at least one of the given signals is received
		static Thread CreateWaitAnyThread (params UnixSignal[] usignals)
		{
			return new Thread (delegate () {
				int idx = UnixSignal.WaitAny (usignals, 30000);
				Assert.AreEqual (idx >= 0 && idx < usignals.Length, true);
			});
		}
	}
}
