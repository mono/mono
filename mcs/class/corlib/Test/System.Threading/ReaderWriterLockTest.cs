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
	// DISABLED due to random hangs. Do not renable until you can run this
	// a few thousand times on an SMP box.
	[Category ("NotWorking")]
	public class ReaderWriterLockTest : Assertion
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
		public void TestIsReaderLockHeld ()
		{
			rwlock = new ReaderWriterLock ();
			Assert ("a1", !rwlock.IsReaderLockHeld);
			rwlock.AcquireReaderLock (500);
			Assert ("a2", rwlock.IsReaderLockHeld);
			RunThread (new ThreadStart (IsReaderLockHeld_2));
			rwlock.ReleaseReaderLock ();
		}
		
		private void IsReaderLockHeld_2 ()
		{
			Assert ("a3", !rwlock.IsReaderLockHeld);
		}
		
		[Test]
		public void TestIsWriterLockHeld ()
		{
			rwlock = new ReaderWriterLock ();
			Assert ("a1", !rwlock.IsWriterLockHeld);
			rwlock.AcquireWriterLock (500);
			Assert ("a2", rwlock.IsWriterLockHeld);
			RunThread (new ThreadStart (IsWriterLockHeld_2));
			rwlock.ReleaseWriterLock ();
		}
		
		private void IsWriterLockHeld_2 ()
		{
			Assert ("a3", !rwlock.IsWriterLockHeld);
		}
				
		[Test]
		public void TestAcquireLocks ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireReaderLock (500);
			rwlock.AcquireReaderLock (500);
			rwlock.ReleaseReaderLock ();
			Assert ("a1", rwlock.IsReaderLockHeld);			
			RunThread (new ThreadStart (AcquireLock_readerWorks));
			Assert ("a2", rwlock.IsReaderLockHeld);
			
			RunThread (new ThreadStart (AcquireLock_writerFails));
			rwlock.ReleaseReaderLock ();
			Assert ("a6", !rwlock.IsReaderLockHeld);
			
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
			Assert ("a3", rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert ("a4", !rwlock.IsReaderLockHeld);
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
		public void TestReleaseRestoreReaderLock ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireReaderLock (500);
			rwlock.AcquireReaderLock (500);
			Assert ("r1", rwlock.IsReaderLockHeld);
			
			LockCookie co = rwlock.ReleaseLock ();
			RunThread (new ThreadStart (AcquireLock_writerWorks));
			
			rwlock.RestoreLock (ref co);
			RunThread (new ThreadStart (AcquireLock_writerFails));
			
			rwlock.ReleaseReaderLock ();
			Assert ("r2", rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert ("r3", !rwlock.IsReaderLockHeld);
		}
		
		[Test]
		public void TestReleaseRestoreWriterLock ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireWriterLock (500);
			rwlock.AcquireWriterLock (500);
			Assert ("w1", rwlock.IsWriterLockHeld);
			
			LockCookie co = rwlock.ReleaseLock ();
			RunThread (new ThreadStart (AcquireLock_readerWorks));
			
			rwlock.RestoreLock (ref co);
			RunThread (new ThreadStart (AcquireLock_readerFails));
			
			rwlock.ReleaseWriterLock ();
			Assert ("w2", rwlock.IsWriterLockHeld);
			rwlock.ReleaseWriterLock ();
			Assert ("w3", !rwlock.IsWriterLockHeld);
		}
		
		[Test]
		public void TestUpgradeDowngradeLock ()
		{
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireReaderLock (200);
			rwlock.AcquireReaderLock (200);
			
			LockCookie co = rwlock.UpgradeToWriterLock (200);
			Assert ("u1", !rwlock.IsReaderLockHeld);
			Assert ("u2", rwlock.IsWriterLockHeld);
			RunThread (new ThreadStart (AcquireLock_writerFails));
			
			rwlock.DowngradeFromWriterLock (ref co);
			Assert ("u3", rwlock.IsReaderLockHeld);
			Assert ("u4", !rwlock.IsWriterLockHeld);
			RunThread (new ThreadStart (AcquireLock_readerWorks));
			
			rwlock.ReleaseReaderLock ();
			Assert ("u5", rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert ("u6", !rwlock.IsReaderLockHeld);
		}
		
		[Test]
		public void TestReaderInsideWriter ()
		{
			// Reader acquires and releases work like the writer equivalent
			
			rwlock = new ReaderWriterLock ();
			rwlock.AcquireWriterLock (-1);
			rwlock.AcquireReaderLock (-1);
			Assert ("u1", !rwlock.IsReaderLockHeld);
			Assert ("u2", rwlock.IsWriterLockHeld);
			rwlock.AcquireReaderLock (-1);
			Assert ("u3", !rwlock.IsReaderLockHeld);
			Assert ("u4", rwlock.IsWriterLockHeld);
			rwlock.ReleaseWriterLock ();
			Assert ("u5", !rwlock.IsReaderLockHeld);
			Assert ("u6", rwlock.IsWriterLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert ("u7", !rwlock.IsReaderLockHeld);
			Assert ("u8", rwlock.IsWriterLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert ("u9", !rwlock.IsReaderLockHeld);
			Assert ("u10", !rwlock.IsWriterLockHeld);
		}
		
		[Test]
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
		
	}
}
