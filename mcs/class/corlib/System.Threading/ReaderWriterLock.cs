//
// System.Threading.ReaderWriterLock.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Jackson Harper (jackson@ximian.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Collections;

namespace System.Threading
{
	public sealed class ReaderWriterLock
	{
		private int seq_num = 1;
		private int state = 0;
		private int readers = 0;
		private LockQueue writer_queue;
		private Hashtable reader_locks;
		private int writer_lock_owner;

		public ReaderWriterLock()
		{
			writer_queue = new LockQueue (this);
			reader_locks = new Hashtable ();
		}

		public bool IsReaderLockHeld {
			get {
				lock (this) return reader_locks.ContainsKey (Thread.CurrentThreadId);
			}
		}

		public bool IsWriterLockHeld {
			get {
				lock (this) return (state < 0 && Thread.CurrentThreadId == writer_lock_owner);
			}
		}

		public int WriterSeqNum {
			get {
				lock (this) return seq_num;
			}
		}

		public void AcquireReaderLock (int millisecondsTimeout) 
		{
			AcquireReaderLock (millisecondsTimeout, 1);
		}
		
		void AcquireReaderLock (int millisecondsTimeout, int initialLockCount) 
		{
			lock (this) {
				if (HasWriterLock ()) {
					AcquireWriterLock (millisecondsTimeout, initialLockCount);
					return;
				}
				
				// Wait if there is a write lock
				readers++;
				try {
					if (state < 0 || !writer_queue.IsEmpty) {
						if (!Monitor.Wait (this, millisecondsTimeout))
							throw new ApplicationException ("Timeout expired");
					}
				}
				finally {
					readers--;
				}
				
				object nlocks = reader_locks [Thread.CurrentThreadId];
				if (nlocks == null) {
					reader_locks [Thread.CurrentThreadId] = initialLockCount;
					state += initialLockCount;
				}
				else {
					reader_locks [Thread.CurrentThreadId] = ((int)nlocks) + 1;
					state++;
				}
			}
		}

		public void AcquireReaderLock(TimeSpan timeout)
		{
			int ms = CheckTimeout (timeout);
			AcquireReaderLock ((int) timeout.TotalMilliseconds, 1);
		}

		public void AcquireWriterLock (int millisecondsTimeout)
		{
			AcquireWriterLock (millisecondsTimeout, 1);
		}
		
		void AcquireWriterLock (int millisecondsTimeout, int initialLockCount)
		{
			lock (this) {
				if (HasWriterLock ()) {
					state--;
					return;
				}
				if (state != 0) { // wait while there are reader locks or another writer lock
					if (!writer_queue.Wait (millisecondsTimeout))
						throw new ApplicationException ("Timeout expited");
				}

				state = -initialLockCount;
				writer_lock_owner = Thread.CurrentThreadId;
				seq_num++;
			}
		}

		public void AcquireWriterLock(TimeSpan timeout) {
			int ms = CheckTimeout (timeout);
			AcquireWriterLock (ms, 1);
		}

		public bool AnyWritersSince(int seqNum) {
			lock (this) {
				return (this.seq_num > seqNum);
			}
		}

		public void DowngradeFromWriterLock(ref LockCookie lockCookie)
		{
			lock (this) {
				if (!HasWriterLock())
					throw new ApplicationException ("The thread does not have the writer lock.");
				
				state = lockCookie.ReaderLocks;
				reader_locks [Thread.CurrentThreadId] = state;
				Monitor.Pulse (this);
				
				// MSDN: A thread does not block when downgrading from the writer lock, 
				// even if other threads are waiting for the writer lock
			}
		}

		public LockCookie ReleaseLock()
		{
			LockCookie cookie;
			lock (this) {
				cookie = GetLockCookie ();
				if (cookie.WriterLocks != 0)
					ReleaseWriterLock (cookie.WriterLocks);
				else if (cookie.ReaderLocks != 0) {
					ReleaseReaderLock (cookie.ReaderLocks, cookie.ReaderLocks);
				}
			}
			return cookie;
		}
		
		public void ReleaseReaderLock()
		{
			lock (this) {
				if (HasWriterLock ()) {
					ReleaseWriterLock ();
					return;
				}
				else if (state > 0) {
					object read_lock_count = reader_locks [Thread.CurrentThreadId];
					if (read_lock_count != null) {
						ReleaseReaderLock ((int)read_lock_count, 1);
						return;
					}
				}
				throw new ApplicationException ("The thread does not have any reader or writer locks.");
			}
		}

		void ReleaseReaderLock (int currentCount, int releaseCount)
		{
			int new_count = currentCount - releaseCount;
			
			if (new_count == 0)
				reader_locks.Remove (Thread.CurrentThreadId);
			else
				reader_locks [Thread.CurrentThreadId] = new_count;
				
			state -= releaseCount;
			if (state == 0 && !writer_queue.IsEmpty)
				writer_queue.Pulse ();
		}

		public void ReleaseWriterLock()
		{
			lock (this) {
				if (!HasWriterLock())
					throw new ApplicationException ("The thread does not have the writer lock.");
				
				ReleaseWriterLock (1);
			}
		}

		void ReleaseWriterLock (int releaseCount)
		{
			state += releaseCount;
			if (state == 0) {
				if (readers > 0)
					Monitor.Pulse (this);
				else if (!writer_queue.IsEmpty)
					writer_queue.Pulse ();
			}
		}
		
		public void RestoreLock(ref LockCookie lockCookie)
		{
			lock (this) {
				if (lockCookie.WriterLocks != 0)
					AcquireWriterLock (-1, lockCookie.WriterLocks);
				else if (lockCookie.ReaderLocks != 0)
					AcquireReaderLock (-1, lockCookie.ReaderLocks);
			}
		}

		public LockCookie UpgradeToWriterLock(int millisecondsTimeout)
		{
			LockCookie cookie;
			lock (this) {
				cookie = GetLockCookie ();
				if (cookie.WriterLocks != 0) return cookie;
				
				if (cookie.ReaderLocks != 0)
					ReleaseReaderLock (cookie.ReaderLocks, cookie.ReaderLocks);
			}
			
			// Don't do this inside the lock, since it can cause a deadlock.
			AcquireWriterLock (millisecondsTimeout);
			return cookie;
		}
		
		public LockCookie UpgradeToWriterLock(TimeSpan timeout)
		{
			int ms = CheckTimeout (timeout);
			return UpgradeToWriterLock (ms);
		}
		
		LockCookie GetLockCookie ()
		{
			LockCookie cookie = new LockCookie (Thread.CurrentThreadId);
			
			if (HasWriterLock())
				cookie.WriterLocks = -state;
			else {
				object locks = reader_locks [Thread.CurrentThreadId];
				if (locks != null) cookie.ReaderLocks = (int)locks;
			}
			return cookie;
		}

		bool HasWriterLock ()
		{
			return (state < 0 && Thread.CurrentThreadId == writer_lock_owner);
		}
		
		private int CheckTimeout (TimeSpan timeout)
		{
			int ms = (int) timeout.TotalMilliseconds;

			if (ms < -1)
				throw new ArgumentOutOfRangeException ("timeout",
						"Number must be either non-negative or -1");
			return ms;
		}
	}
}

