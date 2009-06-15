//
// System.Threading.ReaderWriterLockSlim.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com) 
//   Dick Porter (dick@ximian.com)
//   Jackson Harper (jackson@ximian.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2004-2008 Novell, Inc (http://www.novell.com)
// Copyright 2003, Ximian, Inc.
//
// NoRecursion code based on the blog post from Vance Morrison:
//   http://blogs.msdn.com/vancem/archive/2006/03/28/563180.aspx
//
// Recursion code based on Mono's implementation of ReaderWriterLock.
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
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Diagnostics;
using System.Threading;

namespace System.Threading {

	//
	// This implementation is based on the light-weight
	// Reader/Writer lock sample from Vance Morrison's blog:
	//
	// http://blogs.msdn.com/vancem/archive/2006/03/28/563180.aspx
	//
	// And in Mono's ReaderWriterLock
	//
	[HostProtectionAttribute(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class ReaderWriterLockSlim : IDisposable {
		sealed class LockDetails
		{
			public int ThreadId;
			public int ReadLocks;
		}

		// Are we on a multiprocessor?
		static readonly bool smp;
		
		// Lock specifiation for myLock:  This lock protects exactly the local fields associted
		// instance of MyReaderWriterLock.  It does NOT protect the memory associted with the
		// the events that hang off this lock (eg writeEvent, readEvent upgradeEvent).
		int myLock;

		// Who owns the lock owners > 0 => readers
		// owners = -1 means there is one writer, Owners must be >= -1.  
		int owners;
		Thread upgradable_thread;
		Thread write_thread;
		
		// These variables allow use to avoid Setting events (which is expensive) if we don't have to. 
		uint numWriteWaiters;        // maximum number of threads that can be doing a WaitOne on the writeEvent 
		uint numReadWaiters;         // maximum number of threads that can be doing a WaitOne on the readEvent
		uint numUpgradeWaiters;      // maximum number of threads that can be doing a WaitOne on the upgradeEvent (at most 1). 
		
		// conditions we wait on. 
		EventWaitHandle writeEvent;    // threads waiting to aquire a write lock go here.
		EventWaitHandle readEvent;     // threads waiting to aquire a read lock go here (will be released in bulk)
		EventWaitHandle upgradeEvent;  // thread waiting to upgrade a read lock to a write lock go here (at most one)

		//int lock_owner;

		// Only set if we are a recursive lock
		//Dictionary<int,int> reader_locks;

		readonly LockRecursionPolicy recursionPolicy;
		LockDetails[] read_locks = new LockDetails [8];
		
		static ReaderWriterLockSlim ()
		{
			smp = Environment.ProcessorCount > 1;
		}
		
		public ReaderWriterLockSlim ()
		{
			// NoRecursion (0) is the default value
		}

		public ReaderWriterLockSlim (LockRecursionPolicy recursionPolicy)
		{
			this.recursionPolicy = recursionPolicy;
			
			if (recursionPolicy != LockRecursionPolicy.NoRecursion){
				//reader_locks = new Dictionary<int,int> ();
				throw new NotImplementedException ("recursionPolicy != NoRecursion not currently implemented");
			}
		}

		public void EnterReadLock ()
		{
			TryEnterReadLock (-1);
		}

		public bool TryEnterReadLock (int millisecondsTimeout)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			
			if (read_locks == null)
				throw new ObjectDisposedException (null);

			if (Thread.CurrentThread == write_thread)
				throw new LockRecursionException ("Read lock cannot be acquired while write lock is held");

			EnterMyLock ();

			LockDetails ld = GetReadLockDetails (Thread.CurrentThread.ManagedThreadId, true);
			if (ld.ReadLocks != 0) {
				ExitMyLock ();
				throw new LockRecursionException ("Recursive read lock can only be aquired in SupportsRecursion mode");
			}
			++ld.ReadLocks;
			
			while (true){
				// Easy case, no contention
				// owners >= 0 means there might be readers (but no writer)
				if (owners >= 0 && numWriteWaiters == 0){
					owners++;
					break;
				}
				
				// If the request is to probe.
				if (millisecondsTimeout == 0){
					ExitMyLock ();
					return false;
				}

				// We need to wait.  Mark that we have waiters and wait.  
				if (readEvent == null) {
					LazyCreateEvent (ref readEvent, false);
					// since we left the lock, start over. 
					continue;
				}

				if (!WaitOnEvent (readEvent, ref numReadWaiters, millisecondsTimeout))
					return false;
			}
			ExitMyLock ();
			
			return true;
		}

		public bool TryEnterReadLock (TimeSpan timeout)
		{
			return TryEnterReadLock (CheckTimeout (timeout));
		}

		//
		// TODO: What to do if we are releasing a ReadLock and we do not own it?
		//
		public void ExitReadLock ()
		{
			EnterMyLock ();

			if (owners < 1) {
				ExitMyLock ();
				throw new SynchronizationLockException ("Releasing lock and no read lock taken");
			}

			--owners;
			--GetReadLockDetails (Thread.CurrentThread.ManagedThreadId, false).ReadLocks;

			ExitAndWakeUpAppropriateWaiters ();
		}

		public void EnterWriteLock ()
		{
			TryEnterWriteLock (-1);
		}
		
		public bool TryEnterWriteLock (int millisecondsTimeout)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			if (read_locks == null)
				throw new ObjectDisposedException (null);

			if (IsWriteLockHeld)
				throw new LockRecursionException ();

			EnterMyLock ();

			LockDetails ld = GetReadLockDetails (Thread.CurrentThread.ManagedThreadId, false);
			if (ld != null && ld.ReadLocks > 0) {
				ExitMyLock ();
				throw new LockRecursionException ("Write lock cannot be acquired while read lock is held");
			}

			while (true){
				// There is no contention, we are done
				if (owners == 0){
					// Indicate that we have a writer
					owners = -1;
					write_thread = Thread.CurrentThread;
					break;
				}

				// If we are the thread that took the Upgradable read lock
				if (owners == 1 && upgradable_thread == Thread.CurrentThread){
					owners = -1;
					write_thread = Thread.CurrentThread;
					break;
				}

				// If the request is to probe.
				if (millisecondsTimeout == 0){
					ExitMyLock ();
					return false;
				}

				// We need to wait, figure out what kind of waiting.
				
				if (upgradable_thread == Thread.CurrentThread){
					// We are the upgradable thread, register our interest.
					
					if (upgradeEvent == null){
						LazyCreateEvent (ref upgradeEvent, false);

						// since we left the lock, start over.
						continue;
					}

					if (numUpgradeWaiters > 0){
						ExitMyLock ();
						throw new ApplicationException ("Upgrading lock to writer lock already in process, deadlock");
					}
					
					if (!WaitOnEvent (upgradeEvent, ref numUpgradeWaiters, millisecondsTimeout))
						return false;
				} else {
					if (writeEvent == null){
						LazyCreateEvent (ref writeEvent, true);

						// since we left the lock, retry
						continue;
					}
					if (!WaitOnEvent (writeEvent, ref numWriteWaiters, millisecondsTimeout))
						return false;
				}
			}

			Debug.Assert (owners == -1, "Owners is not -1");
			ExitMyLock ();
			return true;
		}

		public bool TryEnterWriteLock (TimeSpan timeout)
		{
			return TryEnterWriteLock (CheckTimeout (timeout));
		}

		public void ExitWriteLock ()
		{
			EnterMyLock ();

			if (owners != -1) {
				ExitMyLock ();
				throw new SynchronizationLockException ("Calling ExitWriterLock when no write lock is held");
			}

			//Debug.Assert (numUpgradeWaiters > 0);
			if (upgradable_thread == Thread.CurrentThread)
				owners = 1;
			else
				owners = 0;
			write_thread = null;
			ExitAndWakeUpAppropriateWaiters ();
		}

		public void EnterUpgradeableReadLock ()
		{
			TryEnterUpgradeableReadLock (-1);
		}

		//
		// Taking the Upgradable read lock is like taking a read lock
		// but we limit it to a single upgradable at a time.
		//
		public bool TryEnterUpgradeableReadLock (int millisecondsTimeout)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			if (read_locks == null)
				throw new ObjectDisposedException (null);

			if (IsUpgradeableReadLockHeld)
				throw new LockRecursionException ();

			if (IsWriteLockHeld)
				throw new LockRecursionException ();

			EnterMyLock ();
			while (true){
				if (owners == 0 && numWriteWaiters == 0 && upgradable_thread == null){
					owners++;
					upgradable_thread = Thread.CurrentThread;
					break;
				}

				// If the request is to probe
				if (millisecondsTimeout == 0){
					ExitMyLock ();
					return false;
				}

				if (readEvent == null){
					LazyCreateEvent (ref readEvent, false);
					// since we left the lock, start over.
					continue;
				}

				if (!WaitOnEvent (readEvent, ref numReadWaiters, millisecondsTimeout))
					return false;
			}

			ExitMyLock ();
			return true;
		}

