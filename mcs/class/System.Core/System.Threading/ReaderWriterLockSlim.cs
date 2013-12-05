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
using System.Runtime.CompilerServices;

namespace System.Threading {

	[HostProtectionAttribute(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class ReaderWriterLockSlim : IDisposable
	{
		/* Position of each bit isn't really important 
		 * but their relative order is
		 */
		const int RwReadBit = 3;

		/* These values are used to manipulate the corresponding flags in rwlock field
		 */
		const int RwWait = 1;
		const int RwWaitUpgrade = 2;
		const int RwWrite = 4;
		const int RwRead = 8;

		/* Some explanations: this field is the central point of the lock and keep track of all the requests
		 * that are being made. The 3 lowest bits are used as flag to track "destructive" lock entries
		 * (i.e attempting to take the write lock with or without having acquired an upgradeable lock beforehand).
		 * All the remaining bits are intepreted as the actual number of reader currently using the lock
		 * (which mean the lock is limited to 2^29 concurrent readers but since it's a high number there
		 * is no overflow safe guard to remain simple).
		 */
		int rwlock;
		
		readonly LockRecursionPolicy recursionPolicy;
		readonly bool noRecursion;

		AtomicBoolean upgradableTaken = new AtomicBoolean ();

		/* These events are just here for the sake of having a CPU-efficient sleep
		 * when the wait for acquiring the lock is too long
		 */
#if NET_4_0
		ManualResetEventSlim upgradableEvent = new ManualResetEventSlim (true);
		ManualResetEventSlim writerDoneEvent = new ManualResetEventSlim (true);
		ManualResetEventSlim readerDoneEvent = new ManualResetEventSlim (true);
#else
		ManualResetEvent upgradableEvent = new ManualResetEvent (true);
		ManualResetEvent writerDoneEvent = new ManualResetEvent (true);
		ManualResetEvent readerDoneEvent = new ManualResetEvent (true);
#endif

		// This Stopwatch instance is used for all threads since .Elapsed is thread-safe
		readonly static Stopwatch sw = Stopwatch.StartNew ();

		/* For performance sake, these numbers are manipulated via classic increment and
		 * decrement operations and thus are (as hinted by MSDN) not meant to be precise
		 */
		int numReadWaiters, numUpgradeWaiters, numWriteWaiters;
		bool disposed;

		static int idPool = int.MinValue;
		readonly int id = Interlocked.Increment (ref idPool);

		/* This dictionary is instanciated per thread for all existing ReaderWriterLockSlim instance.
		 * Each instance is defined by an internal integer id value used as a key in the dictionary.
		 * to avoid keeping unneeded reference to the instance and getting in the way of the GC.
		 * Since there is no LockCookie type here, all the useful per-thread infos concerning each
		 * instance are kept here.
		 */
		[ThreadStatic]
		static Dictionary<int, ThreadLockState> currentThreadState;

		/* Rwls tries to use this array as much as possible to quickly retrieve the thread-local
		 * informations so that it ends up being only an array lookup. When the number of thread
		 * using the instance goes past the length of the array, the code fallback to the normal
		 * dictionary
		 */
		ThreadLockState[] fastStateCache = new ThreadLockState[64];

		public ReaderWriterLockSlim () : this (LockRecursionPolicy.NoRecursion)
		{
		}

		public ReaderWriterLockSlim (LockRecursionPolicy recursionPolicy)
		{
			this.recursionPolicy = recursionPolicy;
			this.noRecursion = recursionPolicy == LockRecursionPolicy.NoRecursion;
		}

		public void EnterReadLock ()
		{
			TryEnterReadLock (-1);
		}

		public bool TryEnterReadLock (int millisecondsTimeout)
		{
			bool dummy = false;
			return TryEnterReadLock (millisecondsTimeout, ref dummy);
		}

		bool TryEnterReadLock (int millisecondsTimeout, ref bool success)
		{
			ThreadLockState ctstate = CurrentThreadState;

			if (CheckState (ctstate, millisecondsTimeout, LockState.Read)) {
				++ctstate.ReaderRecursiveCount;
				return true;
			}

			// This is downgrading from upgradable, no need for check since
			// we already have a sort-of read lock that's going to disappear
			// after user calls ExitUpgradeableReadLock.
			// Same idea when recursion is allowed and a write thread wants to
			// go for a Read too.
			if (ctstate.LockState.Has (LockState.Upgradable)
			    || (!noRecursion && ctstate.LockState.Has (LockState.Write))) {
				RuntimeHelpers.PrepareConstrainedRegions ();
				try {}
				finally {
					Interlocked.Add (ref rwlock, RwRead);
					ctstate.LockState |= LockState.Read;
					++ctstate.ReaderRecursiveCount;
					success = true;
				}

				return true;
			}
			
			++numReadWaiters;
			int val = 0;
			long start = millisecondsTimeout == -1 ? 0 : sw.ElapsedMilliseconds;

			do {
				/* Check if a writer is present (RwWrite) or if there is someone waiting to
				 * acquire a writer lock in the queue (RwWait | RwWaitUpgrade).
				 */
				if ((rwlock & (RwWrite | RwWait | RwWaitUpgrade)) > 0) {
					writerDoneEvent.Wait (ComputeTimeout (millisecondsTimeout, start));
					continue;
				}

				/* Optimistically try to add ourselves to the reader value
				 * if the adding was too late and another writer came in between
				 * we revert the operation.
				 */
				RuntimeHelpers.PrepareConstrainedRegions ();
				try {}
				finally {
					if (((val = Interlocked.Add (ref rwlock, RwRead)) & (RwWrite | RwWait | RwWaitUpgrade)) == 0) {
						/* If we are the first reader, reset the event to let other threads
						 * sleep correctly if they try to acquire write lock
						 */
						if (val >> RwReadBit == 1)
							readerDoneEvent.Reset ();

						ctstate.LockState ^= LockState.Read;
						++ctstate.ReaderRecursiveCount;
						--numReadWaiters;
						success = true;
					} else {
						Interlocked.Add (ref rwlock, -RwRead);
					}
				}
				if (success)
					return true;

				writerDoneEvent.Wait (ComputeTimeout (millisecondsTimeout, start));
			} while (millisecondsTimeout == -1 || (sw.ElapsedMilliseconds - start) < millisecondsTimeout);

			--numReadWaiters;
			return false;
		}

		public bool TryEnterReadLock (TimeSpan timeout)
		{
			return TryEnterReadLock (CheckTimeout (timeout));
		}

		public void ExitReadLock ()
		{
			RuntimeHelpers.PrepareConstrainedRegions ();
			try {}
			finally {
				ThreadLockState ctstate = CurrentThreadState;

				if (!ctstate.LockState.Has (LockState.Read))
					throw new SynchronizationLockException ("The current thread has not entered the lock in read mode");

				if (--ctstate.ReaderRecursiveCount == 0) {
					ctstate.LockState ^= LockState.Read;
					if (Interlocked.Add (ref rwlock, -RwRead) >> RwReadBit == 0)
						readerDoneEvent.Set ();
				}
			}
		}

		public void EnterWriteLock ()
		{
			TryEnterWriteLock (-1);
		}
		
		public bool TryEnterWriteLock (int millisecondsTimeout)
		{
			ThreadLockState ctstate = CurrentThreadState;

			if (CheckState (ctstate, millisecondsTimeout, LockState.Write)) {
				++ctstate.WriterRecursiveCount;
				return true;
			}

			++numWriteWaiters;
			bool isUpgradable = ctstate.LockState.Has (LockState.Upgradable);
			bool registered = false;
			bool success = false;

			RuntimeHelpers.PrepareConstrainedRegions ();
			try {
				/* If the code goes there that means we had a read lock beforehand
				 * that need to be suppressed, we also take the opportunity to register
				 * our interest in the write lock to avoid other write wannabe process
				 * coming in the middle
				 */
				if (isUpgradable && rwlock >= RwRead) {
					try {}
					finally {
						if (Interlocked.Add (ref rwlock, RwWaitUpgrade - RwRead) >> RwReadBit == 0)
							readerDoneEvent.Set ();
						registered = true;
					}
				}

				int stateCheck = isUpgradable ? RwWaitUpgrade + RwWait : RwWait;
				long start = millisecondsTimeout == -1 ? 0 : sw.ElapsedMilliseconds;
				int registration = isUpgradable ? RwWaitUpgrade : RwWait;

				do {
					int state = rwlock;

					if (state <= stateCheck) {
						try {}
						finally {
							var toWrite = state + RwWrite - (registered ? registration : 0);
							if (Interlocked.CompareExchange (ref rwlock, toWrite, state) == state) {
								writerDoneEvent.Reset ();
								ctstate.LockState ^= LockState.Write;
								++ctstate.WriterRecursiveCount;
								--numWriteWaiters;
								registered = false;
								success = true;
							}
						}
						if (success)
							return true;
					}

					state = rwlock;

					// We register our interest in taking the Write lock (if upgradeable it's already done)
					if (!isUpgradable) {
						while ((state & RwWait) == 0) {
							try {}
							finally {
								if (Interlocked.CompareExchange (ref rwlock, state | RwWait, state) == state)
									registered = true;
							}
							if (registered)
								break;
							state = rwlock;
						}
					}

					// Before falling to sleep
					do {
						if (rwlock <= stateCheck)
							break;
						if ((rwlock & RwWrite) != 0)
							writerDoneEvent.Wait (ComputeTimeout (millisecondsTimeout, start));
						else if ((rwlock >> RwReadBit) > 0)
							readerDoneEvent.Wait (ComputeTimeout (millisecondsTimeout, start));
					} while (millisecondsTimeout < 0 || (sw.ElapsedMilliseconds - start) < millisecondsTimeout);
				} while (millisecondsTimeout < 0 || (sw.ElapsedMilliseconds - start) < millisecondsTimeout);

				--numWriteWaiters;
			} finally {
				if (registered)
					Interlocked.Add (ref rwlock, isUpgradable ? -RwWaitUpgrade : -RwWait);
			}

			return false;
		}

		public bool TryEnterWriteLock (TimeSpan timeout)
		{
			return TryEnterWriteLock (CheckTimeout (timeout));
		}

		public void ExitWriteLock ()
		{
			RuntimeHelpers.PrepareConstrainedRegions ();
			try {}
			finally {
				ThreadLockState ctstate = CurrentThreadState;

				if (!ctstate.LockState.Has (LockState.Write))
					throw new SynchronizationLockException ("The current thread has not entered the lock in write mode");
			
				if (--ctstate.WriterRecursiveCount == 0) {
					bool isUpgradable = ctstate.LockState.Has (LockState.Upgradable);
					ctstate.LockState ^= LockState.Write;

					int value = Interlocked.Add (ref rwlock, isUpgradable ? RwRead - RwWrite : -RwWrite);
					writerDoneEvent.Set ();
					if (isUpgradable && value >> RwReadBit == 1)
						readerDoneEvent.Reset ();
				}
			}
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
			ThreadLockState ctstate = CurrentThreadState;

			if (CheckState (ctstate, millisecondsTimeout, LockState.Upgradable)) {
				++ctstate.UpgradeableRecursiveCount;
				return true;
			}

			if (ctstate.LockState.Has (LockState.Read))
				throw new LockRecursionException ("The current thread has already entered read mode");

			++numUpgradeWaiters;
			long start = millisecondsTimeout == -1 ? 0 : sw.ElapsedMilliseconds;
			bool taken = false;
			bool success = false;

			// We first try to obtain the upgradeable right
			try {
				while (!upgradableEvent.IsSet () || !taken) {
					try {}
					finally {
						taken = upgradableTaken.TryRelaxedSet ();
					}
					if (taken)
						break;
					if (millisecondsTimeout != -1 && (sw.ElapsedMilliseconds - start) > millisecondsTimeout) {
						--numUpgradeWaiters;
						return false;
					}

					upgradableEvent.Wait (ComputeTimeout (millisecondsTimeout, start));
				}

				upgradableEvent.Reset ();

				RuntimeHelpers.PrepareConstrainedRegions ();
				try {
					// Then it's a simple reader lock acquiring
					TryEnterReadLock (ComputeTimeout (millisecondsTimeout, start), ref success);
				} finally {
					if (success) {
						ctstate.LockState |= LockState.Upgradable;
						ctstate.LockState &= ~LockState.Read;
						--ctstate.ReaderRecursiveCount;
						++ctstate.UpgradeableRecursiveCount;
					} else {
						upgradableTaken.Value = false;
						upgradableEvent.Set ();
					}
				}

				--numUpgradeWaiters;
			} catch {
				// An async exception occured, if we had taken the upgradable mode, release it
				if (taken && !success)
					upgradableTaken.Value = false;
			}

			return success;
		}

		public bool TryEnterUpgradeableReadLock (TimeSpan timeout)
		{
			return TryEnterUpgradeableReadLock (CheckTimeout (timeout));
		}
	       
		public void ExitUpgradeableReadLock ()
		{
			RuntimeHelpers.PrepareConstrainedRegions ();
			try {}
			finally {
				ThreadLockState ctstate = CurrentThreadState;

				if (!ctstate.LockState.Has (LockState.Upgradable | LockState.Read))
					throw new SynchronizationLockException ("The current thread has not entered the lock in upgradable mode");

				if (--ctstate.UpgradeableRecursiveCount == 0) {
					upgradableTaken.Value = false;
					upgradableEvent.Set ();

					ctstate.LockState &= ~LockState.Upgradable;
					if (Interlocked.Add (ref rwlock, -RwRead) >> RwReadBit == 0)
						readerDoneEvent.Set ();
				}
			}

		}

		public void Dispose ()
		{
			if (disposed)
				return;

			if (IsReadLockHeld || IsUpgradeableReadLockHeld || IsWriteLockHeld)
				throw new SynchronizationLockException ("The lock is being disposed while still being used");

			disposed = true;
		}

		public bool IsReadLockHeld {
			get {
				return rwlock >= RwRead && CurrentThreadState.LockState.Has (LockState.Read);
			}
		}

		public bool IsWriteLockHeld {
			get {
				return (rwlock & RwWrite) > 0 && CurrentThreadState.LockState.Has (LockState.Write);
			}
		}
		
		public bool IsUpgradeableReadLockHeld {
			get {
				return upgradableTaken.Value && CurrentThreadState.LockState.Has (LockState.Upgradable);
			}
		}

		public int CurrentReadCount {
			get {
				return (rwlock >> RwReadBit) - (upgradableTaken.Value ? 1 : 0);
			}
		}
		
		public int RecursiveReadCount {
			get {
				return CurrentThreadState.ReaderRecursiveCount;
			}
		}

		public int RecursiveUpgradeCount {
			get {
				return CurrentThreadState.UpgradeableRecursiveCount;
			}
		}

		public int RecursiveWriteCount {
			get {
				return CurrentThreadState.WriterRecursiveCount;
			}
		}

		public int WaitingReadCount {
			get {
				return numReadWaiters;
			}
		}

		public int WaitingUpgradeCount {
			get {
				return numUpgradeWaiters;
			}
		}

		public int WaitingWriteCount {
			get {
				return numWriteWaiters;
			}
		}

		public LockRecursionPolicy RecursionPolicy {
			get {
				return recursionPolicy;
			}
		}

		ThreadLockState CurrentThreadState {
			get {
				int tid = Thread.CurrentThread.ManagedThreadId;

				return tid < fastStateCache.Length ?
					fastStateCache [tid] ?? (fastStateCache[tid] = new ThreadLockState ()) :
					GetGlobalThreadState (tid);
			}
		}

		ThreadLockState GetGlobalThreadState (int tid)
		{
			if (currentThreadState == null)
				Interlocked.CompareExchange (ref currentThreadState, new Dictionary<int, ThreadLockState> (), null);

			ThreadLockState state;
			if (!currentThreadState.TryGetValue (id, out state))
				currentThreadState [id] = state = new ThreadLockState ();

			return state;
		}

		bool CheckState (ThreadLockState state, int millisecondsTimeout, LockState validState)
		{
			if (disposed)
				throw new ObjectDisposedException ("ReaderWriterLockSlim");

			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			// Detect and prevent recursion
			LockState ctstate = state.LockState;

			if (ctstate != LockState.None && noRecursion && (!ctstate.Has (LockState.Upgradable) || validState == LockState.Upgradable))
				throw new LockRecursionException ("The current thread has already a lock and recursion isn't supported");

			if (noRecursion)
				return false;

			// If we already had right lock state, just return
			if (ctstate.Has (validState))
				return true;

			// In read mode you can just enter Read recursively
			if (ctstate == LockState.Read)
				throw new LockRecursionException ();				

			return false;
		}

		static int CheckTimeout (TimeSpan timeout)
		{
			try {
				return checked ((int)timeout.TotalMilliseconds);
			} catch (System.OverflowException) {
				throw new ArgumentOutOfRangeException ("timeout");
			}
		}

		static int ComputeTimeout (int millisecondsTimeout, long start)
		{
			return millisecondsTimeout == -1 ? -1 : (int)Math.Max (sw.ElapsedMilliseconds - start - millisecondsTimeout, 1);
		}
	}
}
