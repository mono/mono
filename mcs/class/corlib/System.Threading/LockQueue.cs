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

		public LockQueue (ReaderWriterLock rwlock)
		{
			lock (this) this.rwlock = rwlock;
		}

		public void Wait (int timeout)
		{
			bool _lock = false;

			try {
				lock (this) {
					Monitor.Exit (rwlock);
					_lock = true;
					Monitor.Wait (this, timeout);
				}
			} finally {
				if (_lock)
					lock (this) Monitor.Enter (rwlock);
			}
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

