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
	public class ReaderWriterLockTest : Assertion
	{
		ReaderWriterLock rwlock;
		
		Exception resultException;
		ThreadStart secondaryThread;
		
		void StartThread (ThreadStart ts)
		{
			secondaryThread = ts;
			resultException = null;
			Thread t = new Thread (new ThreadStart (ThreadRunner));
			t.Start ();
			t.Join ();
			if (resultException != null) throw resultException;
		}
		
		void ThreadRunner ()
		{
			try
			{
				secondaryThread();
			}
			catch (Exception ex)
			{
				resultException = ex;
			}
		}
		
		[Test]
		public void TestIsReaderLockHeld ()
		{
			rwlock = new ReaderWriterLock ();
			Assert ("a1", !rwlock.IsReaderLockHeld);
			rwlock.AcquireReaderLock (500);
			Assert ("a2", rwlock.IsReaderLockHeld);
			StartThread (new ThreadStart (IsReaderLockHeld_2));
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
			StartThread (new ThreadStart (IsWriterLockHeld_2));
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
			StartThread (new ThreadStart (AcquireLock_readerWorks));
			Assert ("a2", rwlock.IsReaderLockHeld);
			
			StartThread (new ThreadStart (AcquireLock_writerFails));
			rwlock.ReleaseReaderLock ();
			Assert ("a6", !rwlock.IsReaderLockHeld);
			
			StartThread (new ThreadStart (AcquireLock_writerWorks));
			
			rwlock.AcquireWriterLock (200);
			StartThread (new ThreadStart (AcquireLock_writerFails));
			StartThread (new ThreadStart (AcquireLock_readerFails));
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
			catch (Exception ex)
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
			catch (Exception ex)
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
			StartThread (new ThreadStart (AcquireLock_writerWorks));
			
			rwlock.RestoreLock (ref co);
			StartThread (new ThreadStart (AcquireLock_writerFails));
			
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
			StartThread (new ThreadStart (AcquireLock_readerWorks));
			
			rwlock.RestoreLock (ref co);
			StartThread (new ThreadStart (AcquireLock_readerFails));
			
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
			StartThread (new ThreadStart (AcquireLock_writerFails));
			
			rwlock.DowngradeFromWriterLock (ref co);
			Assert ("u3", rwlock.IsReaderLockHeld);
			Assert ("u4", !rwlock.IsWriterLockHeld);
			StartThread (new ThreadStart (AcquireLock_readerWorks));
			
			rwlock.ReleaseReaderLock ();
			Assert ("u5", rwlock.IsReaderLockHeld);
			rwlock.ReleaseReaderLock ();
			Assert ("u6", !rwlock.IsReaderLockHeld);
		}
	}
}
