// ThreadTest.cs - NUnit Test Cases for the System.Threading.Thread class
//
// Authors
//	Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Eduardo Garcia Cebollero.
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using SD = System.Diagnostics;

using NUnit.Framework;

namespace MonoTests.System.Threading
{
#if !DISABLE_SECURITY
	// These tests seem to hang the 2.0 framework. So they are disabled for now
	// Don't reenable them until you can run a few thousand times on an SMP box.
	[Category ("NotWorking")]
	public class ThreadedPrincipalTest
	{
		public static void NoPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			Assert.IsNull (p, "#1");

			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			Assert.IsNotNull (Thread.CurrentPrincipal, "#2");

			Thread.CurrentPrincipal = null;
			Assert.IsNull (Thread.CurrentPrincipal, "#3");
			// in this case we can return to null
		}

		public static void UnauthenticatedPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.UnauthenticatedPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			Assert.IsNotNull (p, "#1");
			Assert.IsTrue ((p is GenericPrincipal), "#2");
			Assert.AreEqual (String.Empty, p.Identity.Name, "#3");
			Assert.AreEqual (String.Empty, p.Identity.AuthenticationType, "#4");
			Assert.IsFalse (p.Identity.IsAuthenticated, "#5");

			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			Assert.IsNotNull (Thread.CurrentPrincipal, "#6");

			Thread.CurrentPrincipal = null;
			Assert.IsNotNull (Thread.CurrentPrincipal, "#7");
			// in this case we can't return to null
		}

		public static void WindowsPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.WindowsPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			Assert.IsNotNull (p, "#1");
			Assert.IsTrue ((p is WindowsPrincipal), "#2");
			Assert.IsNotNull (p.Identity.Name, "#3");
			Assert.IsNotNull (p.Identity.AuthenticationType, "#4");
			Assert.IsTrue (p.Identity.IsAuthenticated, "#5");

			// note: we can switch from a WindowsPrincipal to a GenericPrincipal
			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			Assert.IsNotNull (Thread.CurrentPrincipal, "#6");

			Thread.CurrentPrincipal = null;
			Assert.IsNotNull (Thread.CurrentPrincipal, "#7");
			// in this case we can't return to null
		}

		public static void CopyOnNewThread ()
		{
			Assert.IsNotNull (Thread.CurrentPrincipal, "#1");
			Assert.AreEqual ("good", Thread.CurrentPrincipal.Identity.Name, "#2");
		}
	}