		public bool TryEnterUpgradeableReadLock (TimeSpan timeout)
		{
			return TryEnterUpgradeableReadLock (CheckTimeout (timeout));
		}
	       
		public void ExitUpgradeableReadLock ()
		{
			EnterMyLock ();
			Debug.Assert (owners > 0, "Releasing an upgradable lock, but there was no reader!");
			--owners;
			upgradable_thread = null;
			ExitAndWakeUpAppropriateWaiters ();
		}

		public void Dispose ()
		{
			read_locks = null;
		}

		public bool IsReadLockHeld {
			get { return RecursiveReadCount != 0; }
		}
		
		public bool IsWriteLockHeld {
			get { return RecursiveWriteCount != 0; }
		}
		
		public bool IsUpgradeableReadLockHeld {
			get { return RecursiveUpgradeCount != 0; }
		}

		public int CurrentReadCount {
			get { return owners & 0xFFFFFFF; }
		}
		
		public int RecursiveReadCount {
			get {
				EnterMyLock ();
				LockDetails ld = GetReadLockDetails (Thread.CurrentThread.ManagedThreadId, false);
				int count = ld == null ? 0 : ld.ReadLocks;
				ExitMyLock ();
				return count;
			}
		}

		public int RecursiveUpgradeCount {
			get { return upgradable_thread == Thread.CurrentThread ? 1 : 0; }
		}

