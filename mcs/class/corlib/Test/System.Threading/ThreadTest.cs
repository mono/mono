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

using NUnit.Framework;
using System;
using System.Security.Principal;
using System.Threading;

namespace MonoTests.System.Threading {

	public class ThreadedPrincipalTest : Assertion {

		public static void NoPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.NoPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			AssertNull ("Thread.CurrentPrincipal-1", p);

			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			AssertNotNull ("Thread.CurrentPrincipal-2", Thread.CurrentPrincipal);

			Thread.CurrentPrincipal = null;
			AssertNull ("Thread.CurrentPrincipal-3", Thread.CurrentPrincipal);
			// in this case we can return to null
		}

		public static void UnauthenticatedPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.UnauthenticatedPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			AssertNotNull ("Thread.CurrentPrincipal", p);
			Assert ("Type", (p is GenericPrincipal));
			AssertEquals ("Name", String.Empty, p.Identity.Name);
			AssertEquals ("AuthenticationType", String.Empty, p.Identity.AuthenticationType);
			Assert ("IsAuthenticated", !p.Identity.IsAuthenticated);

			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			AssertNotNull ("Thread.CurrentPrincipal-2", Thread.CurrentPrincipal);

			Thread.CurrentPrincipal = null;
			AssertNotNull ("Thread.CurrentPrincipal-3", Thread.CurrentPrincipal);
			// in this case we can't return to null
		}

		public static void WindowsPrincipal () 
		{
			AppDomain.CurrentDomain.SetPrincipalPolicy (PrincipalPolicy.WindowsPrincipal);
			IPrincipal p = Thread.CurrentPrincipal;
			AssertNotNull ("Thread.CurrentPrincipal", p);
			Assert ("Type", (p is WindowsPrincipal));
			AssertNotNull ("Name", p.Identity.Name);
			AssertNotNull ("AuthenticationType", p.Identity.AuthenticationType);
			Assert ("IsAuthenticated", p.Identity.IsAuthenticated);

			// note: we can switch from a WindowsPrincipal to a GenericPrincipal
			Thread.CurrentPrincipal = new GenericPrincipal (new GenericIdentity ("mono"), null);
			AssertNotNull ("Thread.CurrentPrincipal-2", Thread.CurrentPrincipal);

			Thread.CurrentPrincipal = null;
			AssertNotNull ("Thread.CurrentPrincipal-3", Thread.CurrentPrincipal);
			// in this case we can't return to null
		}
	}

	[TestFixture]
	public class ThreadTest : Assertion {

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
				sub_thread.Abort();
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
				thread1.Abort();
				TestUtil.WaitForNotAlive (thread1, "wait3");
				T1ON = false;
				thread2.Abort();
				TestUtil.WaitForNotAlive (thread2, "wait4");
				T2ON = false;
			}
			
			public void TestMethod2()
			{
				thread1.Start();
				thread1.Join();
			}
		}

		public void TestCtor1()
		{			
			C1Test test1 = new C1Test();
			try
			{
				Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			}
			catch (Exception e)
			{
				Fail ("#01 Unexpected Exception Thrown: " + e.ToString ());
			}
		}

		[Category("NotDotNet")]
		public void TestStart()
		{
		{
			C1Test test1 = new C1Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try
			{
				TestThread.Start();
			}
			catch (Exception e)
			{
				Fail ("#12 Unexpected Exception Thrown: " + e.ToString ());
			}
			TestThread.Join();
			AssertEquals("#13 Thread Not started: ", 10,test1.cnt);
		}
		{
			bool errorThrown = false;
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			TestThread.Abort();
			try
			{
				TestThread.Start();
			}
			catch(ThreadStateException)
			{
				errorThrown = true;
			}
			Assert ("#14 no ThreadStateException trown", errorThrown);
		}
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			while(!test1.run);
			bool started = (TestThread.ThreadState == ThreadState.Running);
			AssertEquals("#15 Thread Is not in the correct state: ", started , test1.run);	
			TestThread.Abort();
		}
		}

		public void TestApartment()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			ApartmentState before = TestThread.ApartmentState;
			TestThread.Start();
			TestUtil.WaitForAlive (TestThread, "wait5");
			ApartmentState after = TestThread.ApartmentState;
			TestThread.Abort();
			AssertEquals("#21 Apartment State Changed when not needed",before,after);
		}

		public void TestApartmentState()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			ApartmentState before = TestThread.ApartmentState;
			TestThread.Start();
			TestUtil.WaitForAlive (TestThread, "wait6");
			ApartmentState after = TestThread.ApartmentState;
			TestThread.Abort();
			AssertEquals("#31 Apartment State Changed when not needed: ",before,after);
		}

		public void TestPriority1()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Priority=ThreadPriority.BelowNormal;
				ThreadPriority after = TestThread.Priority;
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait7");
				ThreadPriority before = TestThread.Priority;
				AssertEquals("#41 Unexpected Priority Change: ",before,after);
			}
			finally {
				TestThread.Abort();
			}
		}

		public void TestPriority2()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				AssertEquals("#42 Incorrect Priority in New thread: ",ThreadPriority.Normal, TestThread.Priority);
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait8");
				AssertEquals("#43 Incorrect Priority in Started thread: ",ThreadPriority.Normal, TestThread.Priority);
			}
			finally {
				TestThread.Abort();
			}
			AssertEquals("#44 Incorrect Priority in Aborted thread: ",ThreadPriority.Normal, TestThread.Priority);
		}

		public void TestPriority3()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestThread.Priority = ThreadPriority.Lowest;
				AssertEquals("#45A Incorrect Priority:",ThreadPriority.Lowest,TestThread.Priority);
				TestThread.Priority = ThreadPriority.BelowNormal;
				AssertEquals("#45B Incorrect Priority:",ThreadPriority.BelowNormal,TestThread.Priority);
				TestThread.Priority = ThreadPriority.Normal;
				AssertEquals("#45C Incorrect Priority:",ThreadPriority.Normal,TestThread.Priority);
				TestThread.Priority = ThreadPriority.AboveNormal;
				AssertEquals("#45D Incorrect Priority:",ThreadPriority.AboveNormal,TestThread.Priority);
				TestThread.Priority = ThreadPriority.Highest;
				AssertEquals("#45E Incorrect Priority:",ThreadPriority.Highest,TestThread.Priority);
			}
			finally {
				TestThread.Abort();
			}
		}


		public void TestIsBackground1()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait9");
				bool state = TestThread.IsBackground;
				Assert("#51 IsBackground not set at the default state: ",!(state));
			}
			finally {
				TestThread.Abort();
			}
		}

		public void TestIsBackground2()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.IsBackground = true;
			try {
				TestThread.Start();
			}
			finally {
				TestThread.Abort();
			}
			Assert("#52 Is Background Changed ot Start ",TestThread.IsBackground);
		}


		public void TestName()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait10");
				string name = TestThread.Name;
				AssertEquals("#61 Name set when mustn't be set: ", name, (string)null);
				string newname = "Testing....";
				TestThread.Name = newname;
				AssertEquals("#62 Name not set when must be set: ",TestThread.Name,newname);
			}
			finally {
				TestThread.Abort();
			}
		}

		[Category("NotDotNet")]
		public void TestNestedThreads1()
		{
			C3Test  test1 = new C3Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try {
				TestThread.Start();
				TestUtil.WaitForAlive (TestThread, "wait11");
			}
			catch(Exception e) {
				Fail("#71 Unexpected Exception" + e.Message);
			}
			finally {
				TestThread.Abort();
			}
		}

		public void TestNestedThreads2()
		{
			C4Test test1 = new C4Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try {
				TestThread.Start();
			}
			catch(Exception e) {
				Fail("#81 Unexpected Exception" + e.ToString());
			}
			finally {
				TestThread.Abort();
			}
		}

		public void TestJoin1()
		{
			C1Test test1 = new C1Test();
			C1Test test2 = new C1Test();
			Thread thread1 = new Thread(new ThreadStart(test1.TestMethod));
			Thread thread2 = new Thread(new ThreadStart(test1.TestMethod2));
			try
			{
				thread1.Start();
				thread2.Start();
				thread2.Join();
			}
			catch(Exception e)
			{
				Fail("#91 Unexpected Exception " + e.ToString());
			}
			finally
			{
				thread1.Abort();
				thread2.Abort();
			}
		}
		
		public void TestThreadState()
		{
			//TODO: Test The rest of the possible transitions
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			AssertEquals("#101 Wrong Thread State",ThreadState.Unstarted,TestThread.ThreadState);
			try {
				TestThread.Start();
				//while(!TestThread.IsAlive); //In the MS Documentation this is not necessary
											  //but in the MS SDK it is
				Assert("#102 Wrong Thread State: " + TestThread.ThreadState.ToString(), TestThread.ThreadState == ThreadState.Running || (TestThread.ThreadState & ThreadState.Unstarted) != 0);
			}
			finally {
				TestThread.Abort();
			}
			
			TestUtil.WaitForNotAlive (TestThread, "wait12");
			// Docs say state will be Stopped, but Aborted happens sometimes (?)
			Assert("#103 Wrong Thread State: " + TestThread.ThreadState.ToString(), (ThreadState.Stopped & TestThread.ThreadState) != 0 
				|| (ThreadState.Aborted & TestThread.ThreadState) != 0);
		} 

		[Test]
		public void CurrentPrincipal_PrincipalPolicy_NoPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.NoPrincipal));
			try {
				t.Start ();
				t.Join ();
			}
			catch {
				t.Abort ();
			}
		}

		[Test]
		public void CurrentPrincipal_PrincipalPolicy_UnauthenticatedPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.UnauthenticatedPrincipal));
			try {
				t.Start ();
				t.Join ();
			}
			catch {
				t.Abort ();
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
			}
			catch {
				t.Abort ();
			}
		}
		
		int counter = 0;
		
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
			
			Assert ("Timeout while waiting for abort", n < 200);
			
			CheckIsNotRunning ("t6", t);
		}		
		
		void CheckIsRunning (string s, Thread t)
		{
			int c = counter;
			Thread.Sleep (100);
			Assert (s, counter > c);
		}
		
		void CheckIsNotRunning (string s, Thread t)
		{
			int c = counter;
			Thread.Sleep (100);
			Assert (s, counter == c);
		}
		
		void WaitSuspended (string s, Thread t)
		{
			int n=0;
			ThreadState state = t.ThreadState;
			while ((state & ThreadState.Suspended) == 0) {
				Assert (s + ": expected SuspendRequested state", (state & ThreadState.SuspendRequested) != 0);
				Thread.Sleep (10);
				n++;
				Assert (s + ": failed to suspend", n < 100);
				state = t.ThreadState;
			}
			Assert (s + ": SuspendRequested state not expected", (state & ThreadState.SuspendRequested) == 0);
		}
		
		void WaitResumed (string s, Thread t)
		{
			int n=0;
			while ((t.ThreadState & ThreadState.Suspended) != 0) {
				Thread.Sleep (10);
				n++;
				Assert (s + ": failed to resume", n < 100);
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
		
		public static void WhileAlive (Thread t, bool alive, string s)
		{
			DateTime ti = DateTime.Now;
			while (t.IsAlive == alive) {
				if ((DateTime.Now - ti).TotalSeconds > 10) {
					if (alive) Assertion.Fail ("Timeout while waiting for not alive state. " + s);
					else Assertion.Fail ("Timeout while waiting for alive state. " + s);
				}
			}
		}
	}
}
