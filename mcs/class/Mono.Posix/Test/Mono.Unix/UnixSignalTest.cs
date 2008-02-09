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
using System.Text;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace MonoTests.Mono.Unix {

	[TestFixture]
	public class UnixSignalTest {
		[Test]
		public void TestRaise ()
		{
			Thread t1 = new Thread (delegate {
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
			Thread t2 = new Thread (delegate {
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
			Thread t1 = new Thread (delegate {
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
			Thread t2 = new Thread (delegate {
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
			Thread t1 = new Thread (delegate {
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
			Thread t2 = new Thread (delegate {
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
			return new Thread (delegate {
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

			AssertCount (usignals);
			CloseSignals (usignals);
		}

		static Thread CreateSignalCreatorThread ()
		{
			return new Thread (delegate {
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

			AssertCount (usignals);
			CloseSignals (usignals);
		}

		static Thread CreateWaitAnyThread (params UnixSignal[] usignals)
		{
			return new Thread (delegate {
				int idx = UnixSignal.WaitAny (usignals);
				Assert.AreEqual (idx >= 0 && idx < usignals.Length, true);
			});
		}
	}
}