		public int RecursiveWriteCount {
			get { return write_thread == Thread.CurrentThread ? 1 : 0; }
		}

		public int WaitingReadCount {
			get { return (int) numReadWaiters; }
		}

		public int WaitingUpgradeCount {
			get { return (int) numUpgradeWaiters; }
		}

		public int WaitingWriteCount {
			get { return (int) numWriteWaiters; }
		}

		public LockRecursionPolicy RecursionPolicy {
			get { return recursionPolicy; }
		}
		
#region Private methods
		void EnterMyLock ()
		{
			if (Interlocked.CompareExchange(ref myLock, 1, 0) != 0)
				EnterMyLockSpin ();
		}

		void EnterMyLockSpin ()
		{

			for (int i = 0; ;i++) {
				if (i < 3 && smp)
					Thread.SpinWait (20);    // Wait a few dozen instructions to let another processor release lock. 
				else 
					Thread.Sleep (0);        // Give up my quantum.  

				if (Interlocked.CompareExchange(ref myLock, 1, 0) == 0)
					return;
			}
		}

		void ExitMyLock()
		{
			Debug.Assert (myLock != 0, "Exiting spin lock that is not held");
			myLock = 0;
		}

		bool MyLockHeld { get { return myLock != 0; } }

		/// <summary>
		/// Determines the appropriate events to set, leaves the locks, and sets the events. 
		/// </summary>
		private void ExitAndWakeUpAppropriateWaiters()
		{
			Debug.Assert (MyLockHeld);

			// First a writing thread waiting on being upgraded
			if (owners == 1 && numUpgradeWaiters != 0){
				// Exit before signaling to improve efficiency (wakee will need the lock)
				ExitMyLock ();
				// release all upgraders (however there can be at most one). 
				upgradeEvent.Set ();
				//
				// TODO: What does the following comment mean?
				// two threads upgrading is a guarenteed deadlock, so we throw in that case. 
			} else if (owners == 0 && numWriteWaiters > 0) {
				// Exit before signaling to improve efficiency (wakee will need the lock)
				ExitMyLock ();
				// release one writer. 
				writeEvent.Set ();
			}
			else if (owners >= 0 && numReadWaiters != 0) {
				// Exit before signaling to improve efficiency (wakee will need the lock)
				ExitMyLock ();
				// release all readers.
				readEvent.Set();
			} else
				ExitMyLock();
		}

		/// <summary>
		/// A routine for lazily creating a event outside the lock (so if errors
		/// happen they are outside the lock and that we don't do much work
		/// while holding a spin lock).  If all goes well, reenter the lock and
		/// set 'waitEvent' 
		/// </summary>
		void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
		{
			Debug.Assert (MyLockHeld);
			Debug.Assert (waitEvent == null);
			
			ExitMyLock ();
			EventWaitHandle newEvent;
			if (makeAutoResetEvent) 
				newEvent = new AutoResetEvent (false);
			else 
				newEvent = new ManualResetEvent (false);

			EnterMyLock ();

			// maybe someone snuck in. 
			if (waitEvent == null)
				waitEvent = newEvent;
		}

		/// <summary>
		/// Waits on 'waitEvent' with a timeout of 'millisceondsTimeout.  
		/// Before the wait 'numWaiters' is incremented and is restored before leaving this routine.
		/// </summary>
		bool WaitOnEvent (EventWaitHandle waitEvent, ref uint numWaiters, int millisecondsTimeout)
		{
			Debug.Assert (MyLockHeld);

			waitEvent.Reset ();
			numWaiters++;

			bool waitSuccessful = false;

			// Do the wait outside of any lock 
			ExitMyLock();      
			try {
				waitSuccessful = waitEvent.WaitOne (millisecondsTimeout, false);
			} finally {
				EnterMyLock ();
				--numWaiters;
				if (!waitSuccessful)
					ExitMyLock ();
			}
			return waitSuccessful;
		}
		
		static int CheckTimeout (TimeSpan timeout)
		{
			try {
				return checked((int) timeout.TotalMilliseconds);
			} catch (System.OverflowException) {
				throw new ArgumentOutOfRangeException ("timeout");				
			}
		}

		LockDetails GetReadLockDetails (int threadId, bool create)
		{
			int i;
			LockDetails ld;
			for (i = 0; i < read_locks.Length; ++i) {
				ld = read_locks [i];
				if (ld == null)
					break;

				if (ld.ThreadId == threadId)
					return ld;
			}

			if (!create)
				return null;

			if (i == read_locks.Length)
				Array.Resize (ref read_locks, read_locks.Length * 2);
				
			ld = read_locks [i] = new LockDetails ();
			ld.ThreadId = threadId;
			return ld;
		}
#endregion
	}
}
