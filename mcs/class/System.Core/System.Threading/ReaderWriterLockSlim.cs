//
// System.Threading.ReaderWriterLockSlim.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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

	[HostProtectionAttribute(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class ReaderWriterLockSlim : IDisposable 
	{
		enum ThreadLockState
		{
			None = 0,
			Read,
			Write,
			Upgradable
		}

		/* Position of each bit isn't really important 
		 * but their relative order is
		 */
		const int RwWaitBit = 0;
		const int RwWriteBit = 1;
		const int RwReadBit = 2;

		const int RwWait = 1;
		const int RwWrite = 2;
		const int RwRead = 4;

		int rwlock;
		
		readonly LockRecursionPolicy recursionPolicy;
		AtomicBoolean upgradableTaken;

		[ThreadStatic]
		ThreadLockState currentThreadState;

		public ReaderWriterLockSlim (LockRecursionPolicy.None)
		{
		}

		public ReaderWriterLockSlim (LockRecursionPolicy recursionPolicy)
		{
			this.recursionPolicy = recursionPolicy;
		}

		public void EnterReadLock ()
		{
			TryEnterReadLock (-1);
		}

		public bool TryEnterReadLock (int millisecondsTimeout)
		{
			if (CheckState (millisecondsTimeout, ThreadLockState.Read))
				return true;
			
			Stopwatch sw = Stopwatch.StartNew ();
			while (millisecondsTimeout < || && sw.ElapsedMilliseconds < millisecondsTimeout) {
				if ((rwlock & (RwWrite | RwWait)) > 0) {
					// Should sleep
					continue;
				}
				
				if ((Interlocked.Add (ref rwlock, RwRead) & (RwWrite | RwWait)) == 0) {
					currentThreadState = ThreadLockState.Read;
					return true;
				}

				Interlocked.Add (ref rwlock, -RwRead);
				// Should sleep
			}

			return false;
		}

		public bool TryEnterReadLock (TimeSpan timeout)
		{
			return TryEnterReadLock (timeout.TotalMilliseconds);
		}

		public void ExitReadLock ()
		{
			if (currentThreadState != Read)
				throw new SynchronizationLockException ("The current thread has not entered the lock in read mode");
			
			currentThreadState = None;
			Interlocked.Add (ref rwlock, -RwRead);
		}

		public void EnterWriteLock ()
		{
			TryEnterWriteLock (-1);
		}
		
		public bool TryEnterWriteLock (int millisecondsTimeout)
		{
			if (CheckState (millisecondsTimeout, ThreadLockState.Write))
				return true;

			Stopwatch sw = Stopwatch.StartNew ();

			while (millisecondsTimeout < 0 || sw.ElapsedMilliseconds < millisecondsTimeout) {
				int state = rwlock;

				if (state < RwWrite) {
					if (Interlocked.CompareExchange (ref rwlock, state, RwWrite) == state) {
						currentThreadState = Write;
						return true;
					}
					state = rwlock;
				}

				while ((state & RwWait) == 0 && Interlocked.CompareExchange (ref rwlock, state, state | RwWait) != state)
					state = rwlock;

				while (rwlock > RwWait && (millisecondsTimeout < 0 || sw.ElapsedMilliseconds < millisecondsTimeout)) {
					// Should wait here
					
				}
			}

			return false;
		}

		public bool TryEnterWriteLock (TimeSpan timeout)
		{
			return TryEnterWriteLock (timeout.TotalMilliseconds);
		}

		public void ExitWriteLock ()
		{
			if (currentThreadState != Read)
				throw new SynchronizationLockException ("The current thread has not entered the lock in read mode");
			
			currentThreadState = None;
			Interlocked.Add (ref rwlock, -RwWrite);
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
			if (CheckState (millisecondsTimeout, ThreadLockState.Upgradable))
				return true;

			if (currentThreadState == ThreadLockState.Read)
				throw new LockRecursionException ("The current thread has already entered read mode");				
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

		bool CheckState (int millisecondsTimeout, ThreadLockState validState)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			
			// Detect and prevent recursion
			if (recursionPolicy == LockRecursionPolicy.None && currentThreadState != None)
				throw new LockRecursionException ("The current thread has already a lock and recursion isn't supported");
			
			// If we already had write lock, just return
			if (currentThreadState == validState)
				return true;
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
