//
// TimerTest.cs - NUnit test cases for System.Threading.Timer
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//   Rafael Ferreira (raf@ophion.org)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Collections;

namespace MonoTests.System.Threading {
	[TestFixture]
	public class TimerTest {
		// this bucket is used to avoid non-theadlocal issues
		class Bucket {
			public int count;
		}

		[SetUp]
		public void Setup ()
		{
			//creating a timer that will never run just to make sure the
			// scheduler is warm for the unit tests
			// this makes fair for the "DueTime" test since it 
			// doesn't have to wait for the scheduler thread to be 
			// created. 
			new Timer (new TimerCallback (DoNothing), null, Timeout.Infinite, 0);
		}

		void DoNothing (object foo)
		{
		}

		[Test]
		public void TestDueTime ()
		{
			Bucket bucket = new Bucket();
			Timer t = new Timer (new TimerCallback (Callback), bucket, 200, Timeout.Infinite);
			Thread.Sleep (50);
			Assert.AreEqual (0, bucket.count, "#1");
			Thread.Sleep (200);
			Assert.AreEqual (1, bucket.count, "#2");
			Thread.Sleep (500);
			Assert.AreEqual (1, bucket.count, "#3");
			t.Change (10, 10);
			Thread.Sleep (1000);
			Assert.IsTrue(bucket.count > 20, "#4");
			t.Dispose ();
		}

		[Test]
		public void TestChange ()
		{
			Bucket bucket = new Bucket();
			Timer t = new Timer (new TimerCallback (Callback), bucket, 10, 10);
			Thread.Sleep (500);
			int c = bucket.count;
			Assert.IsTrue(c > 20, "#1");
			t.Change (100, 100);
			c = bucket.count;
			Thread.Sleep (500);
			Assert.IsTrue(bucket.count <= c + 20, "#2");
			t.Dispose ();
		}

		[Test]
		public void TestZeroDueTime ()
		{
			Bucket bucket = new Bucket();

			Timer t = new Timer (new TimerCallback (Callback), bucket, 0, Timeout.Infinite);
			Thread.Sleep (100);
			Assert.AreEqual (1, bucket.count, "#1");
			t.Change (0, Timeout.Infinite);
			Thread.Sleep (100);
			Assert.AreEqual (2, bucket.count, "#2");
			t.Dispose ();
		}

		[Test]
		public void TestDispose ()
		{	
			Bucket bucket = new Bucket();
			Timer t = new Timer (new TimerCallback (Callback), bucket, 10, 10);
			Thread.Sleep (200);
			t.Dispose ();
			Thread.Sleep (20);
			int c = bucket.count;
			Assert.IsTrue (bucket.count > 5, "#1");
			Thread.Sleep (200);
			Assert.AreEqual (c, bucket.count, "#2");
		}

		[Test] // bug #320950
		public void TestDispose2 ()
		{
			Timer t = new Timer (new TimerCallback (Callback), null, 10, 10);
			t.Dispose ();
			t.Dispose ();
		}
		
		[Test]
		public void TestHeavyCreationLoad ()
		{
			Bucket b = new Bucket ();
			for (int i = 0; i < 500; ++i)
				new Timer (new TimerCallback (Callback), b, 10,
					Timeout.Infinite);
			// 1000 * 10 msec = 10,000 msec or 10 sec - if everything goes well
			// we add some slack to cope with timing issues caused by system load etc.
			for (int i = 0; i < 20; ++i) {
				if (b.count == 500)
					break;
				Thread.Sleep (1000);
			}
			Assert.AreEqual (500, b.count);
			
		}

		[Test]
		public void TestQuickDisposeDeadlockBug ()
		{
			int i = 0;
			Bucket b = new Bucket ();
			ArrayList timers = new ArrayList();
			while (i < 500) {
				Timer t = new Timer (new TimerCallback (Callback),
					b, 10, Timeout.Infinite);
				timers.Add (t);
				i++;
				t.Dispose ();
			}
			Thread.Sleep (11 * 500);
		}

		[Test]
		public void TestInt32MaxDelay ()
		{
			Bucket b = new Bucket ();
			new Timer (new TimerCallback (Callback), b, Int32.MaxValue,
				Timeout.Infinite);
			Thread.Sleep (50);
			Assert.AreEqual (0, b.count);
		}

