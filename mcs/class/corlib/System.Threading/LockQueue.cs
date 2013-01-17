//
// System.Threading.LockQueue
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
	}
}

