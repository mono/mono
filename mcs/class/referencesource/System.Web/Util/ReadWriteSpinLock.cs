//------------------------------------------------------------------------------
// <copyright file="ReadWriteSpinLock.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


namespace System.Web.Util {

using System.Threading;
using System.Collections;
using System.Globalization;
using Microsoft.Win32;

struct ReadWriteSpinLock {
    // 
    // Fields
    // 

    //  _bits is layed out as follows:
    //
    //   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
    //   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
    //  +-+-+---------------------------+--------------------------------+
    //  |S|W|          WriteLockCount   |           ReadLockCount        |
    //  +-+-+---------------------------+--------------------------------+
    //  where
    //
    //      S - sign bit (always zero) - By having a sign bit, operations 
    //      on the ReadLockCount can use InterlockedIncrement/Decrement
    //
    //      W - writer waiting bit - set by threads attempting write lock, preventing
    //          any further threads from acquiring read locks.  This attempts to hint
    //          that updates have priority, but doesn't guarantee priority.
    //
    //      WriteLockCount - Write lock recursion count
    //
    //      ReadLockCount - Read lock recursion count
    //
    int     _bits;
    int     _id;

    //
    // Statics
    //

    static bool s_disableBusyWaiting = (SystemInfo.GetNumProcessCPUs() == 1);

    // 
    // Constants
    // 

    const int BACK_OFF_FACTORS_LENGTH = 13;
    static readonly double [] s_backOffFactors = new double [BACK_OFF_FACTORS_LENGTH] {
        1.020, 0.965,  0.890, 1.065,
        1.025, 1.115,  0.940, 0.995,
        1.050, 1.080,  0.915, 0.980,
        1.010
    };

    const int WRITER_WAITING_MASK   = (int) 0x40000000;
    const int WRITE_COUNT_MASK      = (int) 0x3FFF0000;
    const int READ_COUNT_MASK       = (int) 0x0000FFFF;
    const int WRITER_WAITING_SHIFT  = 30;           
    const int WRITE_COUNT_SHIFT     = 16;           

    static bool WriterWaiting(int bits)             {return ((bits & WRITER_WAITING_MASK) != 0);}
    static int  WriteLockCount(int bits)            {return ((bits & WRITE_COUNT_MASK) >> WRITE_COUNT_SHIFT);}
    static int  ReadLockCount(int bits)             {return (bits & READ_COUNT_MASK);}
    static bool NoWriters(int bits)                 {return ((bits & WRITE_COUNT_MASK) == 0);}
    static bool NoWritersOrWaitingWriters(int bits) {return ((bits & (WRITE_COUNT_MASK | WRITER_WAITING_MASK)) == 0);}
    static bool NoLocks(int bits)                   {return ((bits & ~WRITER_WAITING_MASK) == 0);}

    bool        WriterWaiting()                     {return WriterWaiting(_bits);}             
    int         WriteLockCount()                    {return WriteLockCount(_bits);}            
    int         ReadLockCount()                     {return ReadLockCount(_bits);}             
    bool        NoWriters()                         {return NoWriters(_bits);}                 
    bool        NoWritersOrWaitingWriters()         {return NoWritersOrWaitingWriters(_bits);} 
    bool        NoLocks()                           {return NoLocks(_bits);}                   

    int CreateNewBits(bool writerWaiting, int writeCount, int readCount) {
        int bits = ((writeCount << WRITE_COUNT_SHIFT) | readCount);
        if (writerWaiting) {
            bits |= WRITER_WAITING_MASK;
        }

        return bits;
    }


    internal /*public*/ void AcquireReaderLock() {

        // This lock supports Writelock then Readlock 
        // from the same thread (possibly from different functions).
        int threadId = Thread.CurrentThread.GetHashCode();

        // Optimize for the common case by 
        if (_TryAcquireReaderLock(threadId))
            return;

        _Spin(true, threadId);
        Debug.Trace("Spinlock", "AcquireReaderLock: _bits=" + _bits.ToString("x8", CultureInfo.InvariantCulture)
                    + " _id= " + _id.ToString("x8", CultureInfo.InvariantCulture));
    }


    internal /*public*/ void AcquireWriterLock() {

        int threadId = Thread.CurrentThread.GetHashCode();

        // Optimize for the common case by 
        if (_TryAcquireWriterLock(threadId))
            return;

        _Spin(false, threadId);

        Debug.Trace("Spinlock", "AcquireWriterLock: _bits=" + _bits.ToString("x8", CultureInfo.InvariantCulture)
                    + " _id= " + _id.ToString("x8", CultureInfo.InvariantCulture));
    }