#endif

	[TestFixture]
	[Category("MobileNotWorking")] // Abort #10240
	public class ThreadTest
	{
		//TimeSpan Infinite = new TimeSpan (-10000);	// -10000 ticks == -1 ms
		TimeSpan SmallNegative = new TimeSpan (-2);	// between 0 and -1.0 (infinite) ms
		TimeSpan Negative = new TimeSpan (-20000);	// really negative
		//TimeSpan MaxValue = TimeSpan.FromMilliseconds ((long) Int32.MaxValue);
		TimeSpan TooLarge = TimeSpan.FromMilliseconds ((long) Int32.MaxValue + 1);

		//Some Classes to test as threads
		private class C1Test
		{
			public int cnt;
			public Thread thread1;
			public bool endm1;
			public bool endm2;

			public C1Test()
			{
				thread1 = (Thread)null;
				this.cnt = 0;
				endm1 = endm2 = false;
			}
			
			public void TestMethod()
			{
				while (cnt < 10)
				{
					cnt++;
				}
				endm1 = true;
			}
			public void TestMethod2()
			{
				if (!(thread1==(Thread)null) )
				{
					thread1.Join();
				}
				endm2 = true;
			}
		}

		private class C2Test
		{
			public int cnt;
			public bool run = false;
			
			public C2Test()
			{
				this.cnt = 0;
			}

			public void TestMethod()
			{
				run = true;
				while (true)
				{
					if (cnt < 1000)
						cnt++;
					else
						cnt = 0;
				}
			}
		}
		
		private class C3Test
		{
			public C1Test sub_class;
			public Thread sub_thread;

			public C3Test()
			{
				sub_class = new C1Test();
				sub_thread = new Thread(new ThreadStart(sub_class.TestMethod));
			}

			public void TestMethod1()
			{
				sub_thread.Start();
				Thread.Sleep (100);
#if MONO_FEATURE_THREAD_ABORT
				sub_thread.Abort();
#else
				sub_thread.Interrupt ();
#endif
			}
		}
		
		private class C4Test
		{
			public C1Test class1;
			public C1Test class2;
			public Thread thread1;
			public Thread thread2;
			public bool T1ON ;
			public bool T2ON ;

			public C4Test()
			{
				T1ON = false;
				T2ON = false;
				class1 = new C1Test();
				class2 = new C1Test();
				thread1 = new Thread(new ThreadStart(class1.TestMethod));
				thread2 = new Thread(new ThreadStart(class2.TestMethod));
			}

			public void TestMethod1()
			{
				thread1.Start();
				TestUtil.WaitForAlive (thread1, "wait1");
				T1ON = true;
				thread2.Start();
				TestUtil.WaitForAlive (thread2, "wait2");
				T2ON = true;
#if MONO_FEATURE_THREAD_ABORT
				thread1.Abort();
#else
				thread1.Interrupt ();
#endif
				TestUtil.WaitForNotAlive (thread1, "wait3");
				T1ON = false;
#if MONO_FEATURE_THREAD_ABORT
				thread2.Abort();
#else
				thread2.Interrupt ();
#endif
				TestUtil.WaitForNotAlive (thread2, "wait4");
				T2ON = false;
			}
			
			public void TestMethod2()
			{
				thread1.Start();
				thread1.Join();
			}
		}

		[Test]
		public void TestCtor1()
		{
			C1Test test1 = new C1Test();
			Thread t = new Thread (new ThreadStart (test1.TestMethod));

			Assert.IsTrue (t.CurrentCulture.IsReadOnly, "CurrentCulture.IsReadOnly");
			Assert.IsFalse (t.IsAlive, "IsAlive");
			Assert.IsFalse (t.IsBackground, "IsBackground");
			Assert.IsNull (t.Name, "Name");
			Assert.AreEqual (ThreadState.Unstarted, t.ThreadState, "ThreadState");
		}

		[Test]
		[Category ("NotWorking")] // we're not sharing (read-only) CultureInfo
		public void CultureInfo_Shared_Across_Threads ()
		{
			Thread t = new Thread (TestCtor1);
			Assert.AreSame (t.CurrentCulture, t.CurrentUICulture, "Culture");

			Assert.AreSame (t.CurrentCulture, CultureInfo.CurrentCulture, "CultureInfo.CurrentCulture");
			Assert.AreSame (t.CurrentUICulture, CultureInfo.CurrentUICulture, "CultureInfo.CurrentUICulture");

			Assert.AreSame (t.CurrentCulture, Thread.CurrentThread.CurrentCulture, "Thread.CurrentThread.CurrentCulture");
			Assert.AreSame (t.CurrentUICulture, Thread.CurrentThread.CurrentUICulture, "Thread.CurrentThread.CurrentUICulture");
		}

		[Test] // bug #325566
		[Category ("MultiThreaded")]
		public void GetHashCodeTest ()
		{
			C1Test test1 = new C1Test ();
			Thread tA = new Thread (new ThreadStart (test1.TestMethod));
			int hA1 = tA.GetHashCode ();
			Assert.IsTrue (hA1 > 0, "#A1");
			tA.Start ();
			int hA2 = tA.GetHashCode ();
			Assert.AreEqual (hA1, hA2, "#A2");
			tA.Join ();
			int hA3 = tA.GetHashCode ();
			Assert.AreEqual (hA1, hA3, "#A3");
			Assert.AreEqual (hA1, tA.ManagedThreadId, "#A4");

			test1 = new C1Test ();
			Thread tB = new Thread (new ThreadStart (test1.TestMethod));
			int hB1 = tB.GetHashCode ();
			Assert.IsTrue (hB1 > 0, "#B1");
			tB.Start ();
			int hB2 = tB.GetHashCode ();
			Assert.AreEqual (hB1, hB2, "#B2");
			tB.Join ();
			int hB3 = tB.GetHashCode ();
			Assert.AreEqual (hB1, hB3, "#B3");
			Assert.AreEqual (hB1, tB.ManagedThreadId, "#B4");
			Assert.IsFalse (hA2 == hB2, "#B5");
		}

		[Test] // bug #82700
		[Category ("MultiThreaded")]
		public void ManagedThreadId ()
		{
			C1Test test1 = new C1Test ();
			Thread t1 = new Thread (new ThreadStart (test1.TestMethod));
			int mtA1 = t1.ManagedThreadId;
			t1.Start ();
			int mtA2 = t1.ManagedThreadId;
			t1.Join ();
			int mtA3 = t1.ManagedThreadId;
			Assert.AreEqual (mtA1, mtA2, "#A1");
			Assert.AreEqual (mtA2, mtA3, "#A2");

			test1 = new C1Test ();
			Thread t2 = new Thread (new ThreadStart (test1.TestMethod));
			int mtB1 = t2.ManagedThreadId;
			t2.Start ();
			int mtB2 = t2.ManagedThreadId;
			t2.Join ();
			int mtB3 = t2.ManagedThreadId;
			Assert.AreEqual (mtB1, mtB2, "#B1");
			Assert.AreEqual (mtB2, mtB3, "#B2");
			Assert.IsFalse (mtB1 == mtA1, "#B3");
		}

		[Test]
		[Category ("NotDotNet")] // it hangs.
		[Category ("MultiThreaded")]
		public void TestStart()
		{
		{
			C1Test test1 = new C1Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			TestThread.Join();
			Assert.AreEqual (10, test1.cnt, "#1");
		}
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
#if MONO_FEATURE_THREAD_ABORT
			TestThread.Abort();
#else
			TestThread.Interrupt ();
#endif
			try {
				TestThread.Start();
				Assert.Fail ("#2");
			} catch (ThreadStateException) {
			}
		}
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			while (!test1.run) {
			}
			bool started = (TestThread.ThreadState == ThreadState.Running);
			Assert.AreEqual (started, test1.run, "#15 Thread Is not in the correct state: ");
#if MONO_FEATURE_THREAD_ABORT
			TestThread.Abort();
#else
			TestThread.Interrupt ();
#endif
		}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestApartmentState ()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			Assert.AreEqual (ApartmentState.Unknown, TestThread.ApartmentState, "#1");
			TestThread.Start();
			TestUtil.WaitForAlive (TestThread, "wait5");
			Assert.AreEqual (ApartmentState.MTA, TestThread.ApartmentState, "#2");
