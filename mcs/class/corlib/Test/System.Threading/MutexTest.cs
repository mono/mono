// MutexTest.cs - NUnit Test Cases for System.Threading.Mutex
//
// Eduardo Garcia Cebollero <kiwnix@yahoo.es>
//
// (C) Eduardo Garcia Cebollero
// 

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading
{

	public class MutexTest : TestCase 
	{
	
		//Auxiliary Classes (Future Threads)
		private class ConcClass
		{
			public int id;
			public Mutex mut;
			public ConcClass(int id,Mutex mut)
			{
				this.id = id;
				this.mut = mut;
			}
			public void wait()
			{
				mut.WaitOne();
			}
			public void signal()
			{
				mut.ReleaseMutex();
			}
		}
		private class ConcClassLoop: ConcClass
		{
			public int marker;

			public ConcClassLoop(int id,Mutex mut) : 
				base(id,mut) 
				{
					this.marker = 0;
				}
			
			public void WithoutWait()
			{
				this.marker = this.id;
				this.signal();
			}


			public void Loop()
			{
				while (this.marker<100)
				{
					this.wait();
					this.marker++;
					this.signal();
				}
			}

			public void WaitAndForget()
			{
				this.wait();
				this.marker = id;
			}
			public void WaitAndWait()
			{
				mut.WaitOne();
				this.marker = this.id;
				mut.WaitOne();
				this.marker = this.id+1;
			}
		}

		protected override void SetUp() {}

		protected override void TearDown() {}

		public void TestCtor1()
		{
			try
			{
				Mutex Sem = new Mutex();
			}
			catch (Exception e)
			{
				Fail("#01 Error Creating The Simple Mutex:" + e.ToString());
			}
		}

// These tests produce mutex release errors
/*
		public void TestCtorDefaultValue()
		{
			Mutex Sem = new Mutex();
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WithoutWait));
			thread1.Start();
			while(thread1.IsAlive);
			AssertEquals("#02 The default value of The mutex wrong set:",class1.id,class1.marker);
		}

		public void TestCtorCtor2()
		{
			Mutex Sem = new Mutex(false);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WithoutWait));
			thread1.Start();
			while(thread1.IsAlive);
			AssertEquals("#03 The value of The mutex wrong set:",class1.id,class1.marker);
		}
		
		public void TestCtorCtor3()
		{
			Mutex Sem = new Mutex(true);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WithoutWait));
			thread1.Start();
			while(thread1.IsAlive);
			AssertEquals("#04 The default value of The mutex wrong set:",class1.id,class1.marker);
		}

*/
		public void TestWaitAndSignal1()
		{
			Mutex Sem = new Mutex(false);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.Loop));
			try {
				thread1.Start();
				TestUtil.WaitForNotAlive (thread1, "");
				AssertEquals("#41 Mutex Worked InCorrecly:",100,class1.marker);
			}
			finally {
				thread1.Abort ();
			}
		}

		public void TestWaitAndFoget1()
		{
			Mutex Sem = new Mutex(false);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			ConcClassLoop class2 = new ConcClassLoop(2,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WaitAndForget));
			Thread thread2 = new Thread(new ThreadStart(class2.WaitAndForget));
			
			try {
				thread1.Start();
				TestUtil.WaitForNotAlive (thread1, "t1");
	
				thread2.Start();
				TestUtil.WaitForNotAlive (thread2, "t2");
			
				AssertEquals("#51 The Mutex Has been Kept after end of the thread:", class2.id,class2.marker);
			}
			finally {
				thread1.Abort ();
				thread2.Abort ();
			}
		}

		public void TestHandle()
		{
			Mutex Sem = new Mutex();
			try
			{
				IntPtr Handle = Sem.Handle;
			}
			catch (Exception e)
			{
				Fail("#61 Unexpected Exception accessing Sem.Handle:" + e.ToString());
			}
		}
	}
}
