//
// ReaderWriterLockTest.cs - NUnit Test Cases for System.Threading.ReaderWriterLock
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class ReaderWriterLockTest
	{
		ReaderWriterLock rwlock;
		
		class ThreadRunner
		{
			public ThreadStart SecondaryThread;
			public Exception ResultException;
			public Thread RunningThread;
			
			public void Run ()
			{
				try
				{
					SecondaryThread();
				}
				catch (Exception ex)
				{
					ResultException = ex;
				}
			}
			
			public void Join ()
			{
				RunningThread.Join (5000);
				if (ResultException != null) throw ResultException;
			}
		}
		
		void RunThread (ThreadStart ts)
		{
			ThreadRunner tr = StartThread (ts);
			tr.Join ();
		}
		
		ThreadRunner StartThread (ThreadStart ts)
		{
			ThreadRunner tr = new ThreadRunner();
			tr.SecondaryThread = ts;
			Thread t = new Thread (new ThreadStart (tr.Run));
			tr.RunningThread = t;
			t.Start ();
			return tr;
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void TestIsReaderLockHeld ()
		{
			rwlock = new ReaderWriterLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld, "#1");
			rwlock.AcquireReaderLock (500);
			Assert.IsTrue (rwlock.IsReaderLockHeld, "#1");
			RunThread (new ThreadStart (IsReaderLockHeld_2));
			rwlock.ReleaseReaderLock ();
		}
		
		private void IsReaderLockHeld_2 ()
		{
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void TestIsWriterLockHeld ()
		{
			rwlock = new ReaderWriterLock ();
			Assert.IsTrue (!rwlock.IsWriterLockHeld, "#1");
			rwlock.AcquireWriterLock (500);
			Assert.IsTrue (rwlock.IsWriterLockHeld, "#2");
			RunThread (new ThreadStart (IsWriterLockHeld_2));
			rwlock.ReleaseWriterLock ();
		}
		
		private void IsWriterLockHeld_2 ()
		{
			Assert.IsTrue (!rwlock.IsWriterLockHeld);
		}
				
		[Test]
		[Category ("MultiThreaded")]
		public void TestAcquireLocks ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireReaderLock (500);
			rwlock.AcquireReaderLock (500);
			rwlock.ReleaseReaderLock ();
				Assert.IsTrue (rwlock.IsReaderLockHeld, "#1");			
			RunThread (new ThreadStart (AcquireLock_readerWorks));
			Assert.IsTrue (rwlock.IsReaderLockHeld);
			
			RunThread (new ThreadStart (AcquireLock_writerFails));
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			
			RunThread (new ThreadStart (AcquireLock_writerWorks));
			
			rwlock.AcquireWriterLock (200);
			RunThread (new ThreadStart (AcquireLock_writerFails));
			RunThread (new ThreadStart (AcquireLock_readerFails));
			rwlock.ReleaseWriterLock ();
		}
		
		void AcquireLock_readerWorks ()
		{
			rwlock.AcquireReaderLock (200);
			rwlock.AcquireReaderLock (200);
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
		}
		
		void AcquireLock_writerFails ()
		{
			try
			{
				rwlock.AcquireWriterLock (200);
				rwlock.ReleaseWriterLock ();
				throw new Exception ("Should not get writer lock");
			}
			catch (Exception)
			{
			}
		}
		
		void AcquireLock_writerWorks ()
		{
			rwlock.AcquireWriterLock (200);
			rwlock.ReleaseWriterLock ();
		}
		
		void AcquireLock_readerFails ()
		{
			try
			{
				rwlock.AcquireReaderLock (200);
				rwlock.ReleaseReaderLock ();
				throw new Exception ("Should not get reader lock");
			}
			catch (Exception)
			{
			}
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void TestReleaseRestoreReaderLock ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireReaderLock (500);
			rwlock.AcquireReaderLock (500);
			Assert.IsTrue (rwlock.IsReaderLockHeld);
			
			LockCookie co = rwlock.ReleaseLock ();
			RunThread (new ThreadStart (AcquireLock_writerWorks));
			
			rwlock.RestoreLock (ref co);
			RunThread (new ThreadStart (AcquireLock_writerFails));
			
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
		}
		
		[Test]
			[Category ("MultiThreaded")]
		public void TestReleaseRestoreWriterLock ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireWriterLock (500);
			rwlock.AcquireWriterLock (500);
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			
			LockCookie co = rwlock.ReleaseLock ();
			RunThread (new ThreadStart (AcquireLock_readerWorks));
			
			rwlock.RestoreLock (ref co);
			RunThread (new ThreadStart (AcquireLock_readerFails));
			
			rwlock.ReleaseWriterLock ();
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			rwlock.ReleaseWriterLock ();
			Assert.IsTrue (!rwlock.IsWriterLockHeld);
		}
		
		[Test]
			[Category ("MultiThreaded")]
		public void TestUpgradeDowngradeLock ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireReaderLock (200);
			rwlock.AcquireReaderLock (200);
			
			LockCookie co = rwlock.UpgradeToWriterLock (200);
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			RunThread (new ThreadStart (AcquireLock_writerFails));
			
			rwlock.DowngradeFromWriterLock (ref co);
			Assert.IsTrue (rwlock.IsReaderLockHeld);
			Assert.IsTrue (!rwlock.IsWriterLockHeld);
			RunThread (new ThreadStart (AcquireLock_readerWorks));
			
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
		}
		
		[Test]
		public void TestReaderInsideWriter ()
		{
			// Reader acquires and releases work like the writer equivalent
			
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireWriterLock (-1);
			rwlock.AcquireReaderLock (-1);
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			rwlock.AcquireReaderLock (-1);
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			rwlock.ReleaseWriterLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			Assert.IsTrue (rwlock.IsWriterLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert.IsTrue (!rwlock.IsReaderLockHeld);
			Assert.IsTrue (!rwlock.IsWriterLockHeld);
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void TestReaderMustWaitWriter ()
		{
			// A thread cannot get the reader lock if there are other threads
			// waiting for the writer lock.
			
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireWriterLock (200);
			
			ThreadRunner tr = StartThread (new ThreadStart (ReaderMustWaitWriter_2));
			Thread.Sleep (200);
			
			RunThread (new ThreadStart (AcquireLock_readerFails));
			
			rwlock.ReleaseReaderLock ();
			tr.Join ();
		}
		
		void ReaderMustWaitWriter_2 ()
		{
			rwlock.AcquireWriterLock (2000);
			rwlock.ReleaseWriterLock ();
		}
		
		[Test]
		public void TestBug_55911 ()
		{
			rwlock = new ReaderWriterLock ();
			
			rwlock.AcquireReaderLock (Timeout.Infinite);
			try {
				LockCookie lc = rwlock.UpgradeToWriterLock (Timeout.Infinite);
			}
			finally { rwlock.ReleaseReaderLock(); }
			
			rwlock.AcquireReaderLock (Timeout.Infinite);
			try {
				LockCookie lc = rwlock.UpgradeToWriterLock (Timeout.Infinite);
			}
			finally { rwlock.ReleaseReaderLock(); }
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void TestBug_55909 ()
		{
			rwlock = new ReaderWriterLock ();
			ThreadRunner tr = StartThread (new ThreadStart(Bug_55909_Thread2));
			Thread.Sleep (200);
			rwlock.AcquireReaderLock (Timeout.Infinite);
			try {
				LockCookie lc = rwlock.UpgradeToWriterLock (Timeout.Infinite);
				Thread.Sleep (500);
			}
			finally { rwlock.ReleaseReaderLock(); }
			
			tr.Join ();
		}
		
		public void Bug_55909_Thread2 ()
		{
			rwlock.AcquireReaderLock(Timeout.Infinite);
			try {
				Thread.Sleep (1000);
				LockCookie lc = rwlock.UpgradeToWriterLock (Timeout.Infinite);
				Thread.Sleep (500);
			}
			finally { rwlock.ReleaseReaderLock(); }
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void TestBug_55909_bis ()
		{
			rwlock = new ReaderWriterLock ();
			ThreadRunner tr1 = StartThread (new ThreadStart(Bug_55909_bis_ReaderWriter));
			Thread.Sleep(100);
			ThreadRunner tr2 = StartThread (new ThreadStart(Bug_55909_bis_Reader));
			Thread.Sleep(100);
			ThreadRunner tr3 = StartThread (new ThreadStart(Bug_55909_bis_Writer));
			Thread.Sleep(100);
			ThreadRunner tr4 = StartThread (new ThreadStart(Bug_55909_bis_Reader));
			tr1.Join ();
			tr2.Join ();
			tr3.Join ();
			tr4.Join ();
		}
		
		void Bug_55909_bis_Reader ()
		{
			rwlock.AcquireReaderLock(-1);
			Thread.Sleep(2000);
			rwlock.ReleaseReaderLock();
		}

		void Bug_55909_bis_ReaderWriter ()
		{
			rwlock.AcquireReaderLock(-1);
			LockCookie lc = rwlock.UpgradeToWriterLock(-1);
			Thread.Sleep(1000);
			rwlock.DowngradeFromWriterLock(ref lc);
			rwlock.ReleaseReaderLock();
		}

		void Bug_55909_bis_Writer ()
		{
			rwlock.AcquireWriterLock(-1);
			rwlock.ReleaseWriterLock();
		}
		

		[Test]
		public void TestBug_475124 ()
		{
			LockCookie lc1, lc2;

			rwlock = new ReaderWriterLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "A1");
			Assert.IsFalse (rwlock.IsWriterLockHeld, "A2");

			rwlock.AcquireReaderLock (Timeout.Infinite);

			Assert.IsTrue (rwlock.IsReaderLockHeld, "B1");
			Assert.IsFalse (rwlock.IsWriterLockHeld, "B2");

			lc1 = rwlock.UpgradeToWriterLock (Timeout.Infinite);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "C1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "C2");

			rwlock.AcquireReaderLock (Timeout.Infinite);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "D1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "D2");

			lc2 = rwlock.UpgradeToWriterLock (Timeout.Infinite);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "E1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "E2");

			rwlock.DowngradeFromWriterLock (ref lc2);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "F1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "F2");

			rwlock.ReleaseReaderLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "G1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "G2");

			rwlock.DowngradeFromWriterLock (ref lc1);

			Assert.IsTrue (rwlock.IsReaderLockHeld, "H1");
			Assert.IsFalse (rwlock.IsWriterLockHeld, "H2");

			rwlock.ReleaseReaderLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "I1");
			Assert.IsFalse (rwlock.IsWriterLockHeld, "I2");
		}

		// this tests how downgrade works when multiple writer locks
		// are acquired - as long as the LockCookie referes to an
		// upgrade where there was already a writer lock, downgrade
		// behaves like a non-blocking ReleaseWriterLock
		[Test]
		public void DowngradeTest ()
		{
			LockCookie lc1, lc2, lc3, lc4;

			rwlock = new ReaderWriterLock ();

			rwlock.AcquireReaderLock (Timeout.Infinite);
			lc1 = rwlock.UpgradeToWriterLock (Timeout.Infinite);
			rwlock.AcquireReaderLock (Timeout.Infinite);
			lc2 = rwlock.UpgradeToWriterLock (Timeout.Infinite);
			rwlock.AcquireReaderLock (Timeout.Infinite);
			lc3 = rwlock.UpgradeToWriterLock (Timeout.Infinite);
			rwlock.AcquireReaderLock (Timeout.Infinite);
			lc4 = rwlock.UpgradeToWriterLock (Timeout.Infinite);

			rwlock.DowngradeFromWriterLock (ref lc2);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "A1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "A2");

			rwlock.ReleaseReaderLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "B1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "B2");

			rwlock.DowngradeFromWriterLock (ref lc4);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "C1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "C2");

			rwlock.ReleaseReaderLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "D1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "D2");

			rwlock.DowngradeFromWriterLock (ref lc3);

			Assert.IsFalse (rwlock.IsReaderLockHeld, "E1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "E2");

			rwlock.ReleaseReaderLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "F1");
			Assert.IsTrue (rwlock.IsWriterLockHeld, "F2");

			rwlock.DowngradeFromWriterLock (ref lc1);

			Assert.IsTrue (rwlock.IsReaderLockHeld, "G1");
			Assert.IsFalse (rwlock.IsWriterLockHeld, "G2");

			rwlock.ReleaseReaderLock ();

			Assert.IsFalse (rwlock.IsReaderLockHeld, "H1");
			Assert.IsFalse (rwlock.IsWriterLockHeld, "H2");
		}
	}
}