    internal /*public*/ void ReleaseReaderLock() {
#if DBG 
        int id = _id;
        Debug.Assert(id == 0 || id == Thread.CurrentThread.GetHashCode(), "id == 0 || id == Thread.CurrentThread.GetHashCode()");
#endif

        int n = Interlocked.Decrement(ref _bits);

        Debug.Assert(n >= 0, "n >= 0");
        Debug.Trace("Spinlock", "ReleaseReaderLock: _bits=" + _bits.ToString("x8", CultureInfo.InvariantCulture)
                    + " _id= " + _id.ToString("x8", CultureInfo.InvariantCulture));
    }


    void AlterWriteCountHoldingWriterLock(int oldBits, int delta) {
        int readLockCount = ReadLockCount(oldBits);
        int oldWriteLockCount = WriteLockCount(oldBits);
        int newWriteLockCount = oldWriteLockCount + delta;
        Debug.Assert(newWriteLockCount >= 0, "newWriteLockCount >= 0");
        int newBits;
        int test;
    
        for (;;) {
            //
            // Since we own the lock, the only change that can be 
            // made by another thread to _bits is to add the writer-waiting bit.
            //
            Debug.Assert(WriteLockCount(oldBits) == oldWriteLockCount, "WriteLockCount(oldBits) == oldWriteLockCount");
            Debug.Assert(ReadLockCount(oldBits) == readLockCount, "ReadLockCount(oldBits) == readLockCount");
            newBits = CreateNewBits(WriterWaiting(oldBits), newWriteLockCount, readLockCount);
            test = Interlocked.CompareExchange(ref _bits, newBits, oldBits);
            if (test == oldBits) {
                break;
            }
    
            oldBits = test;
        }
    }
    
    internal /*public*/ void ReleaseWriterLock() {
#if DBG
        int id = _id;
        Debug.Assert(id == Thread.CurrentThread.GetHashCode(), "id == Thread.CurrentThread.GetHashCode()");
#endif
    
        int oldBits = _bits;
        int writeLockCount = WriteLockCount(oldBits);
        Debug.Assert(writeLockCount > 0, "writeLockCount > 0");
        if (writeLockCount == 1) {
            // Reset the id before releasing count so that
            // AcquireRead works correctly.
            _id = 0;
        }
    
        AlterWriteCountHoldingWriterLock(oldBits, -1);
        Debug.Trace("Spinlock", "ReleaseWriterLock: _bits=" + _bits.ToString("x8", CultureInfo.InvariantCulture) 
                    + " _id= " + _id.ToString("x8", CultureInfo.InvariantCulture));
    }


    bool _TryAcquireWriterLock(int threadId) {
        int id = _id;
        int oldBits = _bits;
        int newBits;
        int test;

        if (id == threadId) {
            // we can just pound in the correct value
            AlterWriteCountHoldingWriterLock(oldBits, +1);
            return true;
        }

        if (id == 0 && NoLocks(oldBits)) {
            newBits = CreateNewBits(false, 1, 0);
            test = Interlocked.CompareExchange(ref _bits, newBits, oldBits);
            if (test == oldBits) {
                id = _id;
                Debug.Assert(id == 0);
                _id = threadId;

                return true;
            }

            oldBits = test;
        }

        // If there is contention, make sure the WRITER_WAITING bit is set.
        // Note: this blocks readers from using a value that is about to be changed
        if (!WriterWaiting(oldBits)) {
            // hammer on _bits until the bit is set
            for (;;) {
                newBits = (oldBits | WRITER_WAITING_MASK);
                test = Interlocked.CompareExchange(ref _bits, newBits, oldBits);
                if (test == oldBits)
                    break;

                oldBits = test;
            }
        }

        return false;
    }

    bool _TryAcquireReaderLock(int threadId) {
        int oldBits = _bits;
        int id = _id;

        if (id == 0) {
            if (!NoWriters(oldBits)) {
                return false;
            }
        }
        else if (id != threadId) {
            return false;
        }

        if (Interlocked.CompareExchange(ref _bits, oldBits + 1, oldBits) == oldBits) {
            return true;
        }

        return false;
    }



