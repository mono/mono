//
// System.Threading.ReaderWriterLock.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell, Inc (http://www.novell.com)
//


namespace System.Threading
{
	public sealed class ReaderWriterLock
	{
		private int seq_num = 1;
		private int state = 0;
		private int readers = 0;
		private bool writer_queued = false;
		private int upgrade_id = -1;
		private LockQueue writer_queue;

		public ReaderWriterLock()
		{
		}

		public bool IsReaderLockHeld {
			get {
				lock (this) return (state > 0);
			}
		}

		public bool IsWriterLockHeld {
			get {
				lock (this) return (state < 0);
			}
		}

		public int WriterSeqNum {
			get {
				lock (this) return seq_num;
			}
		}

		public void AcquireReaderLock(int millisecondsTimeout) {
			lock (this) {
				// Wait if there is a write lock
				readers++;
				if (state < 0 || writer_queued)
					Monitor.Wait (this, millisecondsTimeout);
				readers--;
				state++;
			}
		}

		public void AcquireReaderLock(TimeSpan timeout)
		{
			int ms = CheckTimeout (timeout);
			AcquireReaderLock ((int) timeout.TotalMilliseconds);
		}

		public void AcquireWriterLock(int millisecondsTimeout)
		{
			lock (this) {
				if (writer_queue == null)
					writer_queue = new LockQueue (this);
				writer_queued = true;
				if (state != 0) // wait while there are reader locks or another writer lock
					writer_queue.Wait (millisecondsTimeout);
				writer_queued = false;
				state = -1;
			}
			Interlocked.Increment (ref seq_num);
		}

		public void AcquireWriterLock(TimeSpan timeout) {
			int ms = CheckTimeout (timeout);
			AcquireWriterLock (ms);
		}

		public bool AnyWritersSince(int seqNum) {
			lock (this) {
				return (this.seq_num > seqNum);
			}
		}

		public void DowngradeFromWriterLock(ref LockCookie lockCookie)
		{
			lock (this) {
				state = 1;
				Monitor.Pulse (this);
				if (writer_queue != null)
					writer_queue.Pulse ();
			}
		}

		public LockCookie ReleaseLock()
		{
			LockCookie cookie;
			lock (this) {
				cookie = new LockCookie (Thread.CurrentThreadId, state < 0, state > 0);
				ReleaseWriterLock ();
			}
			return cookie;
		}

		public void ReleaseReaderLock()
		{
			lock (this) {
				int min_state = (upgrade_id == -1 ? 0 : 1);
				if (upgrade_id != -1 && upgrade_id == Thread.CurrentThreadId) {
					Monitor.Pulse (this);
					writer_queue.Pulse ();
					return;
				}
				if (--state == min_state && writer_queue != null)
					writer_queue.Pulse ();
			}
		}

		public void ReleaseWriterLock()
		{
			lock (this) {
				state = 0;
				if (readers > 0)
					Monitor.Pulse (this);
				else if (writer_queue != null)
					writer_queue.Pulse ();
			}
		}

		public void RestoreLock(ref LockCookie lockCookie)
		{
			lock (this) {
				if (lockCookie.IsWriter)
					AcquireWriterLock (-1);
				else if (lockCookie.IsReader)
					AcquireReaderLock (-1);
			}
		}

		public LockCookie UpgradeToWriterLock(int millisecondsTimeout)
		{
			LockCookie cookie;
			lock (this) {
				upgrade_id = Thread.CurrentThreadId;
				cookie = new LockCookie (upgrade_id, state < 0, state > 0);
				
				if (writer_queue == null)
					writer_queue = new LockQueue (this);
				
				writer_queued = true;
				if (state != 1) // wait until there is only 1 reader lock
					writer_queue.Wait (millisecondsTimeout);
				writer_queued = false;
				state = -1;
				upgrade_id = -1;
				
			}
			Interlocked.Increment (ref seq_num);
			return cookie;
		}
		
		public LockCookie UpgradeToWriterLock(TimeSpan timeout)
		{
			int ms = CheckTimeout (timeout);
			return UpgradeToWriterLock (ms);
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

