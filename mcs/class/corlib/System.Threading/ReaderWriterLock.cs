//
// System.Threading.ReaderWriterLock.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class ReaderWriterLock
	{
		public ReaderWriterLock() {
			// FIXME
		}

		public bool IsReaderLockHeld {
			get {
				// FIXME
				return(false);
			}
		}

		public bool IsWriterLockHeld {
			get {
				// FIXME
				return(false);
			}
		}

		public int WriterSeqNum {
			get {
				// FIXME
				return(0);
			}
		}

		public void AcquireReaderLock(int millisecondsTimeout) {
			// FIXME
		}

		public void AcquireReaderLock(TimeSpan timeout) {
			// FIXME
		}

		public void AcquireWriterLock(int millisecondsTimeout) {
			// FIXME
		}

		public void AcquireWriterLock(TimeSpan timeout) {
			// FIXME
		}

		public bool AnyWritersSince(int seqNum) {
			// FIXME
			return(false);
		}

		public void DowngradeFromWriterLock(ref LockCookie lockCookie) {
			// FIXME
		}

		public LockCookie ReleaseLock() {
			// FIXME
			return(new LockCookie());
		}

		public void ReleaseReaderLock() {
			// FIXME
		}

		public void ReleaseWriterLock() {
			// FIXME
		}

		public void RestoreLock(ref LockCookie lockCookie) {
			// FIXME
		}

		public LockCookie UpgradeToWriterLock(int millisecondsTimeout) {
			// FIXME
			return(new LockCookie());
		}

		public LockCookie UpgradeToWriterLock(TimeSpan timeout) {
			// FIXME
			return(new LockCookie());
		}
	}
}
