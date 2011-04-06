//
// UnixSignalTest.cs - NUnit Test Cases for Mono.Unix.UnixSignal
//
// Authors:
//	Jonathan Pryor  <jonpryor@vt.edu>
//
// (C) 2008 Jonathan Pryor
//

using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Text;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace NUnit.Framework.SyntaxHelpers { class Dummy {} }

namespace MonoTests.Mono.Unix {

	[TestFixture]
	public class UnixSignalTest {

		// helper method to create a thread waiting on a UnixSignal
		static Thread CreateWaitSignalThread (UnixSignal signal, int timeout)
		{
			Thread t1 = new Thread(delegate() {
						DateTime start = DateTime.Now;
						bool r = signal.WaitOne (timeout, false);
						DateTime end = DateTime.Now;
						Assert.AreEqual (signal.Count, 1);
						Assert.AreEqual (r, true);
						if ((end - start) > new TimeSpan (0, 0, timeout/1000))
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
		public void TestSignumProperty ()
		{
			UnixSignal signal1 = new UnixSignal (Signum.SIGSEGV);
			Assert.That (signal1.Signum, Is.EqualTo (Signum.SIGSEGV));
		}
	
		[Test]
		[Category ("NotOnMac")]
		public void TestRealTimeCstor ()
		{
			RealTimeSignum rts = new RealTimeSignum (0);
			using (UnixSignal s = new UnixSignal (rts))
			{
				Assert.That(s.IsRealTimeSignal);
				Assert.That(s.RealTimeSignum, Is.EqualTo (rts));
			}
		}

		[Test]
		[ExpectedException]
		[Category ("NotOnMac")]
		public void TestSignumPropertyThrows ()
		{
			UnixSignal signal1 = new UnixSignal (new RealTimeSignum (0));
			Signum s = signal1.Signum;
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestRealTimeSignumProperty ()
		{
			RealTimeSignum rts = new RealTimeSignum (0);
			UnixSignal signal1 = new UnixSignal (rts);
			Assert.That (signal1.RealTimeSignum, Is.EqualTo (rts));
		}
	
		[Test]
		[ExpectedException]
		[Category ("NotOnMac")]
		public void TestRealTimePropertyThrows ()
		{
			UnixSignal signal1 = new UnixSignal (Signum.SIGSEGV);
			RealTimeSignum s = signal1.RealTimeSignum;
		}

		[Test]
		[Category ("NotOnMac")]
		public void TestRaiseRTMINSignal ()
		{
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
						DateTime start = DateTime.Now;
						bool r = a.WaitOne (5000, false);
						DateTime end = DateTime.Now;
						Assert.AreEqual (a.Count, 1);
						Assert.AreEqual (r, true);
						if ((end - start) > new TimeSpan (0, 0, 5))
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
						DateTime start = DateTime.Now;
						int idx = UnixSignal.WaitAny (new UnixSignal[]{a}, 5000);
						DateTime end = DateTime.Now;
						Assert.AreEqual (idx, 0);
						Assert.AreEqual (a.Count, 1);
						if ((end - start) > new TimeSpan (0, 0, 5))
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
						DateTime start = DateTime.Now;
						int idx = UnixSignal.WaitAny (new UnixSignal[]{a, b}, 5000);
						DateTime end = DateTime.Now;
						Assert.AreEqual (idx, 1);
						Assert.AreEqual (a.Count, 0);
						Assert.AreEqual (b.Count, 1);
						if ((end - start) > new TimeSpan (0, 0, 5))
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
				DateTime start = DateTime.Now;
				bool r = u.WaitOne (5100, false);
				Assert.AreEqual (r, false);
				DateTime end = DateTime.Now;
				if ((end - start) < new TimeSpan (0, 0, 5))
					throw new InvalidOperationException ("Signal didn't block for 5s; blocked for " + (end-start).ToString());
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

			Assert.AreEqual (a.Count, 1);
			Assert.AreEqual (b.Count, 1);

			a.Close ();
			b.Reset ();

			Stdlib.raise (Signum.SIGINT);
			Assert.AreEqual (b.Count, 1);

			b.Close ();
		}

		[Test]
		public void TestDispose2 ()
		{
			UnixSignal a = new UnixSignal (Signum.SIGINT);
			UnixSignal b = new UnixSignal (Signum.SIGINT);

			Stdlib.raise (Signum.SIGINT);

			Assert.AreEqual (a.Count, 1);
			Assert.AreEqual (b.Count, 1);

			b.Close ();
			a.Reset ();

			Stdlib.raise (Signum.SIGINT);
			Assert.AreEqual (a.Count, 1);

			a.Close ();
		}

		[Test]
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
		[Category("NotOnMac")] // OSX signal storming will not deliver every one
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
			AssertCount (usignals);
			CloseSignals (usignals);
		}

		static void AssertCount (UnixSignal[] usignals)
		{
			int sum = 0;
			foreach (UnixSignal s in usignals)
				sum += s.Count;
			Assert.AreEqual (sum, StormCount);
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
		[Category("NotOnMac")] // OSX signal storming will not deliver every one
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

			AssertCount (usignals);
			CloseSignals (usignals);
		}

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
		[Category("NotOnMac")] // OSX signal storming will not deliver every one
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

			AssertCount (usignals);
			CloseSignals (usignals);
		}

		static Thread CreateWaitAnyThread (params UnixSignal[] usignals)
		{
			return new Thread (delegate () {
				int idx = UnixSignal.WaitAny (usignals, 30000);
				Assert.AreEqual (idx >= 0 && idx < usignals.Length, true);
			});
		}
	}
}