#if MONO_FEATURE_THREAD_ABORT
			TestThread.Abort();
#else
			TestThread.Interrupt ();
#endif
		}

		[Test]
		[Category ("NotWorking")] // setting the priority of a Thread before it is started isn't implemented in Mono yet
		public void TestPriority1()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Priority=ThreadPriority.BelowNormal;
				ThreadPriority before = TestThread.Priority;
				Assert.AreEqual (ThreadPriority.BelowNormal, before, "#40 Unexpected priority before thread start: ");
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait7");
				ThreadPriority after = TestThread.Priority;
				Assert.AreEqual (before, after, "#41 Unexpected Priority Change: ");
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
		}

#if MONO_FEATURE_THREAD_ABORT
		[Test]
		[Category ("NotDotNet")] // on MS, Thread is still in AbortRequested state when Start is invoked
		public void AbortUnstarted ()
		{
			C2Test test1 = new C2Test();
			Thread th = new Thread (new ThreadStart (test1.TestMethod));
			th.Abort ();
			th.Start ();
		}
#endif

		[Test]
		[Category ("NotDotNet")] // on MS, ThreadState is immediately Stopped after Abort
		[Category ("NotWorking")] // this is a MonoTODO -> no support for Priority
		public void TestPriority2()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#42 Incorrect Priority in New thread: ");
				TestThread.Start();
				TestUtil.WaitForAliveOrStop (TestThread, "wait8");
				Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#43 Incorrect Priority in Started thread: ");
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
			Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#44 Incorrect Priority in Aborted thread: ");
		}

		[Test]
		[Category ("NotWorking")] // this is a MonoTODO -> no support for Priority
		public void TestPriority3()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestThread.Priority = ThreadPriority.Lowest;
				Assert.AreEqual (ThreadPriority.Lowest, TestThread.Priority, "#45A Incorrect Priority:");
				TestThread.Priority = ThreadPriority.BelowNormal;
				Assert.AreEqual (ThreadPriority.BelowNormal, TestThread.Priority, "#45B Incorrect Priority:");
				TestThread.Priority = ThreadPriority.Normal;
				Assert.AreEqual (ThreadPriority.Normal, TestThread.Priority, "#45C Incorrect Priority:");
				TestThread.Priority = ThreadPriority.AboveNormal;
				Assert.AreEqual (ThreadPriority.AboveNormal, TestThread.Priority, "#45D Incorrect Priority:");
				TestThread.Priority = ThreadPriority.Highest;
				Assert.AreEqual (ThreadPriority.Highest, TestThread.Priority, "#45E Incorrect Priority:");
			}
			finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestUndivisibleByPageSizeMaxStackSize ()
		{
			const int undivisible_stacksize = 1048573;

			var thread = new Thread (new ThreadStart (delegate {}), undivisible_stacksize);
			thread.Start ();
			thread.Join ();
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestIsBackground1 ()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait9");
				bool state = TestThread.IsBackground;
				Assert.IsFalse (state, "#51 IsBackground not set at the default state: ");
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestIsBackground2 ()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.IsBackground = true;
			try {
				TestThread.Start();
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
			
			if (TestThread.IsAlive) {
				try {
					Assert.IsTrue (TestThread.IsBackground, "#52 Is Background Changed to Start ");
				} catch (ThreadStateException) {
					// Ignore if thread died meantime
				}
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestName()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait10");
				string name = TestThread.Name;
				Assert.IsNull (name, "#61 Name set when mustn't be set: ");
				string newname = "Testing....";
				TestThread.Name = newname;
				Assert.AreEqual (newname, TestThread.Name, "#62 Name not set when must be set: ");
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
		}

		[Test]
		public void Name ()
		{
			Thread t = new Thread (new ThreadStart (Name));
			Assert.IsNull (t.Name, "Name-1");
			t.Name = null;
			Assert.IsNull (t.Name, "Name-2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Rename ()
		{
			Thread t = new Thread (new ThreadStart (Rename));
			t.Name = "a";
			t.Name = "b";
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestNestedThreads1()
		{
			C3Test test1 = new C3Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait11");
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestNestedThreads2()
		{
			C4Test test1 = new C4Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try {
				TestThread.Start();
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestJoin1()
		{
			C1Test test1 = new C1Test();
			C1Test test2 = new C1Test();
			Thread thread1 = new Thread(new ThreadStart(test1.TestMethod));
			Thread thread2 = new Thread(new ThreadStart(test1.TestMethod2));
			try {
				thread1.Start();
				thread2.Start();
				thread2.Join();
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				thread1.Abort();
				thread2.Abort();
#else
				thread1.Interrupt ();
				thread2.Interrupt ();
#endif
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Join_Int32_Negative ()
		{
			// -1 is Timeout.Infinite
			Thread.CurrentThread.Join (-2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Join_TimeSpan_Negative ()
		{
			Thread.CurrentThread.Join (Negative);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Join_TimeSpan_TooLarge ()
		{
			Thread.CurrentThread.Join (TooLarge);
		}

		[Test]
		public void Join_TimeSpan_SmallNegative ()
		{
			Thread.CurrentThread.Join (SmallNegative);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Sleep_Int32_Negative ()
		{
			// -1 is Timeout.Infinite
			Thread.Sleep (-2);
		}

		[Test]
		public void Sleep_TimeSpan_SmallNegative ()
		{
			Thread.Sleep (SmallNegative);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Sleep_TimeSpan_Negative ()
		{
			Thread.Sleep (Negative);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Sleep_TimeSpan_TooLarge ()
		{
			Thread.Sleep (TooLarge);
		}

		[Test]
		public void SpinWait ()
		{
			// no exception for negative numbers
			Thread.SpinWait (Int32.MinValue);
			Thread.SpinWait (0);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestThreadState ()
		{
			//TODO: Test The rest of the possible transitions
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			Assert.AreEqual (ThreadState.Unstarted, TestThread.ThreadState, "#101 Wrong Thread State");
			try {
				TestThread.Start();
				//while(!TestThread.IsAlive); //In the MS Documentation this is not necessary
											  //but in the MS SDK it is
				Assert.IsTrue (TestThread.ThreadState == ThreadState.Running || (TestThread.ThreadState & ThreadState.Unstarted) != 0,
					"#102 Wrong Thread State: " + TestThread.ThreadState.ToString ());
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				TestThread.Abort();
#else
				TestThread.Interrupt ();
#endif
			}
			
			TestUtil.WaitForNotAlive (TestThread, "wait12");
			// Docs say state will be Stopped, but Aborted happens sometimes (?)
			Assert.IsTrue ((ThreadState.Stopped & TestThread.ThreadState) != 0 || (ThreadState.Aborted & TestThread.ThreadState) != 0,
				"#103 Wrong Thread State: " + TestThread.ThreadState.ToString ());
		}

#if !DISABLE_SECURITY
		[Test]
		[Ignore ("see comment below.")]
		public void CurrentPrincipal_PrincipalPolicy_NoPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.NoPrincipal));
			try {
				t.Start ();
				t.Join ();
			} catch {
#if MONO_FEATURE_THREAD_ABORT
				t.Abort ();
#else
				t.Interrupt ();
#endif
			}
		}

		[Test]
		[Ignore ("see comment below.")]
		public void CurrentPrincipal_PrincipalPolicy_UnauthenticatedPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.UnauthenticatedPrincipal));
			try {
				t.Start ();
				t.Join ();
			} catch {
#if MONO_FEATURE_THREAD_ABORT
				t.Abort ();
#else
				t.Interrupt ();
#endif
			}
		}

		[Test]
		public void CurrentPrincipal_PrincipalPolicy_WindowsPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.WindowsPrincipal));
			try {
				t.Start ();
				t.Join ();
			} catch {
#if MONO_FEATURE_THREAD_ABORT
				t.Abort ();
#else
				t.Interrupt ();
#endif
			}
		}
		
		[Test]
		public void IPrincipal_CopyOnNewThread () 
		{
			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("bad"), null);
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.CopyOnNewThread));
			try {
				Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("good"), null);
				t.Start ();
				t.Join ();
			} catch {
#if MONO_FEATURE_THREAD_ABORT
				t.Abort ();
#else
				t.Interrupt ();
#endif
			}
		}
#endif

		int counter = 0;

#if MONO_FEATURE_THREAD_SUSPEND_RESUME
		[Test]
		[Category ("MultiThreaded")]
		public void TestSuspend ()
		{
			Thread t = new Thread (new ThreadStart (DoCount));
			t.IsBackground = true;
			t.Start ();
			
			CheckIsRunning ("t1", t);
			
			t.Suspend ();
			WaitSuspended ("t2", t);
			
			CheckIsNotRunning ("t3", t);
			
			t.Resume ();
			WaitResumed ("t4", t);
			
			CheckIsRunning ("t5", t);
			
			t.Abort ();
			TestUtil.WaitForNotAlive (t, "wait13");
			CheckIsNotRunning ("t6", t);
		}
#endif
		
#if MONO_FEATURE_THREAD_SUSPEND_RESUME && MONO_FEATURE_THREAD_ABORT
		[Test]
		[Category("NotDotNet")] // On MS, ThreadStateException is thrown on Abort: "Thread is suspended; attempting to abort"
		[Category ("MultiThreaded")]
		public void TestSuspendAbort ()
		{
			Thread t = new Thread (new ThreadStart (DoCount));
			t.IsBackground = true;
			t.Start ();
			
			CheckIsRunning ("t1", t);
			
			t.Suspend ();
			WaitSuspended ("t2", t);
			
			CheckIsNotRunning ("t3", t);
			
			t.Abort ();
			
			int n=0;
			while (t.IsAlive && n < 200) {
				Thread.Sleep (10);
				n++;
			}

			Assert.IsTrue (n < 200, "Timeout while waiting for abort");
			
			CheckIsNotRunning ("t6", t);
		}
#endif

		[Test]
		[Category ("MultiThreaded")]
		public void Test_Interrupt ()
		{
			ManualResetEvent mre = new ManualResetEvent (false);
			bool interruptedExceptionThrown = false;

			ThreadPool.QueueUserWorkItem (Test_Interrupt_Worker, Thread.CurrentThread);

			try {
				try {
					mre.WaitOne (3000);
				} finally {
					try {
						mre.WaitOne (0);
					} catch (ThreadInterruptedException) {
						Assert.Fail ("ThreadInterruptedException thrown twice");
					}
				}
			} catch (ThreadInterruptedException) {
				interruptedExceptionThrown = true;
			}

			Assert.IsTrue (interruptedExceptionThrown, "ThreadInterruptedException expected.");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestQueueUserWorkItemNullCallback ()
		{
			ThreadPool.QueueUserWorkItem (null, null);
		}

		private void Test_Interrupt_Worker (object o)
		{
			Thread t = o as Thread;
			Thread.Sleep (100);
			t.Interrupt ();
		}
		
		[Test]
		public void Test_InterruptCurrentThread ()
		{
			ManualResetEvent mre = new ManualResetEvent (false);
			bool interruptedExceptionThrown = false;

			Thread.CurrentThread.Interrupt ();
			try {
				mre.WaitOne (0);
				Assert.Fail ();
			} catch (ThreadInterruptedException) {
			}
		}

		[Test]
		public void GetNamedDataSlotTest ()
		{
			Assert.IsNotNull (Thread.GetNamedDataSlot ("te#st"), "#1");
			Assert.AreSame (Thread.GetNamedDataSlot ("te#st"), Thread.GetNamedDataSlot ("te#st"), "#2");
		}

		class DomainClass : MarshalByRefObject {
			Thread m_thread;
			bool success;

			public bool Run () {
				m_thread = new Thread(ThreadProc);
				m_thread.Start(Thread.CurrentThread);
				m_thread.Join();
				return success;
			}

			public void ThreadProc (object arg) {
				success = m_thread == Thread.CurrentThread;
			}
		}

#if MONO_FEATURE_MULTIPLE_APPDOMAINS
		[Test]
		[Category ("NotDotNet")]
		public void CurrentThread_Domains ()
		{
			AppDomain ad = AppDomain.CreateDomain ("foo");
			ad.Load (typeof (DomainClass).Assembly.GetName ());
			var o = (DomainClass)ad.CreateInstanceAndUnwrap (typeof (DomainClass).Assembly.FullName, typeof (DomainClass).FullName);
			Assert.IsTrue (o.Run ());
			AppDomain.Unload (ad);
		}
#endif // MONO_FEATURE_MULTIPLE_APPDOMAINS

		[Test]
		public void SetNameInThreadPoolThread ()
		{
			Task t = Task.Run (delegate () {
				Thread.CurrentThread.Name = "ThreadName1";
				Assert.AreEqual (Thread.CurrentThread.Name, "ThreadName1", "#1");

				try {
					Thread.CurrentThread.Name = "ThreadName2";
					Assert.Fail ("#2");
				} catch (InvalidOperationException) {
				}
			});

			t.Wait ();
		}

		void CheckIsRunning (string s, Thread t)
		{
			int c = counter;
			Thread.Sleep (100);
			Assert.IsTrue (counter > c, s);
		}
		
		void CheckIsNotRunning (string s, Thread t)
		{
			int c = counter;
			Thread.Sleep (100);
			Assert.AreEqual (counter, c, s);
		}
		
		void WaitSuspended (string s, Thread t)
		{
			int n=0;
			ThreadState state = t.ThreadState;
			while ((state & ThreadState.Suspended) == 0) {
				Assert.IsTrue ((state & ThreadState.SuspendRequested) != 0, s + ": expected SuspendRequested state");
				Thread.Sleep (10);
				n++;
				Assert.IsTrue (n < 100, s + ": failed to suspend");
				state = t.ThreadState;
			}
			Assert.IsTrue ((state & ThreadState.SuspendRequested) == 0, s + ": SuspendRequested state not expected");
		}
		
		void WaitResumed (string s, Thread t)
		{
			int n=0;
			while ((t.ThreadState & ThreadState.Suspended) != 0) {
				Thread.Sleep (10);
				n++;
				Assert.IsTrue (n < 100, s + ": failed to resume");
			}
		}
		
		public void DoCount ()
		{
			while (true) {
				counter++;
				Thread.Sleep (1);
			}
		}
	}

	[TestFixture]
	public class ThreadStateTest {
		void Start ()
		{
		}

		[Test] // bug #81720
		[Category ("MultiThreaded")]
		public void IsBackGround ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			Assert.AreEqual (ThreadState.Unstarted, t1.ThreadState, "#A1");
			Assert.IsFalse (t1.IsBackground, "#A2");
			t1.Start ();
			t1.Join ();
			Assert.AreEqual (ThreadState.Stopped, t1.ThreadState, "#A3");

			try {
				bool isBackGround = t1.IsBackground;
				Assert.Fail ("#A4: " + isBackGround.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
				Assert.IsNotNull (ex.Message, "#A7");
			}

			Thread t2 = new Thread (new ThreadStart (Start));
			Assert.AreEqual (ThreadState.Unstarted, t2.ThreadState, "#B1");
			t2.IsBackground = true;
			Assert.AreEqual (ThreadState.Unstarted | ThreadState.Background, t2.ThreadState, "#B2");
			Assert.IsTrue (t2.IsBackground, "#B3");
			t2.Start ();
			t2.Join ();
			Assert.AreEqual (ThreadState.Stopped, t2.ThreadState, "#B4");

			try {
				bool isBackGround = t2.IsBackground;
				Assert.Fail ("#B5: " + isBackGround.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#B6");
				Assert.IsNull (ex.InnerException, "#B7");
				Assert.IsNotNull (ex.Message, "#B8");
			}
		}

		[Test] // bug #60031
		[Category ("MultiThreaded")]
		public void StoppedThreadsThrowThreadStateException ()
		{
			var t = new Thread (() => { });
			t.Start ();
			t.Join ();

			Assert.Throws<ThreadStateException> (() => { var isb = t.IsBackground; }, "IsBackground getter");
			Assert.Throws<ThreadStateException> (() => { var isb = t.ApartmentState; }, "ApartmentState getter");
			Assert.Throws<ThreadStateException> (() => t.ApartmentState = ApartmentState.MTA, "ApartmentState setter");
			Assert.Throws<ThreadStateException> (() => t.IsBackground = false, "IsBackground setter");
			Assert.Throws<ThreadStateException> (() => t.Start (), "Start ()");
#if MONO_FEATURE_THREAD_SUSPEND_RESUME
			Assert.Throws<ThreadStateException> (() => t.Resume (), "Resume ()");
			Assert.Throws<ThreadStateException> (() => t.Suspend (), "Suspend ()");
#endif
			Assert.Throws<ThreadStateException> (() => t.GetApartmentState (), "GetApartmentState ()");
			Assert.Throws<ThreadStateException> (() => t.SetApartmentState (ApartmentState.MTA), "SetApartmentState ()");
			Assert.Throws<ThreadStateException> (() => t.TrySetApartmentState (ApartmentState.MTA), "TrySetApartmentState ()");
		}
	}

	[TestFixture]
	[Serializable]
	public class ThreadTest_ManagedThreadId
	{
		AppDomain ad1;
		AppDomain ad2;
		MBRO mbro = new MBRO ();

		class MBRO : MarshalByRefObject {
			public int id_a1;
			public int id_b1;
			public int id_b2;
			public string ad_a1;
			public string ad_b1;
			public string ad_b2;
			public string message;
		}
#if !MOBILE
		[Test]
		public void ManagedThreadId_AppDomains ()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			ad1 = AppDomain.CreateDomain ("AppDomain 1", currentDomain.Evidence, currentDomain.SetupInformation);
			ad2 = AppDomain.CreateDomain ("AppDomain 2", currentDomain.Evidence, currentDomain.SetupInformation);

			Thread a = new Thread (ThreadA);
			Thread b = new Thread (ThreadB);
			// execute on AppDomain 1 thread A
			// execute on AppDomain 2 thread B
			// execute on AppDomain 1 thread B - must have same ManagedThreadId as Ad 2 on thread B
			a.Start ();
			a.Join ();
			b.Start ();
			b.Join ();

			AppDomain.Unload (ad1);
			AppDomain.Unload (ad2);

			if (mbro.message != null)
				Assert.Fail (mbro.message);

			// Console.WriteLine ("Done id_a1: {0} id_b1: {1} id_b2: {2} ad_a1: {3} ad_b1: {4} ad_b2: {5}", mbro.id_a1, mbro.id_b1, mbro.id_b2, mbro.ad_a1, mbro.ad_b1, mbro.ad_b2);

			Assert.AreEqual ("AppDomain 1", mbro.ad_a1, "Name #1");
			Assert.AreEqual ("AppDomain 1", mbro.ad_b1, "Name #2");
			Assert.AreEqual ("AppDomain 2", mbro.ad_b2, "Name #3");

			Assert.AreNotEqual (mbro.id_a1, mbro.id_b1, "Id #1");
			Assert.AreNotEqual (mbro.id_a1, mbro.id_b2, "Id #2");
			Assert.AreEqual (mbro.id_b1, mbro.id_b2, "Id #3");

			Assert.AreNotEqual (mbro.id_a1, Thread.CurrentThread.ManagedThreadId, "Id #4");
			Assert.AreNotEqual (mbro.id_b1, Thread.CurrentThread.ManagedThreadId, "Id #5");
			Assert.AreNotEqual (mbro.id_b2, Thread.CurrentThread.ManagedThreadId, "Id #6");
			Assert.AreNotEqual (mbro.ad_a1, AppDomain.CurrentDomain.FriendlyName, "Name #4");
			Assert.AreNotEqual (mbro.ad_b1, AppDomain.CurrentDomain.FriendlyName, "Name #5");
			Assert.AreNotEqual (mbro.ad_b2, AppDomain.CurrentDomain.FriendlyName, "Name #6");
		}
#endif
		void A1 ()
		{
			mbro.id_a1 = Thread.CurrentThread.ManagedThreadId;
			mbro.ad_a1 = AppDomain.CurrentDomain.FriendlyName;
		}
		
		void B2 ()
		{
			mbro.id_b2 = Thread.CurrentThread.ManagedThreadId;
			mbro.ad_b2 = AppDomain.CurrentDomain.FriendlyName;
		}

		void B1 ()
		{
			mbro.id_b1 = Thread.CurrentThread.ManagedThreadId;
			mbro.ad_b1 = AppDomain.CurrentDomain.FriendlyName;
		}

		void ThreadA (object obj)
		{
			// Console.WriteLine ("ThreadA");
			try {
				ad1.DoCallBack (A1);
			} catch (Exception ex) {
				mbro.message = string.Format ("ThreadA exception: {0}", ex);
			}
			// Console.WriteLine ("ThreadA Done");
		}

		void ThreadB (object obj)
		{
			// Console.WriteLine ("ThreadB");
			try {
				ad2.DoCallBack (B2);
				ad1.DoCallBack (B1);
			} catch (Exception ex) {
				mbro.message = string.Format ("ThreadB exception: {0}", ex);
			}
			// Console.WriteLine ("ThreadB Done");
		}
	}

	[TestFixture]
	public class ThreadApartmentTest
	{
		void Start ()
		{
		}

		[Test] // bug #81658
		[Category ("MultiThreaded")]
		public void ApartmentState_StoppedThread ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.Start ();
			t1.Join ();
			try {
				ApartmentState state = t1.ApartmentState;
				Assert.Fail ("#A1: " + state.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Thread t2 = new Thread (new ThreadStart (Start));
			t2.IsBackground = true;
			t2.Start ();
			t2.Join ();
			try {
				ApartmentState state = t2.ApartmentState;
				Assert.Fail ("#B1: " + state.ToString ());
			} catch (ThreadStateException ex) {
				Assert.AreEqual (typeof (ThreadStateException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void ApartmentState_BackGround ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.IsBackground = true;
			Assert.AreEqual (ApartmentState.Unknown, t1.ApartmentState, "#1");
			t1.ApartmentState = ApartmentState.STA;
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "#2");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestApartmentState ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			Thread t2 = new Thread (new ThreadStart (Start));
			Thread t3 = new Thread (new ThreadStart (Start));

			Assert.AreEqual (ApartmentState.Unknown, t1.ApartmentState, "Thread1 Default");
			Assert.AreEqual (ApartmentState.Unknown, t2.ApartmentState, "Thread2 Default");
			Assert.AreEqual (ApartmentState.Unknown, t3.ApartmentState, "Thread3 Default");

			t1.ApartmentState = ApartmentState.STA;
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set Once");
			t1.ApartmentState = ApartmentState.MTA;
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set Twice");

			t2.ApartmentState = ApartmentState.MTA;
			Assert.AreEqual (ApartmentState.MTA, t2.ApartmentState, "Thread2 Set Once");
			t2.ApartmentState = ApartmentState.STA;
			Assert.AreEqual (ApartmentState.MTA, t2.ApartmentState, "Thread2 Set Twice");

			bool exception_occured = false;
			try {
				t3.ApartmentState = ApartmentState.Unknown;
			}
			catch (Exception) {
				exception_occured = true;
			}
			Assert.AreEqual (ApartmentState.Unknown, t3.ApartmentState, "Thread3 Set Invalid");
			Assert.IsFalse (exception_occured, "Thread3 Set Invalid Exception Occured");

			t1.Start ();
			exception_occured = false;
			try {
				t1.ApartmentState = ApartmentState.STA;
			}
			catch (Exception) {
				exception_occured = true;
			}
			Assert.IsTrue (exception_occured, "Thread1 Started Invalid Exception Occured");
		}

		[Test]
		public void TestSetApartmentStateSameState ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.SetApartmentState (ApartmentState.STA);
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set Once");

			t1.SetApartmentState (ApartmentState.STA);
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set twice");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestSetApartmentStateDiffState ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.SetApartmentState (ApartmentState.STA);
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "Thread1 Set Once");

			t1.SetApartmentState (ApartmentState.MTA);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestTrySetApartmentState ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.SetApartmentState (ApartmentState.STA);
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "#1");

			bool result = t1.TrySetApartmentState (ApartmentState.MTA);
			Assert.IsFalse (result, "#2");

			result = t1.TrySetApartmentState (ApartmentState.STA);
			Assert.IsTrue (result, "#3");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestTrySetApartmentStateRunning ()
		{
			Thread t1 = new Thread (new ThreadStart (Start));
			t1.SetApartmentState (ApartmentState.STA);
			Assert.AreEqual (ApartmentState.STA, t1.ApartmentState, "#1");

			t1.Start ();

			try {
				t1.TrySetApartmentState (ApartmentState.STA);
				Assert.Fail ("#2");
			} catch (ThreadStateException) {
			}

			t1.Join ();
		}

		[Test]
		public void Volatile () {
			double v3 = 55667;
			Thread.VolatileWrite (ref v3, double.MaxValue);
			Assert.AreEqual (v3, double.MaxValue);

			float v4 = 1;
			Thread.VolatileWrite (ref v4, float.MaxValue);
			Assert.AreEqual (v4, float.MaxValue);
		}

		[Test]
		public void Culture ()
		{
			Assert.IsNotNull (Thread.CurrentThread.CurrentCulture, "CurrentCulture");
			Assert.IsNotNull (Thread.CurrentThread.CurrentUICulture, "CurrentUICulture");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void ThreadStartSimple ()
		{
			int i = 0;
			Thread t = new Thread (delegate () {
				// ensure the NSAutoreleasePool works
				i++;
			});
			t.Start ();
			t.Join ();
			Assert.AreEqual (1, i, "ThreadStart");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void ParametrizedThreadStart ()
		{
			int i = 0;
			object arg = null;
			Thread t = new Thread (delegate (object obj) {
				// ensure the NSAutoreleasePool works
				i++;
				arg = obj;
			});
			t.Start (this);
			t.Join ();

			Assert.AreEqual (1, i, "ParametrizedThreadStart");
			Assert.AreEqual (this, arg, "obj");	
		}		

		[Test]
		public void SetNameTpThread () {
			ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));
		}

		static void ThreadProc(Object stateInfo) {
			Thread.CurrentThread.Name = "My Worker";
		}

		[Test]
		public void GetStackTraces () {
			var m = typeof (Thread).GetMethod ("Mono_GetStackTraces", BindingFlags.NonPublic|BindingFlags.Static);
			if (m != null) {
				var res = (Dictionary<Thread,SD.StackTrace>)typeof (Thread).GetMethod ("Mono_GetStackTraces", BindingFlags.NonPublic|BindingFlags.Static).Invoke (null, null);
				foreach (var t in res.Keys) {
					var st = res [t].ToString ();
				}
			}
		}
	}

	public class TestUtil
	{
		public static void WaitForNotAlive (Thread t, string s)
		{
			WhileAlive (t, true, s);
		}
		
		public static void WaitForAlive (Thread t, string s)
		{
			WhileAlive (t, false, s);
		}
		
		public static bool WaitForAliveOrStop (Thread t, string s)
		{
			return WhileAliveOrStop (t, false, s);
		}
		
		public static void WhileAlive (Thread t, bool alive, string s)
		{
			var sw = SD.Stopwatch.StartNew ();
			while (t.IsAlive == alive) {
				if (sw.Elapsed.TotalSeconds > 10) {
					if (alive) Assert.Fail ("Timeout while waiting for not alive state. " + s);
					else Assert.Fail ("Timeout while waiting for alive state. " + s);
				}
			}
		}

		public static bool WhileAliveOrStop (Thread t, bool alive, string s)
		{
			var sw = SD.Stopwatch.StartNew ();
			while (t.IsAlive == alive) {
				if (t.ThreadState == ThreadState.Stopped)
					return false;

				if (sw.Elapsed.TotalSeconds > 10) {
					if (alive) Assert.Fail ("Timeout while waiting for not alive state. " + s);
					else Assert.Fail ("Timeout while waiting for alive state. " + s);
				}
			}

			return true;
		}
	}
}
