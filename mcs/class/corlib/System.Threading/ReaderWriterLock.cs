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
		[MonoTODO]
		public ReaderWriterLock() {
			// FIXME
		}

		[MonoTODO]
		public bool IsReaderLockHeld {
			get {
				// FIXME
				return(false);
			}
		}

		[MonoTODO]
		public bool IsWriterLockHeld {
			get {
				// FIXME
				return(false);
			}
		}

		[MonoTODO]
		public int WriterSeqNum {
			get {
				// FIXME
				return(0);
			}
		}

		
		[MonoTODO]
		public void AcquireReaderLock(int millisecondsTimeout) {
			// FIXME
		}

		[MonoTODO]
		public void AcquireReaderLock(TimeSpan timeout) {
			// FIXME
		}

		[MonoTODO]
		public void AcquireWriterLock(int millisecondsTimeout) {
			// FIXME
		}

		[MonoTODO]
		public void AcquireWriterLock(TimeSpan timeout) {
			// FIXME
		}

		[MonoTODO]
		public bool AnyWritersSince(int seqNum) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		public void DowngradeFromWriterLock(ref LockCookie lockCookie) {
			// FIXME
		}

		[MonoTODO]
		public LockCookie ReleaseLock() {
			// FIXME
			return(new LockCookie());
		}

		[MonoTODO]
		public void ReleaseReaderLock() {
			// FIXME
		}

		[MonoTODO]
		public void ReleaseWriterLock() {
			// FIXME
		}

		[MonoTODO]
		public void RestoreLock(ref LockCookie lockCookie) {
			// FIXME
		}

		[MonoTODO]
		public LockCookie UpgradeToWriterLock(int millisecondsTimeout) {
			// FIXME
			return(new LockCookie());
		}
		
		[MonoTODO]
		public LockCookie UpgradeToWriterLock(TimeSpan timeout) {
			// FIXME
			return(new LockCookie());
		}
	}
}
