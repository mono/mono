//
// System.Threading.LockQueue
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;

namespace System.Threading {

	// Used for queueing on a reader writer lock
	internal class LockQueue {

		private ReaderWriterLock rwlock;
		private int lockCount = 0;

		public LockQueue (ReaderWriterLock rwlock)
		{
			this.rwlock = rwlock;
		}

		public bool Wait (int timeout)
		{
			bool _lock = false;

			try {
				lock (this) {
					lockCount++;
					Monitor.Exit (rwlock);
					_lock = true;
					return Monitor.Wait (this, timeout);
				}
			} finally {
				if (_lock) {
					Monitor.Enter (rwlock);
					lockCount--;
				}
			}
		}
		
		public bool IsEmpty
		{
			get { lock (this) return (lockCount == 0); }
		}

		public void Pulse ()
		{
			lock (this) Monitor.Pulse (this);
		}

		public void PulseAll ()
		{
			lock (this) Monitor.PulseAll (this);
		}
	}
}

