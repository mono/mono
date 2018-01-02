//
// ReaderWriterLockSlimTest.cs
//
// Authors:
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using NUnit.Framework;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class ReaderWriterLockSlimTests
	{
		[Test]
		public void DefaultValues ()
		{
			var v = new ReaderWriterLockSlim ();
			Assert.AreEqual (0, v.CurrentReadCount, "1");
			Assert.AreEqual (false, v.IsReadLockHeld, "2");
			Assert.AreEqual (false, v.IsUpgradeableReadLockHeld, "3");
			Assert.AreEqual (false, v.IsWriteLockHeld, "4");
			Assert.AreEqual (LockRecursionPolicy.NoRecursion, v.RecursionPolicy, "5");
			Assert.AreEqual (0, v.RecursiveReadCount, "6");
			Assert.AreEqual (0, v.RecursiveUpgradeCount, "7");
			Assert.AreEqual (0, v.RecursiveWriteCount, "8");
			Assert.AreEqual (0, v.WaitingReadCount, "9");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "10");
			Assert.AreEqual (0, v.WaitingWriteCount, "11");
		}

		[Test]
		public void Dispose_Errors ()
		{
			var v = new ReaderWriterLockSlim ();
			v.Dispose ();

			try {
				v.EnterUpgradeableReadLock ();
				Assert.Fail ("1");
			} catch (ObjectDisposedException) {
			}

			try {
				v.EnterReadLock ();
				Assert.Fail ("2");
			} catch (ObjectDisposedException) {
			}

			try {
				v.EnterWriteLock ();
				Assert.Fail ("3");
			} catch (ObjectDisposedException) {
			}
		}

		[Test]
		public void Dispose_WithReadLock ()
		{
			var rwl = new ReaderWriterLockSlim ();
			rwl.EnterReadLock ();
			try {
				rwl.Dispose ();
				Assert.Fail ("1");
			} catch (SynchronizationLockException) {
			}
		}

		[Test]
		public void Dispose_WithWriteLock ()
		{
			var rwl = new ReaderWriterLockSlim ();
			rwl.EnterWriteLock ();
			try {
				rwl.Dispose ();
				Assert.Fail ("1");
			} catch (SynchronizationLockException) {
			}
		}

		[Test]
		public void Dispose_UpgradeableReadLock ()
		{
			var rwl = new ReaderWriterLockSlim ();
			rwl.EnterUpgradeableReadLock ();
			try {
				rwl.Dispose ();
				Assert.Fail ("1");
			} catch (SynchronizationLockException) {
			}
		}

		[Test]
		public void TryEnterReadLock_OutOfRange ()
		{
			var v = new ReaderWriterLockSlim ();
			try {
				v.TryEnterReadLock (-2);
				Assert.Fail ("1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v.TryEnterReadLock (TimeSpan.MaxValue);
				Assert.Fail ("2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v.TryEnterReadLock (TimeSpan.MinValue);
				Assert.Fail ("3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TryEnterUpgradeableReadLock_OutOfRange ()
		{
			var v = new ReaderWriterLockSlim ();
			try {
				v.TryEnterUpgradeableReadLock (-2);
				Assert.Fail ("1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v.TryEnterUpgradeableReadLock (TimeSpan.MaxValue);
				Assert.Fail ("2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v.TryEnterUpgradeableReadLock (TimeSpan.MinValue);
				Assert.Fail ("3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TryEnterWriteLock_OutOfRange ()
		{
			var v = new ReaderWriterLockSlim ();
			try {
				v.TryEnterWriteLock (-2);
				Assert.Fail ("1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v.TryEnterWriteLock (TimeSpan.MaxValue);
				Assert.Fail ("2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v.TryEnterWriteLock (TimeSpan.MinValue);
				Assert.Fail ("3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test, ExpectedException (typeof (SynchronizationLockException))]
		public void ExitReadLock ()
		{
			var v = new ReaderWriterLockSlim ();
			v.ExitReadLock ();
		}

		[Test, ExpectedException (typeof (SynchronizationLockException))]
		public void ExitWriteLock ()
		{
			var v = new ReaderWriterLockSlim ();
			v.ExitWriteLock ();
		}

		[Test]
		public void EnterReadLock_NoRecursionError ()
		{
			var v = new ReaderWriterLockSlim ();
			v.EnterReadLock ();
			Assert.AreEqual (1, v.RecursiveReadCount);

			try {
				v.EnterReadLock ();
				Assert.Fail ("1");
			} catch (LockRecursionException) {
			}

			try {
				v.EnterWriteLock ();
				Assert.Fail ("2");
			} catch (LockRecursionException) {
			}
		}

		[Test]
		public void EnterReadLock ()
		{
			var v = new ReaderWriterLockSlim ();

			v.EnterReadLock ();
			Assert.IsTrue (v.IsReadLockHeld, "A");
			Assert.AreEqual (0, v.RecursiveWriteCount, "A1");
			Assert.AreEqual (1, v.RecursiveReadCount, "A2");
			Assert.AreEqual (0, v.RecursiveUpgradeCount, "A3");
			Assert.AreEqual (0, v.WaitingReadCount, "A4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "A5");
			Assert.AreEqual (0, v.WaitingWriteCount, "A6");
			v.ExitReadLock ();

			v.EnterReadLock ();
			Assert.IsTrue (v.IsReadLockHeld, "B");
			Assert.AreEqual (0, v.RecursiveWriteCount, "B1");
			Assert.AreEqual (1, v.RecursiveReadCount, "B2");
			Assert.AreEqual (0, v.RecursiveUpgradeCount, "B3");
			Assert.AreEqual (0, v.WaitingReadCount, "B4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "B5");
			Assert.AreEqual (0, v.WaitingWriteCount, "B6");
			v.ExitReadLock ();
		}

		[Test]
		public void EnterWriteLock_NoRecursionError ()
		{
			var v = new ReaderWriterLockSlim ();
			v.EnterWriteLock ();
			Assert.AreEqual (1, v.RecursiveWriteCount);

			try {
				v.EnterWriteLock ();
				Assert.Fail ("1");
			} catch (LockRecursionException) {
			}

			try {
				v.EnterReadLock ();
				Assert.Fail ("2");
			} catch (LockRecursionException) {
			}
		}

		[Test]
		public void EnterWriteLock ()
		{
			var v = new ReaderWriterLockSlim ();

			v.EnterWriteLock ();
			Assert.IsTrue (v.IsWriteLockHeld, "A");
			Assert.AreEqual (1, v.RecursiveWriteCount, "A1");
			Assert.AreEqual (0, v.RecursiveReadCount, "A2");
			Assert.AreEqual (0, v.RecursiveUpgradeCount, "A3");
			Assert.AreEqual (0, v.WaitingReadCount, "A4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "A5");
			Assert.AreEqual (0, v.WaitingWriteCount, "A6");
			v.ExitWriteLock ();

			v.EnterWriteLock ();
			Assert.IsTrue (v.IsWriteLockHeld, "B");
			Assert.AreEqual (1, v.RecursiveWriteCount, "B1");
			Assert.AreEqual (0, v.RecursiveReadCount, "B2");
			Assert.AreEqual (0, v.RecursiveUpgradeCount, "B3");
			Assert.AreEqual (0, v.WaitingReadCount, "B4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "B5");
			Assert.AreEqual (0, v.WaitingWriteCount, "B6");
			v.ExitWriteLock ();
		}

		[Test]
		public void EnterUpgradeableReadLock_NoRecursionError ()
		{
			var v = new ReaderWriterLockSlim ();
			v.EnterUpgradeableReadLock ();
			Assert.AreEqual (1, v.RecursiveUpgradeCount);

			try {
				v.EnterUpgradeableReadLock ();
				Assert.Fail ("2");
			} catch (LockRecursionException) {
			}
		}

		[Test]
		public void EnterUpgradeableReadLock ()
		{
			var v = new ReaderWriterLockSlim ();

			v.EnterUpgradeableReadLock ();
			Assert.IsTrue (v.IsUpgradeableReadLockHeld, "A");
			Assert.AreEqual (0, v.RecursiveWriteCount, "A1");
			Assert.AreEqual (0, v.RecursiveReadCount, "A2");
			Assert.AreEqual (1, v.RecursiveUpgradeCount, "A3");
			Assert.AreEqual (0, v.WaitingReadCount, "A4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "A5");
			Assert.AreEqual (0, v.WaitingWriteCount, "A6");
			v.ExitUpgradeableReadLock ();

			v.EnterUpgradeableReadLock ();
			Assert.IsTrue (v.IsUpgradeableReadLockHeld, "B");
			Assert.AreEqual (0, v.RecursiveWriteCount, "B1");
			Assert.AreEqual (0, v.RecursiveReadCount, "B2");
			Assert.AreEqual (1, v.RecursiveUpgradeCount, "B3");
			Assert.AreEqual (0, v.WaitingReadCount, "B4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "B5");
			Assert.AreEqual (0, v.WaitingWriteCount, "B6");

			v.EnterReadLock ();
			v.ExitUpgradeableReadLock ();

			Assert.IsTrue (v.IsReadLockHeld, "C");
			Assert.AreEqual (0, v.RecursiveWriteCount, "C1");
			Assert.AreEqual (1, v.RecursiveReadCount, "C2");
			Assert.AreEqual (0, v.RecursiveUpgradeCount, "C3");
			Assert.AreEqual (0, v.WaitingReadCount, "C4");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "C5");
			Assert.AreEqual (0, v.WaitingWriteCount, "C6");

			v.ExitReadLock ();
		}

		[Test]
		[Category ("MultiThreaded")]
		public void EnterReadLock_MultiRead ()
		{
			var v = new ReaderWriterLockSlim ();
			int local = 10;

			var r = from i in Enumerable.Range (1, 30) select new Thread (() => {

				// Just to cause some contention
				Thread.Sleep (100);

				v.EnterReadLock ();

				Assert.AreEqual (10, local);
			});

			var threads = r.ToList ();

			foreach (var t in threads) {
				t.Start ();
			}

			foreach (var t in threads) {
				// Console.WriteLine (t.ThreadState);
				t.Join ();
			}
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TryEnterWriteLock_WhileReading ()
		{
			var v = new ReaderWriterLockSlim ();
			AutoResetEvent ev = new AutoResetEvent (false);
			AutoResetEvent ev2 = new AutoResetEvent (false);

			Thread t1 = new Thread (() => {
				v.EnterReadLock ();
				ev2.Set ();
				ev.WaitOne ();
				v.ExitReadLock ();
			});

			t1.Start ();
			ev2.WaitOne ();

			Assert.IsFalse (v.TryEnterWriteLock (100));
			ev.Set ();
			t1.Join ();

			Assert.IsTrue (v.TryEnterWriteLock (100));
		}

		[Test]
		[Category ("MultiThreaded")]
		public void EnterWriteLock_MultiRead ()
		{
			var v = new ReaderWriterLockSlim ();
			int local = 10;
			int ready_count = 0;
			int entered_count = 0;
			const int thread_count = 10;

			var r = from i in Enumerable.Range (1, thread_count) select new Thread (() => {
				Interlocked.Increment (ref ready_count);
				v.EnterReadLock ();
				Interlocked.Increment (ref entered_count);

				Assert.AreEqual (11, local);
			});

			v.EnterWriteLock ();

			var threads = r.ToList ();
			foreach (var t in threads) {
				t.Start ();
			}

			while (ready_count != thread_count)
				Thread.Sleep (10);

			/* Extra up to 2s of sleep to ensure all threads got the chance to enter the lock */
			for (int i = 0; i < 200 && v.WaitingReadCount != thread_count; ++i)
				Thread.Sleep (10);
			local = 11;

			Assert.AreEqual (0, v.WaitingWriteCount, "in waiting write");
			Assert.AreEqual (thread_count, v.WaitingReadCount, "in waiting read");
			Assert.AreEqual (0, v.WaitingUpgradeCount, "in waiting upgrade");
			v.ExitWriteLock ();

			foreach (var t in threads) {
				// Console.WriteLine (t.ThreadState);
				t.Join ();
			}
		}

		[Test]
		public void EnterWriteLock_After_ExitUpgradeableReadLock ()
		{
			var v = new ReaderWriterLockSlim ();

			v.EnterUpgradeableReadLock ();
			Assert.IsTrue (v.TryEnterWriteLock (100));
			v.ExitWriteLock ();
			v.ExitUpgradeableReadLock ();
			Assert.IsTrue (v.TryEnterWriteLock (100));
			v.ExitWriteLock ();
		}

		[Test]
		[Category ("MultiThreaded")]
		public void EnterWriteLockWhileInUpgradeAndOtherWaiting ()
		{
			var v = new ReaderWriterLockSlim ();

			var task2 = new Task(() => {
                v.EnterWriteLock();
                v.ExitWriteLock();
            });

            var task1 = new Task(() =>
            {
                v.EnterUpgradeableReadLock ();
                task2.Start ();
                Thread.Sleep (100);
                v.EnterWriteLock ();
                v.ExitWriteLock ();
                v.ExitUpgradeableReadLock ();
            });
            task1.Start ();

            Assert.IsTrue (task1.Wait (500));
		}
		[Test]
		public void RecursiveReadLockTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			Assert.IsTrue (v.TryEnterReadLock (100), "#1");
			Assert.IsTrue (v.TryEnterReadLock (100), "#2");
			Assert.IsTrue (v.TryEnterReadLock (100), "#3");

			Assert.AreEqual (3, v.RecursiveReadCount);
		}

		[Test]
		public void RecursiveReadPlusWriteLockTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			try {
				v.EnterReadLock ();
				v.EnterWriteLock ();
				Assert.Fail ("1");
			} catch (LockRecursionException ex) {
				Assert.IsNotNull (ex, "#1");
			}
		}

		[Test]
		public void RecursiveReadPlusUpgradeableLockTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			try {
				v.EnterReadLock ();
				v.EnterUpgradeableReadLock ();
				Assert.Fail ("1");
			} catch (LockRecursionException ex) {
				Assert.IsNotNull (ex, "#1");
			}
		}

		[Test]
		public void RecursiveWriteLockTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			Assert.IsTrue (v.TryEnterWriteLock (100), "#1");
			Assert.IsTrue (v.TryEnterWriteLock (100), "#2");
			Assert.IsTrue (v.TryEnterWriteLock (100), "#3");

			Assert.AreEqual (3, v.RecursiveWriteCount);
		}

		[Test]
		public void RecursiveWritePlusReadLockTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			Assert.IsTrue (v.TryEnterWriteLock (100), "#1");
			Assert.AreEqual (1, v.RecursiveWriteCount, "1b");
			Assert.AreEqual (0, v.RecursiveReadCount, "1c");

			Assert.IsTrue (v.TryEnterReadLock (100), "#2");
			Assert.AreEqual (1, v.RecursiveWriteCount, "2b");
			Assert.AreEqual (1, v.RecursiveReadCount, "2c");

			Assert.IsTrue (v.TryEnterReadLock (100), "#3");
			Assert.AreEqual (1, v.RecursiveWriteCount, "3b");
			Assert.AreEqual (2, v.RecursiveReadCount, "3c");

			v.ExitReadLock ();
			Assert.AreEqual (1, v.RecursiveWriteCount, "4b");
			Assert.AreEqual (1, v.RecursiveReadCount, "4c");
		}

		[Test]
		public void RecursiveUpgradeableReadLockTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			Assert.IsTrue (v.TryEnterUpgradeableReadLock (100), "#1");
			Assert.IsTrue (v.TryEnterUpgradeableReadLock (100), "#2");
			Assert.IsTrue (v.TryEnterUpgradeableReadLock (100), "#3");

			Assert.AreEqual (3, v.RecursiveUpgradeCount);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void RecursiveReadPropertiesTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterReadLock ();
			v.EnterReadLock ();

			Assert.AreEqual (true, v.IsReadLockHeld, "#1a");
			Assert.AreEqual (1, v.CurrentReadCount, "#2a");
			Assert.AreEqual (2, v.RecursiveReadCount, "#3a");

			bool rLock = true;
			int cReadCount = -1, rReadCount = -1;

			Thread t = new Thread ((_) => {
					rLock = v.IsReadLockHeld;
					cReadCount = v.CurrentReadCount;
					rReadCount = v.RecursiveReadCount;
				});

			t.Start ();
			t.Join ();

			Assert.AreEqual (false, rLock, "#1b");
			Assert.AreEqual (1, cReadCount, "#2b");
			Assert.AreEqual (0, rReadCount, "#3b");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void RecursiveUpgradePropertiesTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterUpgradeableReadLock ();
			v.EnterUpgradeableReadLock ();

			Assert.AreEqual (true, v.IsUpgradeableReadLockHeld, "#1a");
			Assert.AreEqual (false, v.IsReadLockHeld, "#11a");
			Assert.AreEqual (0, v.CurrentReadCount, "#2a");
			Assert.AreEqual (2, v.RecursiveUpgradeCount, "#3a");

			bool upLock = false, rLock = false;
			int rCount = -1, rUCount = -1;

			Thread t = new Thread ((_) => {
					upLock = v.IsUpgradeableReadLockHeld;
					rLock = v.IsReadLockHeld;
					rCount = v.CurrentReadCount;
					rUCount = v.RecursiveUpgradeCount;
				});

			t.Start ();
			t.Join ();

			Assert.AreEqual (false, upLock, "#1b");
			Assert.AreEqual (false, rLock, "#11b");
			Assert.AreEqual (0, rCount, "#2b");
			Assert.AreEqual (0, rUCount, "#3b");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void RecursiveWritePropertiesTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterWriteLock ();
			v.EnterWriteLock ();

			Assert.AreEqual (true, v.IsWriteLockHeld, "#1a");
			Assert.AreEqual (2, v.RecursiveWriteCount, "#3a");

			bool wLock = false;
			int rWrite = -1;

			Thread t = new Thread ((_) => {
					wLock = v.IsWriteLockHeld;
					rWrite = v.RecursiveWriteCount;
				});

			t.Start ();
			t.Join ();

			Assert.AreEqual (false, wLock, "#1b");
			Assert.AreEqual (0, rWrite, "#3b");
		}

		[Test]
		public void RecursiveEnterExitReadTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterReadLock ();
			v.EnterReadLock ();
			v.EnterReadLock ();

			Assert.IsTrue (v.IsReadLockHeld);
			Assert.AreEqual (3, v.RecursiveReadCount);

			v.ExitReadLock ();

			Assert.IsTrue (v.IsReadLockHeld);
			Assert.AreEqual (2, v.RecursiveReadCount);
		}

		[Test]
		public void RecursiveEnterExitWriteTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterWriteLock ();
			v.EnterWriteLock ();
			v.EnterWriteLock ();

			Assert.IsTrue (v.IsWriteLockHeld);
			Assert.AreEqual (3, v.RecursiveWriteCount);

			v.ExitWriteLock ();
			v.ExitWriteLock ();

			Assert.IsTrue (v.IsWriteLockHeld);
			Assert.AreEqual (1, v.RecursiveWriteCount);
		}

		[Test]
		public void RecursiveEnterExitUpgradableTest ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterUpgradeableReadLock ();
			v.EnterUpgradeableReadLock ();
			v.EnterUpgradeableReadLock ();

			Assert.IsTrue (v.IsUpgradeableReadLockHeld);
			Assert.AreEqual (3, v.RecursiveUpgradeCount);

			v.ExitUpgradeableReadLock ();

			Assert.IsTrue (v.IsUpgradeableReadLockHeld);
			Assert.AreEqual (2, v.RecursiveUpgradeCount);
		}

		[Test]
		public void RecursiveWriteUpgradeReadTest ()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

			rwlock.EnterWriteLock ();
			Assert.IsTrue (rwlock.IsWriteLockHeld);
			rwlock.EnterUpgradeableReadLock ();
			Assert.IsTrue (rwlock.IsUpgradeableReadLockHeld);
			rwlock.EnterReadLock ();
			Assert.IsTrue (rwlock.IsReadLockHeld);
			rwlock.ExitUpgradeableReadLock();
			Assert.IsFalse (rwlock.IsUpgradeableReadLockHeld);
			Assert.IsTrue (rwlock.IsReadLockHeld);
			Assert.IsTrue (rwlock.IsWriteLockHeld);

			rwlock.ExitReadLock ();
			Assert.IsTrue (rwlock.IsWriteLockHeld);
		}

		[Test]
		public void RecursiveWriteUpgradeTest ()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

			rwlock.EnterWriteLock ();
			Assert.IsTrue (rwlock.IsWriteLockHeld);
			rwlock.EnterUpgradeableReadLock ();
			Assert.IsTrue (rwlock.IsUpgradeableReadLockHeld);
			rwlock.ExitUpgradeableReadLock ();
			Assert.IsFalse (rwlock.IsUpgradeableReadLockHeld);
			Assert.IsTrue (rwlock.IsWriteLockHeld);
			rwlock.ExitWriteLock ();
			Assert.IsFalse (rwlock.IsWriteLockHeld);
			rwlock.EnterWriteLock ();
			Assert.IsTrue (rwlock.IsWriteLockHeld);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void RecursiveWriteReadAcquisitionInterleaving ()
		{
			var v = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);

			v.EnterWriteLock ();
			Assert.IsTrue (v.IsWriteLockHeld, "#1");

			bool result = true;
			var t = new Thread (delegate () {
					result = v.TryEnterReadLock (100);
				});
			t.Start ();
			t.Join ();
			Assert.IsFalse (result, "#2");

			v.ExitWriteLock ();
			t = new Thread (delegate () {
					result = v.TryEnterReadLock (100);
				});
			t.Start ();
			t.Join ();
			Assert.IsTrue (result, "#3");
		}
	}
}