		[Test]
		public void TestInt32MaxPeriod ()
		{
			Bucket b = new Bucket ();
			new Timer (new TimerCallback (Callback), b, 0,
				Int32.MaxValue);
			Thread.Sleep (50);
			Assert.AreEqual (1, b.count);
			
		}

		[Test]
		public void TestNegativeDelay ()
		{
			Bucket b = new Bucket ();
			try {
				new Timer (new TimerCallback (Callback), b, -10,
					Timeout.Infinite);
				Assert.Fail ();
			} catch (ArgumentOutOfRangeException) {
				return;
			}
		}

		[Test]
		public void TestNegativePeriod ()
		{
			Bucket b = new Bucket ();
			try {
				new Timer (new TimerCallback (Callback), b, 0,
					-10);
				Assert.Fail ();
			} catch (ArgumentOutOfRangeException) {
				return;
			}
		}

		[Test]
		public void TestDelayZeroPeriodZero()
		{
			Bucket b = new Bucket();
			Timer t = new Timer(new TimerCallback(Callback),b,0,0);
			Thread.Sleep(100);
			t.Change (int.MaxValue, Timeout.Infinite);
			// since period is 0 the callback should happen once (bug #340212)
			Assert.IsTrue(b.count == 1);
			
		}

		[Test]
		[Ignore ()]
		public void TestDisposeOnCallback ()
		{
			// this test is bad, as the provided `state` (t1) is null and will throw an NRE inside the callback
			// that was ignored before 238785a3e3d510528228fc551625975bc508c2f3 and most unit test runner won't
			// report it since the NRE will not happen on the main thread (but Touch.Unit will)
			Timer t1 = null;
			t1 = new Timer (new TimerCallback (CallbackTestDisposeOnCallback), t1, 0, 10);
			Thread.Sleep (200);
			Assert.IsNotNull (t1);
			
		}

		private void CallbackTestDisposeOnCallback (object foo)
		{
			((Timer) foo).Dispose ();
		}

		private void Callback (object foo)
		{
			Bucket b = foo as Bucket;
			Interlocked.Increment (ref b.count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DisposeNullWaitHandle ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Dispose (null);
			}
		}

		[Test]
		public void Change_IntInt_Infinite ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change ((int)Timeout.Infinite, (int)Timeout.Infinite);
			}
		}

		[Test]
		public void Change_IntInt_MaxValue ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change (Int32.MaxValue, Int32.MaxValue);
			}
		}

		[Test]
		public void Change_UIntUInt_Infinite ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change (unchecked ((uint) Timeout.Infinite), unchecked ((uint) Timeout.Infinite));
			}
		}

		[Test]
		public void Change_UIntUInt_MaxValue ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				// UInt32.MaxValue == Timeout.Infinite == 0xffffffff
				t.Change (UInt32.MaxValue, UInt32.MaxValue);
			}
		}

		[Test]
		public void Change_LongLong_Infinite ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change ((long) Timeout.Infinite, (long) Timeout.Infinite);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Change_LongLong_MaxValue ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change (Int64.MaxValue, Int64.MaxValue);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Change_LongLong_UInt32MaxValue ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				// not identical to (long)-1
				t.Change ((long)UInt32.MaxValue, (long)UInt32.MaxValue);
			}
		}

		[Test]
		public void Change_LongLong_UInt32MaxValueMinusOne ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				// not identical to (long)-1
				t.Change ((long) UInt32.MaxValue - 1, (long) UInt32.MaxValue -1);
			}
		}

		[Test]
		public void Change_TimeSpanTimeSpan_Infinite ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change (new TimeSpan (-1), new TimeSpan (-1));
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Change_TimeSpanTimeSpan_MaxValue ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change (TimeSpan.MaxValue, TimeSpan.MaxValue);
			}
		}

		[Test]
		public void Change_TimeSpanTimeSpan_UInt32MaxValue ()
		{
			using (Timer t = new Timer (DoNothing, null, 0, 0)) {
				t.Change (new TimeSpan (UInt32.MaxValue), new TimeSpan (UInt32.MaxValue));
			}
		}
	}
}
