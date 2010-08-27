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
		IDictionary<ReaderWriterLockSlim, ThreadLockState> currentThreadState;

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
					CurrentThreadState = ThreadLockState.Read;
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
			if (CurrentThreadState != Read)
				throw new SynchronizationLockException ("The current thread has not entered the lock in read mode");
			
			CurrentThreadState = None;
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
						CurrentThreadState = Write;
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
			if (CurrentThreadState != Read)
				throw new SynchronizationLockException ("The current thread has not entered the lock in read mode");
			
			CurrentThreadState = None;
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

			if (CurrentThreadState == ThreadLockState.Read)
				throw new LockRecursionException ("The current thread has already entered read mode");				
		}

		public bool TryEnterUpgradeableReadLock (TimeSpan timeout)
		{
			return TryEnterUpgradeableReadLock (CheckTimeout (timeout));
		}
	       
		public void ExitUpgradeableReadLock ()
		{
			
		}

		bool CheckState (int millisecondsTimeout, ThreadLockState validState)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			
			// Detect and prevent recursion
			if (recursionPolicy == LockRecursionPolicy.None && CurrentThreadState != None)
				throw new LockRecursionException ("The current thread has already a lock and recursion isn't supported");
			
			// If we already had write lock, just return
			if (CurrentThreadState == validState)
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
		ThreadLockState CurrentThreadState {
			get {
				// TODO: provide a IEqualityComparer thingie to have better hashes
				if (currentThreadState == null)
					currentThreadState = new Dictionary<ReaderWriterLockSlim, ThreadLockState> ();

				ThreadLockState state;
				if (!currentThreadState.TryGetValue (this, out state))
					currentThreadState[this] = state = ThreadLockState.None;

				return state;
			}
			set {
				if (currentThreadState == null)
					currentThreadState = new Dictionary<ReaderWriterLockSlim, ThreadLockState> ();

				currentThreadState[this] = value;
			}
		}
#endregion
	}
}