    /// <internalonly/>
    void _Spin(bool isReaderLock, int threadId) {

        const int LOCK_MAXIMUM_SPINS =      10000;    // maximum allowable spin count
        const int LOCK_DEFAULT_SPINS =       4000;    // default spin count
        const int LOCK_MINIMUM_SPINS =        100;    // minimum allowable spin count

        int sleepTime = 0;
        int baseSpins;

        {   // limit scope of temp. stack vars to calculation of baseSpin2

            // Alternatives for threadId include a static counter
            // or the low DWORD of QueryPerformanceCounter().
            double randomBackoffFactor = s_backOffFactors[Math.Abs(threadId) % BACK_OFF_FACTORS_LENGTH];
            baseSpins = (int)(LOCK_DEFAULT_SPINS * randomBackoffFactor);
            baseSpins = Math.Min(LOCK_MAXIMUM_SPINS, baseSpins);
            baseSpins = Math.Max(baseSpins, LOCK_MINIMUM_SPINS);
        }

        DateTime utcSpinStartTime = DateTime.UtcNow; // error if struct not initialized

        // hand-optimize loop: Increase locality by copying static variables 
        // onto the stack (this will reduce cache misses after a contact 
        // switch induced by Sleep()).
        bool disableBusyWaiting = s_disableBusyWaiting;

        for (;;) {
            if (isReaderLock) {
                if (_TryAcquireReaderLock(threadId)) {
                    break;
                }
            }
            else {
                if (_TryAcquireWriterLock(threadId)) {
                    break;
                }
            }

            // if 1 cpu, or cpu affinity is set to 1, spinning is a waste of time
            if (disableBusyWaiting) {

                Thread.Sleep(sleepTime);

                // Avoid priority inversion: 0, 1, 0, 1,...
                sleepTime ^= 1;
            }
            else {
                int spinCount = baseSpins;

                // Check no more than baseSpins times then yield.
                // It is important not to use the InterlockedExchange in the
                // inner loop in order to minimize system memory bus traffic.
                for(;;) {
                    //
                    // If the lock is available break spinning and 
                    // try to obtain it.
                    //
                    if (isReaderLock) {
                        if (NoWritersOrWaitingWriters()) {
                            break;
                        }
                    }
                    else {
                        if (NoLocks()) {
                            break;
                        }
                    }

                    if (--spinCount < 0) {

                        Thread.Sleep(sleepTime);

                        // Backoff algorithm: reduce (or increase) busy wait time
                        baseSpins /= 2;
                        // LOCK_MINIMUM_SPINS <= baseSpins <= LOCK_MAXIMUM_SPINS
                        //baseSpins = Math.Min(LOCK_MAXIMUM_SPINS, baseSpins); //= min(LOCK_MAXIMUM_SPINS, baseSpins)
                        baseSpins = Math.Max(baseSpins, LOCK_MINIMUM_SPINS); //= max(baseSpins, LOCK_MINIMUM_SPINS);
                        spinCount = baseSpins;

                        // Using Sleep(0) leads to the possibility of priority
                        // inversion.  Sleep(0) only yields the processor if
                        // there's another thread of the same priority that's
                        // ready to run.  If a high-priority thread is trying to
                        // acquire the lock, which is held by a low-priority
                        // thread, then the low-priority thread may never get
                        // scheduled and hence never free the lock.  NT attempts
                        // to avoid priority inversions by temporarily boosting
                        // the priority of low-priority runnable threads, but the
                        // problem can still occur if there's a medium-priority
                        // thread that's always runnable.  If Sleep(1) is used,
                        // then the thread unconditionally yields the CPU.  We
                        // only do this for the second and subsequent even
                        // iterations, since a millisecond is a long time to wait
                        // if the thread can be scheduled in again sooner
                        // (~100,000 instructions).
                        // Avoid priority inversion: 0, 1, 0, 1,...
                        sleepTime ^= 1;
                    }
                    else {
                        // kill about 20 clock cycles on this proc
                        Thread.SpinWait(10);
                    }
                }

            }
        }// while

    }// _Spin

} // ReadWriteSpinLock

} // namespace System.Web.Util

// NOTES:
//
// This ReaderWriterSpinlock is a combination of the 
// original lightweight (4 byte) System.Web.Util.ReadWriteSpinLock
// and the lightweight (4 byte) exclusive lock (SmallSpinLock) used 
// in the George Reilly's LKRHash (see http://georgere/work/lkrhash).
//
// In an effort to support reentrancy during writes we are squirreling 
// away the thread id of the thread holding the write lock into the upper 
// 16 bits of the lock count.  This is possible as long as thread ids stay
// smaller than 7FFF.  Anything higher than that would flip the sign bit
// and we'd no longer be able to do signed comparisons to check 
// for read vs. write.  
//
//          read            write
// lower    #read locks     #write locks (from same thread)
// higher   0x0000          thread id of thread holding lock
//
// Adapted from LKRHash's lock.cpp, from GeorgeRe
// The original implementation is due to PALarson.

