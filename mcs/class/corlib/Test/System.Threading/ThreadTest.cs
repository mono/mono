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
				while (!thread1.IsAlive);
				T1ON = true;
				thread2.Start();
				while (!thread2.IsAlive);
				T2ON = true;
				thread1.Abort();
				while (thread1.IsAlive);
				T1ON = false;
				thread2.Abort();
				while (thread2.IsAlive);
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
			while(!TestThread.IsAlive);
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
			while(!TestThread.IsAlive);
			ApartmentState after = TestThread.ApartmentState;
			TestThread.Abort();
			AssertEquals("#31 Apartment State Changed when not needed: ",before,after);
		}

		public void TestPriority1()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Priority=ThreadPriority.BelowNormal;
			ThreadPriority after = TestThread.Priority;
			TestThread.Start();
			while(!TestThread.IsAlive);
			ThreadPriority before = TestThread.Priority;
			TestThread.Abort();
			AssertEquals("#41 Unexpected Priority Change: ",before,after);
		}

		public void TestPriority2()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			AssertEquals("#42 Incorrect Priority in New thread: ",ThreadPriority.Normal, TestThread.Priority);
			TestThread.Start();
			while(!TestThread.IsAlive);
			AssertEquals("#43 Incorrect Priority in Started thread: ",ThreadPriority.Normal, TestThread.Priority);
			TestThread.Abort();
			AssertEquals("#44 Incorrect Priority in Aborted thread: ",ThreadPriority.Normal, TestThread.Priority);
		}

		public void TestPriority3()
		{
			
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
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
			TestThread.Abort();
		}


		public void TestIsBackground1()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			while(!TestThread.IsAlive);
			bool state = TestThread.IsBackground;
			TestThread.Abort();
			Assert("#51 IsBackground not set at the default state: ",!(state));
		}

		public void TestIsBackground2()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.IsBackground = true;
			TestThread.Start();
			TestThread.Abort();
			Assert("#52 Is Background Changed ot Start ",TestThread.IsBackground);
		}


		public void TestName()
		{
			C2Test test1 = new C2Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod));
			TestThread.Start();
			while(!TestThread.IsAlive);
			string name = TestThread.Name;
			AssertEquals("#61 Name set when mustn't be set: ", name, (string)null);
			string newname = "Testing....";
			TestThread.Name = newname;
			AssertEquals("#62 Name not set when must be set: ",TestThread.Name,newname);
			TestThread.Abort();
		}

		[Category("NotDotNet")]
		public void TestNestedThreads1()
		{
			C3Test  test1 = new C3Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try
			{
				TestThread.Start();
				while(!TestThread.IsAlive);
				TestThread.Abort();
			}
			catch(Exception e)
			{
				Fail("#71 Unexpected Exception" + e.Message);
			}
		}

		public void TestNestedThreads2()
		{
			C4Test test1 = new C4Test();
			Thread TestThread = new Thread(new ThreadStart(test1.TestMethod1));
			try
			{
				TestThread.Start();
				TestThread.Abort();
			}
			catch(Exception e)
			{
				Fail("#81 Unexpected Exception" + e.ToString());
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
			TestThread.Start();
			//while(!TestThread.IsAlive); //In the MS Documentation this is not necessary
										  //but in the MS SDK it is
			Assert("#102 Wrong Thread State: " + TestThread.ThreadState.ToString(), TestThread.ThreadState == ThreadState.Running || (TestThread.ThreadState & ThreadState.Unstarted) != 0);
			TestThread.Abort();
			while(TestThread.IsAlive);
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
			t.Start ();
			t.Join ();
		}

		[Test]
		public void CurrentPrincipal_PrincipalPolicy_UnauthenticatedPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.UnauthenticatedPrincipal));
			t.Start ();
			t.Join ();
		}

		[Test]
		public void CurrentPrincipal_PrincipalPolicy_WindowsPrincipal () 
		{
			// note: switching from PrincipalPolicy won't work inside the same thread
			// because as soon as a Principal object is created the Policy doesn't matter anymore
			Thread t = new Thread (new ThreadStart (ThreadedPrincipalTest.WindowsPrincipal));
			t.Start ();
			t.Join ();
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
			while(t.IsAlive);
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
}
