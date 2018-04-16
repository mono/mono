// MutexTest.cs - NUnit Test Cases for System.Threading.Mutex
//
// Eduardo Garcia Cebollero <kiwnix@yahoo.es>
//
// (C) Eduardo Garcia Cebollero
// 

using System;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class MutexTest
	{
		//Auxiliary Classes (Future Threads)
		private class ConcClass
		{
			public int id;
			public Mutex mut;
			public bool abandoned_exception;
			public ConcClass(int id,Mutex mut)
			{
				this.id = id;
				this.mut = mut;
			}
			public void Wait()
			{
				mut.WaitOne();
			}
			public void Signal()
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
				this.Signal();
			}


			public void Loop()
			{
				while (this.marker<100)
				{
					this.Wait();
					this.marker++;
					this.Signal();
				}
			}

			public void WaitAndForget()
			{
				try {
					this.Wait();
				} catch (AbandonedMutexException) {
					this.abandoned_exception = true;
				}

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

		[Test]
		public void TestCtor1()
		{
			Mutex Sem = new Mutex();
		}

// These tests produce mutex release errors
/**
		[Test]
		public void TestCtorDefaultValue()
		{
			Mutex Sem = new Mutex();
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WithoutWait));
			thread1.Start();
			while(thread1.IsAlive);
			Assert.AreEqual(class1.id,class1.marker);
		}

		[Test]
		public void TestCtorCtor2()
		{
			Mutex Sem = new Mutex(false);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WithoutWait));
			thread1.Start();
			while(thread1.IsAlive);
			Assert.AreEqual(class1.id,class1.marker);
		}
	
		[Test]
		public void TestCtorCtor3()
		{
			Mutex Sem = new Mutex(true);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WithoutWait));
			thread1.Start();
			while(thread1.IsAlive);
			Assert.AreEqual(class1.id,class1.marker);
		}
*/

		[Test]
		[Category ("MultiThreaded")]
		public void TestWaitAndSignal1()
		{
			Mutex Sem = new Mutex (false);
			ConcClassLoop class1 = new ConcClassLoop (1, Sem);
			Thread thread1 = new Thread (new ThreadStart (class1.Loop));
			try {
				thread1.Start ();
				TestUtil.WaitForNotAlive (thread1, "");
				Assert.AreEqual (100, class1.marker);
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				thread1.Abort ();
#else
				thread1.Interrupt ();
#endif
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestWaitAndForget1()
		{
			Mutex Sem = new Mutex(false);
			ConcClassLoop class1 = new ConcClassLoop(1,Sem);
			ConcClassLoop class2 = new ConcClassLoop(2,Sem);
			Thread thread1 = new Thread(new ThreadStart(class1.WaitAndForget));
			Thread thread2 = new Thread(new ThreadStart(class2.WaitAndForget));
			
			try {
				thread1.Start();
				TestUtil.WaitForNotAlive (thread1, "t1");
				Assert.IsFalse (class1.abandoned_exception, "e1");
	
				thread2.Start();
				TestUtil.WaitForNotAlive (thread2, "t2");
				Assert.IsTrue (class2.abandoned_exception, "e2");
			
				Assert.AreEqual (class2.id, class2.marker);
			} finally {
#if MONO_FEATURE_THREAD_ABORT
				thread1.Abort ();
				thread2.Abort ();
#else
				thread1.Interrupt ();
				thread2.Interrupt ();
#endif
			}
		}

		[Test]
		public void TestHandle()
		{
			Mutex Sem = new Mutex();
			IntPtr Handle = Sem.Handle;
		}

		[Test] // bug #79358
		public void DoubleRelease ()
		{
			Mutex mutex = new Mutex ();
			mutex.WaitOne ();
			mutex.ReleaseMutex ();

			try {
				mutex.ReleaseMutex ();
				Assert.Fail ("#1");
			} catch (ApplicationException ex) {
				Assert.AreEqual (typeof (ApplicationException), ex.GetType (), "#2");
			}
		}
	}
}
