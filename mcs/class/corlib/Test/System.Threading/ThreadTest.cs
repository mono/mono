// ThreadTest.cs - NUnit Test Cases for the System.Threading.Thread class
//
// Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//
// (C) Eduardo Garcia Cebollero.
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading
{
	public class ThreadTest : TestCase
	{
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
			test1.thread1.Start();
			test1.thread1.Abort();
			while(test1.thread1.IsAlive);
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
			AssertEquals("#102 Wrong Thread State", ThreadState.Running | ThreadState.Unstarted ,TestThread.ThreadState);
			TestThread.Abort();
			while(TestThread.IsAlive);
			AssertEquals("#103 Wrong Thread State",ThreadState.Aborted,TestThread.ThreadState);
		} 
	}

}
		


