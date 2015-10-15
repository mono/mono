//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime
{
    using System.Threading;
    using System.Security;

    // IOThreadScheduler takes no locks due to contention problems on multiproc.
    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.NonBlocking)]
    class IOThreadScheduler
    {
        // Do not increase the maximum capacity above 32k!  It must be a power of two, 0x8000 or less, in order to
        // work with the strategy for 'headTail'.
        const int MaximumCapacity = 0x8000;

        [Fx.Tag.SecurityNote(Miscellaneous = "can be called outside user context")]
        static class Bits
        {
            public const int HiShift     = 32 / 2;

            public const int HiOne       = 1 << HiShift;
            public const int LoHiBit     = HiOne >> 1;
            public const int HiHiBit     = LoHiBit << HiShift;
            public const int LoCountMask = LoHiBit - 1;
            public const int HiCountMask = LoCountMask << HiShift;
            public const int LoMask      = LoCountMask | LoHiBit;
            public const int HiMask      = HiCountMask | HiHiBit;
            public const int HiBits      = LoHiBit | HiHiBit;

            public static int Count(int slot)
            {
                return ((slot >> HiShift) - slot + 2 & LoMask) - 1;
            }

            public static int CountNoIdle(int slot)
            {
                return (slot >> HiShift) - slot + 1 & LoMask;
            }

            public static int IncrementLo(int slot)
            {
                return slot + 1 & LoMask | slot & HiMask;
            }

            // This method is only valid if you already know that (gate & HiBits) != 0.
            public static bool IsComplete(int gate)
            {
                return (gate & HiMask) == gate << HiShift;
            }
        }

        static IOThreadScheduler current = new IOThreadScheduler(32, 32);
        readonly ScheduledOverlapped overlapped;

        [Fx.Tag.Queue(typeof(Slot), Scope = Fx.Tag.Strings.AppDomain)]
        [Fx.Tag.SecurityNote(Critical = "holds callbacks which get called outside of the app security context")]
        [SecurityCritical]
        readonly Slot[] slots;

        [Fx.Tag.Queue(typeof(Slot), Scope = Fx.Tag.Strings.AppDomain)]
        [Fx.Tag.SecurityNote(Critical = "holds callbacks which get called outside of the app security context")]
        [SecurityCritical]
        readonly Slot[] slotsLowPri;

        // This field holds both the head (HiWord) and tail (LoWord) indicies into the slot array.  This limits each
        // value to 64k.  In order to be able to distinguish wrapping the slot array (allowed) from wrapping the
        // indicies relative to each other (not allowed), the size of the slot array is limited by an additional bit
        // to 32k.
        //
        // The HiWord (head) holds the index of the last slot to have been scheduled into.  The LoWord (tail) holds
        // the index of the next slot to be dispatched from.  When the queue is empty, the LoWord will be exactly
        // one slot ahead of the HiWord.  When the two are equal, the queue holds one item.
        //
        // When the tail is *two* slots ahead of the head (equivalent to a count of -1), that means the IOTS is
        // idle.  Hence, we start out headTail with a -2 (equivalent) in the head and zero in the tail.
        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int headTail = -2 << Bits.HiShift;

        // This field is the same except that it governs the low-priority work items.  It doesn't have a concept
        // of idle (-2) so starts empty (-1).
        [Fx.Tag.SynchronizationObject(Blocking = false, Kind = Fx.Tag.SynchronizationKind.InterlockedNoSpin)]
        int headTailLowPri = -1 << Bits.HiShift;

        [Fx.Tag.SecurityNote(Critical = "creates a ScheduledOverlapped, touches slots, can be called outside of user context",
            Safe = "The scheduled overlapped is only used internally, and flows security.")]
        [SecuritySafeCritical]
        IOThreadScheduler(int capacity, int capacityLowPri)
        {
            Fx.Assert(capacity > 0, "Capacity must be positive.");
            Fx.Assert(capacity <= 0x8000, "Capacity cannot exceed 32k.");

            Fx.Assert(capacityLowPri > 0, "Low-priority capacity must be positive.");
            Fx.Assert(capacityLowPri <= 0x8000, "Low-priority capacity cannot exceed 32k.");

            this.slots = new Slot[capacity];
            Fx.Assert((this.slots.Length & SlotMask) == 0, "Capacity must be a power of two.");

            this.slotsLowPri = new Slot[capacityLowPri];
            Fx.Assert((this.slotsLowPri.Length & SlotMaskLowPri) == 0, "Low-priority capacity must be a power of two.");

            this.overlapped = new ScheduledOverlapped();
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into critical class CriticalHelper, doesn't flow context")]
        [SecurityCritical]
        public static void ScheduleCallbackNoFlow(Action<object> callback, object state)
        {
            if (callback == null)
            {
                throw Fx.Exception.ArgumentNull("callback");
            }

            bool queued = false;
            while (!queued)
            {
                try { } finally
                {
                    // Called in a finally because it needs to run uninterrupted in order to maintain consistency.
                    queued = IOThreadScheduler.current.ScheduleCallbackHelper(callback, state);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into critical class CriticalHelper, doesn't flow context")]
        [SecurityCritical]
        public static void ScheduleCallbackLowPriNoFlow(Action<object> callback, object state)
        {
            if (callback == null)
            {
                throw Fx.Exception.ArgumentNull("callback");
            }

            bool queued = false;
            while (!queued)
            {
                try { } finally
                {
                    // Called in a finally because it needs to run uninterrupted in order to maintain consistency.
                    queued = IOThreadScheduler.current.ScheduleCallbackLowPriHelper(callback, state);
                }
            }
        }

        // Returns true if successfully scheduled, false otherwise.
        [Fx.Tag.SecurityNote(Critical = "calls into ScheduledOverlapped to post it, touches slots, can be called outside user context.")]
        [SecurityCritical]
        bool ScheduleCallbackHelper(Action<object> callback, object state)
        {
            // See if there's a free slot.  Fortunately the overflow bit is simply lost.
            int slot = Interlocked.Add(ref this.headTail, Bits.HiOne);

            // If this brings us to 'empty', then the IOTS used to be 'idle'.  Remember that, and increment
            // again.  This doesn't need to be in a loop, because until we call Post(), we can't go back to idle.
            bool wasIdle = Bits.Count(slot) == 0;
            if (wasIdle)
            {
                slot = Interlocked.Add(ref this.headTail, Bits.HiOne);
                Fx.Assert(Bits.Count(slot) != 0, "IOTS went idle when it shouldn't have.");
            }

            // Check if we wrapped *around* to idle.
            if (Bits.Count(slot) == -1)
            {
                // Since the capacity is limited to 32k, this means we wrapped the array at least twice.  That's bad
                // because headTail no longer knows how many work items we have - it looks like zero.  This can
                // only happen if 32k threads come through here while one is swapped out.
                throw Fx.AssertAndThrowFatal("Head/Tail overflow!");
            }

            bool wrapped;
            bool queued = this.slots[slot >> Bits.HiShift & SlotMask].TryEnqueueWorkItem(callback, state, out wrapped);

            if (wrapped)
            {
                // Wrapped around the circular buffer.  Create a new, bigger IOThreadScheduler.
                IOThreadScheduler next =
                    new IOThreadScheduler(Math.Min(this.slots.Length * 2, MaximumCapacity), this.slotsLowPri.Length);
                Interlocked.CompareExchange<IOThreadScheduler>(ref IOThreadScheduler.current, next, this);
            }

            if (wasIdle)
            {
                // It's our responsibility to kick off the overlapped.
                this.overlapped.Post(this);
            }

            return queued;
        }

        // Returns true if successfully scheduled, false otherwise.
        [Fx.Tag.SecurityNote(Critical = "calls into ScheduledOverlapped to post it, touches slots, can be called outside user context.")]
        [SecurityCritical]
        bool ScheduleCallbackLowPriHelper(Action<object> callback, object state)
        {
            // See if there's a free slot.  Fortunately the overflow bit is simply lost.
            int slot = Interlocked.Add(ref this.headTailLowPri, Bits.HiOne);

            // If this is the first low-priority work item, make sure we're not idle.
            bool wasIdle = false;
            if (Bits.CountNoIdle(slot) == 1)
            {
                // Since Interlocked calls create a full thread barrier, this will read the value of headTail
                // at the time of the Interlocked.Add or later.  The invariant is that the IOTS is unidle at some
                // point after the Add.
                int ht = this.headTail;

                if (Bits.Count(ht) == -1)
                {
                    // Use a temporary local here to store the result of the Interlocked.CompareExchange.  This
                    // works around a codegen 
                    int interlockedResult = Interlocked.CompareExchange(ref this.headTail, ht + Bits.HiOne, ht);
                    if (ht == interlockedResult)
                    {
                        wasIdle = true;
                    }
                }
            }

            // Check if we wrapped *around* to empty.
            if (Bits.CountNoIdle(slot) == 0)
            {
                // Since the capacity is limited to 32k, this means we wrapped the array at least twice.  That's bad
                // because headTail no longer knows how many work items we have - it looks like zero.  This can
                // only happen if 32k threads come through here while one is swapped out.
                throw Fx.AssertAndThrowFatal("Low-priority Head/Tail overflow!");
            }

            bool wrapped;
            bool queued = this.slotsLowPri[slot >> Bits.HiShift & SlotMaskLowPri].TryEnqueueWorkItem(
                callback, state, out wrapped);

            if (wrapped)
            {
                IOThreadScheduler next =
                    new IOThreadScheduler(this.slots.Length, Math.Min(this.slotsLowPri.Length * 2, MaximumCapacity));
                Interlocked.CompareExchange<IOThreadScheduler>(ref IOThreadScheduler.current, next, this);
            }

            if (wasIdle)
            {
                // It's our responsibility to kick off the overlapped.
                this.overlapped.Post(this);
            }

            return queued;
        }

        [Fx.Tag.SecurityNote(Critical = "calls into ScheduledOverlapped to post it, touches slots, may be called outside of user context")]
        [SecurityCritical]
        void CompletionCallback(out Action<object> callback, out object state)
        {
            int slot = this.headTail;
            int slotLowPri;
            while (true)
            {
                Fx.Assert(Bits.Count(slot) != -1, "CompletionCallback called on idle IOTS!");

                bool wasEmpty = Bits.Count(slot) == 0;
                if (wasEmpty)
                {
                    // We're about to set this to idle.  First check the low-priority queue.  This alone doesn't
                    // guarantee we service all the low-pri items - there hasn't even been an Interlocked yet.  But
                    // we take care of that later.
                    slotLowPri = this.headTailLowPri;
                    while (Bits.CountNoIdle(slotLowPri) != 0)
                    {
                        if (slotLowPri == (slotLowPri = Interlocked.CompareExchange(ref this.headTailLowPri,
                            Bits.IncrementLo(slotLowPri), slotLowPri)))
                        {
                            this.overlapped.Post(this);
                            this.slotsLowPri[slotLowPri & SlotMaskLowPri].DequeueWorkItem(out callback, out state);
                            return;
                        }
                    }
                }

                if (slot == (slot = Interlocked.CompareExchange(ref this.headTail, Bits.IncrementLo(slot), slot)))
                {
                    if (!wasEmpty)
                    {
                        this.overlapped.Post(this);
                        this.slots[slot & SlotMask].DequeueWorkItem(out callback, out state);
                        return;
                    }

                    // We just set the IOThreadScheduler to idle.  Check if a low-priority item got added in the
                    // interim.
                    // Interlocked calls create a thread barrier, so this read will give us the value of
                    // headTailLowPri at the time of the interlocked that set us to idle, or later.  The invariant
                    // here is that either the low-priority queue was empty at some point after we set the IOTS to
                    // idle (so that the next enqueue will notice, and issue a Post), or that the IOTS was unidle at
                    // some point after we set it to idle (so that the next attempt to go idle will verify that the
                    // low-priority queue is empty).
                    slotLowPri = this.headTailLowPri;

                    if (Bits.CountNoIdle(slotLowPri) != 0)
                    {
                        // Whoops, go back from being idle (unless someone else already did).  If we go back, start
                        // over.  (We still owe a Post.)
                        slot = Bits.IncrementLo(slot);
                        if (slot == Interlocked.CompareExchange(ref this.headTail, slot + Bits.HiOne, slot))
                        {
                            slot += Bits.HiOne;
                            continue;
                        }

                        // We know that there's a low-priority work item.  But we also know that the IOThreadScheduler
                        // wasn't idle.  It's best to let it take care of itself, since according to this method, we
                        // just set the IOThreadScheduler to idle so shouldn't take on any tasks.
                    }

                    break;
                }
            }

            callback = null;
            state = null;
            return;
        }

        [Fx.Tag.SecurityNote(Critical = "touches slots, may be called outside of user context")]
        [SecurityCritical]
        bool TryCoalesce(out Action<object> callback, out object state)
        {
            int slot = this.headTail;
            int slotLowPri;
            while (true)
            {
                if (Bits.Count(slot) > 0)
                {
                    if (slot == (slot = Interlocked.CompareExchange(ref this.headTail, Bits.IncrementLo(slot), slot)))
                    {
                        this.slots[slot & SlotMask].DequeueWorkItem(out callback, out state);
                        return true;
                    }
                    continue;
                }

                slotLowPri = this.headTailLowPri;
                if (Bits.CountNoIdle(slotLowPri) > 0)
                {
                    if (slotLowPri == (slotLowPri = Interlocked.CompareExchange(ref this.headTailLowPri,
                        Bits.IncrementLo(slotLowPri), slotLowPri)))
                    {
                        this.slotsLowPri[slotLowPri & SlotMaskLowPri].DequeueWorkItem(out callback, out state);
                        return true;
                    }
                    slot = this.headTail;
                    continue;
                }

                break;
            }

            callback = null;
            state = null;
            return false;
        }

        int SlotMask
        {
            [Fx.Tag.SecurityNote(Critical = "touches slots, may be called outside of user context")]
            [SecurityCritical]
            get
            {
                return this.slots.Length - 1;
            }
        }

        int SlotMaskLowPri
        {
            [Fx.Tag.SecurityNote(Critical = "touches slots, may be called outside of user context")]
            [SecurityCritical]
            get
            {
                return this.slotsLowPri.Length - 1;
            }
        }

        //


        ~IOThreadScheduler()
        {
            // If the AppDomain is shutting down, we may still have pending ops.  The AppDomain shutdown will clean
            // everything up.
            if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
#if DEBUG
                DebugVerifyHeadTail();
#endif
                Cleanup();
            }
        }

        [SecuritySafeCritical]
        void Cleanup()
        {
            if (this.overlapped != null)
            {
                this.overlapped.Cleanup();
            }
        }

#if DEBUG

        [SecuritySafeCritical]
        private void DebugVerifyHeadTail()
        {
            if (this.slots != null)
            {
                // The headTail value could technically be zero if the constructor was aborted early.  The
                // constructor wasn't aborted early if the slot array got created.
                Fx.Assert(Bits.Count(this.headTail) == -1, "IOTS finalized while not idle.");

                for (int i = 0; i < this.slots.Length; i++)
                {
                    this.slots[i].DebugVerifyEmpty();
                }
            }

            if (this.slotsLowPri != null)
            {
                Fx.Assert(Bits.CountNoIdle(this.headTailLowPri) == 0, "IOTS finalized with low-priority items queued.");

                for (int i = 0; i < this.slotsLowPri.Length; i++)
                {
                    this.slotsLowPri[i].DebugVerifyEmpty();
                }
            }
        }

#endif 

        // TryEnqueueWorkItem and DequeueWorkItem use the slot's 'gate' field for synchronization.  Because the
        // slot array is circular and there are no locks, we must assume that multiple threads can be entering each
        // method simultaneously.  If the first DequeueWorkItem occurs before the first TryEnqueueWorkItem, the
        // sequencing (and the enqueue) fails.
        //
        // The gate is a 32-bit int divided into four fields.  The bottom 15 bits (0x00007fff) are the count of
        // threads that have entered TryEnqueueWorkItem.  The first thread to enter is the one responsible for
        // filling the slot with work.  The 16th bit (0x00008000) is a flag indicating that the slot has been
        // successfully filled.  Only the first thread to enter TryEnqueueWorkItem can set this flag.  The
        // high-word (0x7fff0000) is the count of threads entering DequeueWorkItem.  The first thread to enter
        // is the one responsible for accepting (and eventually dispatching) the work in the slot.  The
        // high-bit (0x80000000) is a flag indicating that the slot has been successfully emptied.
        //
        // When the low-word and high-work counters are equal, and both bit flags have been set, the gate is considered
        // 'complete' and can be reset back to zero.  Any operation on the gate might bring it to this state.
        // It's the responsibility of the thread that brings the gate to a completed state to reset it to zero.
        // (It's possible that the gate will fall out of the completed state before it can be reset - that's ok,
        // the next time it becomes completed it can be reset.)
        //
        // It's unlikely either count will ever go higher than 2 or 3.
        //
        // The value of 'callback' has these properties:
        //   -  When the gate is zero, callback is null.
        //   -  When the low-word count is non-zero, but the 0x8000 bit is unset, callback is writable by the thread
        //      that incremented the low word to 1.  Its value is undefined for other threads.  The thread that
        //      sets callback is responsible for setting the 0x8000 bit when it's done.
        //   -  When the 0x8000 bit is set and the high-word count is zero, callback is valid.  (It may be null.)
        //   -  When the 0x8000 bit is set, the high-word count is non-zero, and the high bit is unset, callback is
        //      writable by the thread that incremented the high word to 1 *or* the thread that set the 0x8000 bit,
        //      whichever happened last.  That thread can read the value and set callback to null.  Its value is
        //      undefined for other threads.  The thread that clears the callback is responsible for setting the
        //      high bit.
        //   -  When the high bit is set, callback is null.
        //   -  It's illegal for the gate to be in a state that would satisfy more than one of these conditions.
        //   -  The state field follows the same rules as callback.
        struct Slot
        {
            int gate;
            Action<object> callback;
            object state;

            [Fx.Tag.SecurityNote(Miscellaneous = "called by critical code, can be called outside user context")]
            public bool TryEnqueueWorkItem(Action<object> callback, object state, out bool wrapped)
            {
                // Register our arrival and check the state of this slot.  If the slot was already full, we wrapped.
                int gateSnapshot = Interlocked.Increment(ref this.gate);
                wrapped = (gateSnapshot & Bits.LoCountMask) != 1;
                if (wrapped)
                {
                    if ((gateSnapshot & Bits.LoHiBit) != 0 && Bits.IsComplete(gateSnapshot))
                    {
                        Interlocked.CompareExchange(ref this.gate, 0, gateSnapshot);
                    }
                    return false;
                }

                Fx.Assert(this.callback == null, "Slot already has a work item.");
                Fx.Assert((gateSnapshot & Bits.HiBits) == 0, "Slot already marked.");

                this.state = state;
                this.callback = callback;

                // Set the special bit to show that the slot is filled.
                gateSnapshot = Interlocked.Add(ref this.gate, Bits.LoHiBit);
                Fx.Assert((gateSnapshot & Bits.HiBits) == Bits.LoHiBit, "Slot already empty.");

                if ((gateSnapshot & Bits.HiCountMask) == 0)
                {
                    // Good - no one has shown up looking for this work yet.
                    return true;
                }

                // Oops - someone already came looking for this work.  We have to abort and reschedule.
                this.state = null;
                this.callback = null;

                // Indicate that the slot is clear.  We might be able to bypass setting the high bit.
                if (gateSnapshot >> Bits.HiShift != (gateSnapshot & Bits.LoCountMask) ||
                    Interlocked.CompareExchange(ref this.gate, 0, gateSnapshot) != gateSnapshot)
                {
                    gateSnapshot = Interlocked.Add(ref this.gate, Bits.HiHiBit);
                    if (Bits.IsComplete(gateSnapshot))
                    {
                        Interlocked.CompareExchange(ref this.gate, 0, gateSnapshot);
                    }
                }

                return false;
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "called by critical code, can be called outside user context")]
            public void DequeueWorkItem(out Action<object> callback, out object state)
            {
                // Stake our claim on the item.
                int gateSnapshot = Interlocked.Add(ref this.gate, Bits.HiOne);

                if ((gateSnapshot & Bits.LoHiBit) == 0)
                {
                    // Whoops, a ----.  The work item hasn't made it in yet.  In this context, returning a null callback
                    // is treated like a degenrate work item (rather than an empty queue).  The enqueuing thread will
                    // notice this ---- and reschedule the real work in a new slot.  Do not reset the slot to zero,
                    // since it's still going to get enqueued into.  (The enqueueing thread will reset it.)
                    callback = null;
                    state = null;
                    return;
                }

                // If we're the first, we get to do the work.
                if ((gateSnapshot & Bits.HiCountMask) == Bits.HiOne)
                {
                    callback = this.callback;
                    state = this.state;
                    this.state = null;
                    this.callback = null;

                    // Indicate that the slot is clear.
                    // We should be able to bypass setting the high-bit in the common case.
                    if ((gateSnapshot & Bits.LoCountMask) != 1 ||
                        Interlocked.CompareExchange(ref this.gate, 0, gateSnapshot) != gateSnapshot)
                    {
                        gateSnapshot = Interlocked.Add(ref this.gate, Bits.HiHiBit);
                        if (Bits.IsComplete(gateSnapshot))
                        {
                            Interlocked.CompareExchange(ref this.gate, 0, gateSnapshot);
                        }
                    }
                }
                else
                {
                    callback = null;
                    state = null;

                    // If we're the last, we get to reset the slot.
                    if (Bits.IsComplete(gateSnapshot))
                    {
                        Interlocked.CompareExchange(ref this.gate, 0, gateSnapshot);
                    }
                }
            }

#if DEBUG
            public void DebugVerifyEmpty()
            {
                Fx.Assert(this.gate == 0, "Finalized with unfinished slot.");
                Fx.Assert(this.callback == null, "Finalized with leaked callback.");
                Fx.Assert(this.state == null, "Finalized with leaked state.");
            }
#endif
        }

        // A note about the IOThreadScheduler and the ScheduledOverlapped references:
        // Although for each scheduler we have a single instance of overlapped, we cannot point to the scheduler from the
        // overlapped, through the entire lifetime of the overlapped. This is because the ScheduledOverlapped is pinned
        // and if it has a reference to the IOTS, it would be rooted and the finalizer will never get called.
        // Therefore, we are passing the reference, when we post a pending callback and reset it, once the callback was
        // invoked; during that time the scheduler is rooted but in that time we don't want that it would be collected
        // by the GC anyway.
        [Fx.Tag.SecurityNote(Critical = "manages NativeOverlapped instance, can be called outside user context")]
        [SecurityCritical]
        unsafe class ScheduledOverlapped
        {
            readonly NativeOverlapped* nativeOverlapped;
            IOThreadScheduler scheduler;

            public ScheduledOverlapped()
            {
                this.nativeOverlapped = (new Overlapped()).UnsafePack(
                    Fx.ThunkCallback(new IOCompletionCallback(IOCallback)), null);
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "note that in some hosts this runs without any user context on the stack")]
            void IOCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                // Unhook the IOThreadScheduler ASAP to prevent it from leaking.
                IOThreadScheduler iots = this.scheduler;
                this.scheduler = null;
                Fx.Assert(iots != null, "Overlapped completed without a scheduler.");

                Action<object> callback;
                object state;
                try { } finally
                {
                    // Called in a finally because it needs to run uninterrupted in order to maintain consistency.
                    iots.CompletionCallback(out callback, out state);
                }

                bool found = true;
                while (found)
                {
                    // The callback can be null if synchronization misses result in unsuable slots.  Keep going onto
                    // the next slot in such cases until there are no more slots.
                    if (callback != null)
                    {
                        callback(state);
                    }

                    try { } finally
                    {
                        // Called in a finally because it needs to run uninterrupted in order to maintain consistency.
                        found = iots.TryCoalesce(out callback, out state);
                    }
                }
            }

            public void Post(IOThreadScheduler iots)
            {
                Fx.Assert(this.scheduler == null, "Post called on an overlapped that is already posted.");
                Fx.Assert(iots != null, "Post called with a null scheduler.");

                this.scheduler = iots;
                ThreadPool.UnsafeQueueNativeOverlapped(this.nativeOverlapped);
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "note that this runs on the finalizer thread")]
            public void Cleanup()
            {
                if (this.scheduler != null)
                {
                    throw Fx.AssertAndThrowFatal("Cleanup called on an overlapped that is in-flight.");
                }
                Overlapped.Free(this.nativeOverlapped);
            }
        }
    }
}
