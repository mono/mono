//------------------------------------------------------------------------------
// <copyright file="_TimerThread.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System.Collections;
    using System.Globalization;
    using System.Threading;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// <para>Acts as countdown timer, used to measure elapsed time over a [....] operation.</para>
    /// </summary>
    internal static class TimerThread {
        /// <summary>
        /// <para>Represents a queue of timers, which all have the same duration.</para>
        /// </summary>
        internal abstract class Queue {
            private readonly int m_DurationMilliseconds;

            internal Queue(int durationMilliseconds) {
                m_DurationMilliseconds = durationMilliseconds;
            }

            /// <summary>
            /// <para>The duration in milliseconds of timers in this queue.</para>
            /// </summary>
            internal int Duration {
                get {
                    return m_DurationMilliseconds;
                }
            }

            /// <summary>
            /// <para>Creates and returns a handle to a new polled timer.</para>
            /// </summary>
            internal Timer CreateTimer() {
                return CreateTimer(null, null);
            }

            /*
            // Consider removing.
            /// <summary>
            /// <para>Creates and returns a handle to a new timer.</para>
            /// </summary>
            internal Timer CreateTimer(Callback callback) {
                return CreateTimer(callback, null);
            }
            */

            /// <summary>
            /// <para>Creates and returns a handle to a new timer with attached context.</para>
            /// </summary>
            internal abstract Timer CreateTimer(Callback callback, object context);
        }

        /// <summary>
        /// <para>Represents a timer and provides a mechanism to cancel.</para>
        /// </summary>
        internal abstract class Timer : IDisposable
        {
            private readonly int m_StartTimeMilliseconds;
            private readonly int m_DurationMilliseconds;

            internal Timer(int durationMilliseconds) {
                m_DurationMilliseconds = durationMilliseconds;
                m_StartTimeMilliseconds = Environment.TickCount;
            }

            /// <summary>
            /// <para>The duration in milliseconds of timer.</para>
            /// </summary>
            internal int Duration {
                get {
                    return m_DurationMilliseconds;
                }
            }

            /// <summary>
            /// <para>The time (relative to Environment.TickCount) when the timer started.</para>
            /// </summary>
            internal int StartTime {
                get {
                    return m_StartTimeMilliseconds;
                }
            }

            /// <summary>
            /// <para>The time (relative to Environment.TickCount) when the timer will expire.</para>
            /// </summary>
            internal int Expiration {
                get {
                    return unchecked(m_StartTimeMilliseconds + m_DurationMilliseconds);
                }
            }

            /*
            // Consider removing.
            /// <summary>
            /// <para>The amount of time the timer has been running.  If it equals Duration, it has fired.  1 less means it has expired but
            /// not yet fired.  Int32.MaxValue is the ceiling - the actual value could be longer.  In the case of infinite timers, this
            /// value becomes unreliable when TickCount wraps (about 46 days).</para>
            /// </summary>
            internal int Elapsed {
                get {
                    if (HasExpired || Duration == 0) {
                        return Duration;
                    }

                    int now = Environment.TickCount;
                    if (Duration == TimeoutInfinite)
                    {
                        return (int) (Math.Min((uint) unchecked(now - StartTime), (uint) Int32.MaxValue);
                    }
                    else
                    {
                        return (IsTickBetween(StartTime, Expiration, now) && Duration > 1) ?
                            (int) (Math.Min((uint) unchecked(now - StartTime), Duration - 2) : Duration - 1;
                    }
                }
            } 
            */

            /// <summary>
            /// <para>The amount of time left on the timer.  0 means it has fired.  1 means it has expired but
            /// not yet fired.  -1 means infinite.  Int32.MaxValue is the ceiling - the actual value could be longer.</para>
            /// </summary>
            internal int TimeRemaining {
                get {
                    if (HasExpired) {
                        return 0;
                    }

                    if (Duration == Timeout.Infinite) {
                        return Timeout.Infinite;
                    }

                    int now = Environment.TickCount;
                    int remaining = IsTickBetween(StartTime, Expiration, now) ?
                        (int) (Math.Min((uint) unchecked(Expiration - now), (uint) Int32.MaxValue)) : 0;
                    return remaining < 2 ? remaining + 1 : remaining;
                }
            }

            /// <summary>
            /// <para>Cancels the timer.  Returns true if the timer hasn't and won't fire; false if it has or will.</para>
            /// </summary>
            internal abstract bool Cancel();

            /// <summary>
            /// <para>Whether or not the timer has expired.</para>
            /// </summary>
            internal abstract bool HasExpired { get; }

            public void Dispose()
            {
                Cancel();
            }
        }

        /// <summary>
        /// <para>Prototype for the callback that is called when a timer expires.</para>
        /// </summary>
        internal delegate void Callback(Timer timer, int timeNoticed, object context);

        private const int c_ThreadIdleTimeoutMilliseconds = 30 * 1000;
        private const int c_CacheScanPerIterations = 32;
        private const int c_TickCountResolution = 15;

        private static LinkedList<WeakReference> s_Queues = new LinkedList<WeakReference>();
        private static LinkedList<WeakReference> s_NewQueues = new LinkedList<WeakReference>();
        private static int s_ThreadState = (int) TimerThreadState.Idle;  // Really a TimerThreadState, but need an int for Interlocked.
        private static AutoResetEvent s_ThreadReadyEvent = new AutoResetEvent(false);
        private static ManualResetEvent s_ThreadShutdownEvent = new ManualResetEvent(false);
        private static WaitHandle[] s_ThreadEvents;
        private static int s_CacheScanIteration;
        private static Hashtable s_QueuesCache = new Hashtable();

        static TimerThread() {
            s_ThreadEvents = new WaitHandle[] { s_ThreadShutdownEvent, s_ThreadReadyEvent };
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(OnDomainUnload);
        }

        /// <summary>
        /// <para>The possible states of the timer thread.</para>
        /// </summary>
        private enum TimerThreadState {
            Idle,
            Running,
            Stopped
        }

        /// <summary>
        /// <para>The main external entry-point, allows creating new timer queues.</para>
        /// </summary>
        internal static Queue CreateQueue(int durationMilliseconds)
        {
            if (durationMilliseconds == Timeout.Infinite) {
                return new InfiniteTimerQueue();
            }

            if (durationMilliseconds < 0) {
                throw new ArgumentOutOfRangeException("durationMilliseconds");
            }

            // Queues are held with a weak reference so they can simply be abandoned and automatically cleaned up.
            TimerQueue queue;
            lock(s_NewQueues) {
                queue = new TimerQueue(durationMilliseconds);
                WeakReference weakQueue = new WeakReference(queue);
                s_NewQueues.AddLast(weakQueue);
            }

            return queue;
        }

        /// <summary>
        /// <para>Alternative cache-based queue factory.  Always synchronized.</para>
        /// </summary>
        internal static Queue GetOrCreateQueue(int durationMilliseconds) {
            if (durationMilliseconds == Timeout.Infinite) {
                return new InfiniteTimerQueue();
            }

            if (durationMilliseconds < 0) {
                throw new ArgumentOutOfRangeException("durationMilliseconds");
            }

            TimerQueue queue;
            WeakReference weakQueue = (WeakReference) s_QueuesCache[durationMilliseconds];
            if (weakQueue == null || (queue = (TimerQueue) weakQueue.Target) == null) {
                lock(s_NewQueues) {
                    weakQueue = (WeakReference) s_QueuesCache[durationMilliseconds];
                    if (weakQueue == null || (queue = (TimerQueue) weakQueue.Target) == null) {
                        queue = new TimerQueue(durationMilliseconds);
                        weakQueue = new WeakReference(queue);
                        s_NewQueues.AddLast(weakQueue);
                        s_QueuesCache[durationMilliseconds] = weakQueue;

                        // Take advantage of this lock to periodically scan the table for garbage.
                        if (++s_CacheScanIteration % c_CacheScanPerIterations == 0) {
                            List<int> garbage = new List<int>();
                            foreach (DictionaryEntry pair in s_QueuesCache) {
                                if (((WeakReference) pair.Value).Target == null) {
                                    garbage.Add((int) pair.Key);
                                }
                            }
                            for (int i = 0; i < garbage.Count; i++) {
                                s_QueuesCache.Remove(garbage[i]);
                            }
                        }
                    }
                }
            }

            return queue;
        }

        /// <summary>
        /// <para>Represents a queue of timers of fixed duration.</para>
        /// </summary>
        private class TimerQueue : Queue {
            // This is a GCHandle that holds onto the TimerQueue when active timers are in it.
            // The TimerThread only holds WeakReferences to it so that it can be collected when the user lets go of it.
            // But we don't want the user to HAVE to keep a reference to it when timers are active in it.
            // It gets created when the first timer gets added, and cleaned up when the TimerThread notices it's empty.
            // The TimerThread will always notice it's empty eventually, since the TimerThread will always wake up and
            // try to fire the timer, even if it was cancelled and removed prematurely.
            private IntPtr m_ThisHandle;

            // This sentinel TimerNode acts as both the head and the tail, allowing nodes to go in and out of the list without updating
            // any TimerQueue members.  m_Timers.Next is the true head, and .Prev the true tail.  This also serves as the list's lock.
            private readonly TimerNode m_Timers;

            /// <summary>
            /// <para>Create a new TimerQueue.  TimerQueues must be created while s_NewQueues is locked in
            /// order to synchronize with Shutdown().</para>
            /// </summary>
            /// <param name="durationMilliseconds"></param>
            internal TimerQueue(int durationMilliseconds) :
                base(durationMilliseconds)
            {
                // Create the doubly-linked list with a sentinel head and tail so that this member never needs updating.
                m_Timers = new TimerNode();
                m_Timers.Next = m_Timers;
                m_Timers.Prev = m_Timers;

                // If ReleaseHandle comes back, we need something like this here.
                // m_HandleFrozen = s_ThreadState == (int) TimerThreadState.Stopped ? 1 : 0;
            }

            /// <summary>
            /// <para>Creates new timers.  This method is thread-safe.</para>
            /// </summary>
            internal override Timer CreateTimer(Callback callback, object context) {
                TimerNode timer = new TimerNode(callback, context, Duration, m_Timers);

                // Add this on the tail.  (Actually, one before the tail - m_Timers is the sentinel tail.)
                bool needProd = false;
                lock (m_Timers)
                {
                    GlobalLog.Assert(m_Timers.Prev.Next == m_Timers, "TimerThread#{0}::CreateTimer()|m_Tail corruption.", Thread.CurrentThread.ManagedThreadId.ToString());

                    // If this is the first timer in the list, we need to create a queue handle and prod the timer thread.
                    if (m_Timers.Next == m_Timers)
                    {
                        if (m_ThisHandle == IntPtr.Zero)
                        {
                            m_ThisHandle = (IntPtr) GCHandle.Alloc(this);
                        }
                        needProd = true;
                    }

                    timer.Next = m_Timers;
                    timer.Prev = m_Timers.Prev;
                    m_Timers.Prev.Next = timer;
                    m_Timers.Prev = timer;
                }

                // If, after we add the new tail, there is a chance that the tail is the next
                // node to be processed, we need to wake up the timer thread.
                if (needProd)
                {
                    TimerThread.Prod();
                }

                return timer;
            }

            /// <summary>
            /// <para>Called by the timer thread to fire the expired timers.  Returns true if there are future timers
            /// in the queue, and if so, also sets nextExpiration.</para>
            /// </summary>
            internal bool Fire(out int nextExpiration) {
                while (true)
                {
                    // Check if we got to the end.  If so, free the handle.
                    TimerNode timer = m_Timers.Next;
                    if (timer == m_Timers)
                    {
                        lock (m_Timers)
                        {
                            timer = m_Timers.Next;
                            if (timer == m_Timers)
                            {
                                if(m_ThisHandle != IntPtr.Zero)
                                {
                                    ((GCHandle) m_ThisHandle).Free();
                                    m_ThisHandle = IntPtr.Zero;
                                }

                                nextExpiration = 0;
                                return false;
                            }
                        }
                    }

                    if (!timer.Fire())
                    {
                        nextExpiration = timer.Expiration;
                        return true;
                    }
                }
            }

            /* Currently unused.  If revived, needs to be changed to the new design of m_ThisHandle.
            /// <summary>
            /// <para>Release the GCHandle to this object, and prevent it from ever being allocated again.</para>
            /// </summary>
            internal void ReleaseHandle()
            {
                if (Interlocked.Exchange(ref m_HandleFrozen, 1) == 1) {
                    return;
                }

                // Add a fake timer to the count.  This will prevent the count ever again reaching zero, effectively
                // disabling the GCHandle alloc/free logic.  If it finds that one is allocated, deallocate it.
                if (Interlocked.Increment(ref m_ActiveTimerCount) != 1) {
                    IntPtr handle;
                    while ((handle = Interlocked.Exchange(ref m_ThisHandle, IntPtr.Zero)) == IntPtr.Zero)
                    {
                        Thread.SpinWait(1);
                    }
                    ((GCHandle)handle).Free();
                }
            }
            */
        }

        /// <summary>
        /// <para>A special dummy implementation for a queue of timers of infinite duration.</para>
        /// </summary>
        private class InfiniteTimerQueue : Queue {
            internal InfiniteTimerQueue() : base(Timeout.Infinite) { }

            /// <summary>
            /// <para>Always returns a dummy infinite timer.</para>
            /// </summary>
            internal override Timer CreateTimer(Callback callback, object context)
            {
                return new InfiniteTimer();
            }
        }

        /// <summary>
        /// <para>Internal representation of an individual timer.</para>
        /// </summary>
        private class TimerNode : Timer {
            private TimerState m_TimerState;
            private Callback m_Callback;
            private object m_Context;
            private object m_QueueLock;
            private TimerNode next;
            private TimerNode prev;

            /// <summary>
            /// <para>Status of the timer.</para>
            /// </summary>
            private enum TimerState {
                Ready,
                Fired,
                Cancelled,
                Sentinel
            }

            internal TimerNode(Callback callback, object context, int durationMilliseconds, object queueLock) : base(durationMilliseconds)
            {
                if (callback != null)
                {
                    m_Callback = callback;
                    m_Context = context;
                }
                m_TimerState = TimerState.Ready;
                m_QueueLock = queueLock;
                GlobalLog.Print("TimerThreadTimer#" + StartTime.ToString() + "::.ctor()");
            }

            // A sentinel node - both the head and tail are one, which prevent the head and tail from ever having to be updated.
            internal TimerNode() : base (0)
            {
                m_TimerState = TimerState.Sentinel;
            }

            /*
            // Consider removing.
            internal bool IsDead
            {
                get
                {
                    return m_TimerState != TimerState.Ready;
                }
            }
            */

            internal override bool HasExpired {
                get {
                    return m_TimerState == TimerState.Fired;
                }
            }

            internal TimerNode Next
            {
                get
                {
                    return next;
                }

                set
                {
                    next = value;
                }
            }

            internal TimerNode Prev
            {
                get
                {
                    return prev;
                }

                set
                {
                    prev = value;
                }
            }

            /// <summary>
            /// <para>Cancels the timer.  Returns true if it hasn't and won't fire; false if it has or will, or has already been cancelled.</para>
            /// </summary>
            internal override bool Cancel() {
                if (m_TimerState == TimerState.Ready)
                {
                    lock (m_QueueLock)
                    {
                        if (m_TimerState == TimerState.Ready)
                        {
                            // Remove it from the list.  This keeps the list from getting to big when there are a lot of rapid creations
                            // and cancellations.  This is done before setting it to Cancelled to try to prevent the Fire() loop from
                            // seeing it, or if it does, of having to take a lock to synchronize with the state of the list.
                            Next.Prev = Prev;
                            Prev.Next = Next;

                            // Just cleanup.  Doesn't need to be in the lock but is easier to have here.
                            Next = null;
                            Prev = null;
                            m_Callback = null;
                            m_Context = null;

                            m_TimerState = TimerState.Cancelled;

                            GlobalLog.Print("TimerThreadTimer#" + StartTime.ToString() + "::Cancel() (success)");
                            return true;
                        }
                    }
                }

                GlobalLog.Print("TimerThreadTimer#" + StartTime.ToString() + "::Cancel() (failure)");
                return false;
            }

            /// <summary>
            /// <para>Fires the timer if it is still active and has expired.  Returns
            /// true if it can be deleted, or false if it is still timing.</para>
            /// </summary>
            internal bool Fire() {
                GlobalLog.Assert(m_TimerState != TimerState.Sentinel, "TimerThread#{0}::Fire()|TimerQueue tried to Fire a Sentinel.", Thread.CurrentThread.ManagedThreadId.ToString());

                if (m_TimerState != TimerState.Ready)
                {
                    return true;
                }

                // Must get the current tick count within this method so it is guaranteed not to be before
                // StartTime, which is set in the constructor.
                int nowMilliseconds = Environment.TickCount;
                if (IsTickBetween(StartTime, Expiration, nowMilliseconds)) {
                    GlobalLog.Print("TimerThreadTimer#" + StartTime + "::Fire() Not firing (" + StartTime + " <= " + nowMilliseconds + " < " + Expiration + ")");
                    return false;
                }

                bool needCallback = false;
                lock (m_QueueLock)
                {
                    if (m_TimerState == TimerState.Ready)
                    {
                        GlobalLog.Print("TimerThreadTimer#" + StartTime + "::Fire() Firing (" + StartTime + " <= " + nowMilliseconds + " >= " + Expiration + ")");
                        m_TimerState = TimerState.Fired;

                        // Remove it from the list.
                        Next.Prev = Prev;
                        Prev.Next = Next;

                        // Doesn't need to be in the lock but is easier to have here.
                        Next = null;
                        Prev = null;
                        needCallback = m_Callback != null;
                    }
                }

                if (needCallback)
                {
                    try {
                        Callback callback = m_Callback;
                        object context = m_Context;
                        m_Callback = null;
                        m_Context = null;
                        callback(this, nowMilliseconds, context);
                    }
                    catch (Exception exception) {
                        if (NclUtilities.IsFatal(exception)) throw;

                        if (Logging.On) Logging.PrintError(Logging.Web, "TimerThreadTimer#" + StartTime.ToString(NumberFormatInfo.InvariantInfo) + "::Fire() - " + SR.GetString(SR.net_log_exception_in_callback, exception));
                        GlobalLog.Print("TimerThreadTimer#" + StartTime + "::Fire() exception in callback: " + exception);

                        // This thread is not allowed to go into user code, so we should never get an exception here.
                        // So, in debug, throw it up, killing the AppDomain.  In release, we'll just ignore it.
#if DEBUG
                        throw;
#endif
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// <para>A dummy infinite timer.</para>
        /// </summary>
        private class InfiniteTimer : Timer {
            internal InfiniteTimer() : base(Timeout.Infinite) { }

            private int cancelled;

            internal override bool HasExpired {
                get {
                    return false;
                }
            }

            /// <summary>
            /// <para>Cancels the timer.  Returns true the first time, false after that.</para>
            /// </summary>
            internal override bool Cancel() {
                return Interlocked.Exchange(ref cancelled, 1) == 0;
            }
        }

        /// <summary>
        /// <para>Internal mechanism used when timers are added to wake up / create the thread.</para>
        /// </summary>
        private static void Prod() {
            s_ThreadReadyEvent.Set();
            TimerThreadState oldState = (TimerThreadState) Interlocked.CompareExchange(
                ref s_ThreadState,
                (int) TimerThreadState.Running,
                (int) TimerThreadState.Idle);

            if (oldState == TimerThreadState.Idle) {
                new Thread(new ThreadStart(ThreadProc)).Start();
            }
        }

        /// <summary>
        /// <para>Thread for the timer.  Ignores all exceptions except ThreadAbort.  If no activity occurs for a while,
        /// the thread will shut down.</para>
        /// </summary>
        private static void ThreadProc()
        {
#if DEBUG
            GlobalLog.SetThreadSource(ThreadKinds.Timer);
            using (GlobalLog.SetThreadKind(ThreadKinds.System | ThreadKinds.Async)) {
#endif
            GlobalLog.Print("TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString() + "::ThreadProc() Start");
            
            // t_IsTimerThread = true; -- Not used anywhere.

            // Set this thread as a background thread.  On AppDomain/Process shutdown, the thread will just be killed.
            Thread.CurrentThread.IsBackground = true;

            // Keep a permanent lock on s_Queues.  This lets for example Shutdown() know when this thread isn't running.
            lock (s_Queues) {

                // If shutdown was recently called, abort here.
                if (Interlocked.CompareExchange(ref s_ThreadState, (int) TimerThreadState.Running, (int) TimerThreadState.Running) !=
                    (int) TimerThreadState.Running) {
                    return;
                }

                bool running = true;
                while(running) {
                    try {
                        s_ThreadReadyEvent.Reset();

                        while (true) {
                            // Copy all the new queues to the real queues.  Since only this thread modifies the real queues, it doesn't have to lock it.
                            if (s_NewQueues.Count > 0) {
                                lock (s_NewQueues) {
                                    for (LinkedListNode<WeakReference> node = s_NewQueues.First; node != null; node = s_NewQueues.First) {
                                        s_NewQueues.Remove(node);
                                        s_Queues.AddLast(node);
                                    }
                                }
                            }

                            int now = Environment.TickCount;
                            int nextTick = 0;
                            bool haveNextTick = false;
                            for (LinkedListNode<WeakReference> node = s_Queues.First; node != null; /* node = node.Next must be done in the body */) {
                                TimerQueue queue = (TimerQueue) node.Value.Target;
                                if (queue == null) {
                                    LinkedListNode<WeakReference> next = node.Next;
                                    s_Queues.Remove(node);
                                    node = next;
                                    continue;
                                }

                                // Fire() will always return values that should be interpreted as later than 'now' (that is, even if 'now' is
                                // returned, it is 0x100000000 milliseconds in the future).  There's also a chance that Fire() will return a value
                                // intended as > 0x100000000 milliseconds from 'now'.  Either case will just cause an extra scan through the timers.
                                int nextTickInstance;
                                if (queue.Fire(out nextTickInstance) && (!haveNextTick || IsTickBetween(now, nextTick, nextTickInstance))){
                                    nextTick = nextTickInstance;
                                    haveNextTick = true;
                                }

                                node = node.Next;
                            }

                            // Figure out how long to wait, taking into account how long the loop took.
                            // Add 15 ms to compensate for poor TickCount resolution (want to guarantee a firing).
                            int newNow = Environment.TickCount;
                            int waitDuration = haveNextTick ?
                                (int) (IsTickBetween(now, nextTick, newNow) ?
                                    Math.Min(unchecked((uint) (nextTick - newNow)), (uint) (Int32.MaxValue - c_TickCountResolution)) + c_TickCountResolution :
                                    0) :
                                c_ThreadIdleTimeoutMilliseconds;

                            GlobalLog.Print("TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString() + "::ThreadProc() Waiting for " + waitDuration + "ms");
                            int waitResult = WaitHandle.WaitAny(s_ThreadEvents, waitDuration, false);

                            // 0 is s_ThreadShutdownEvent - die.
                            if (waitResult == 0) {
                                GlobalLog.Print("TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString() + "::ThreadProc() Awoke, cause: Shutdown");
                                running = false;
                                break;
                            }

                            GlobalLog.Print("TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString() + "::ThreadProc() Awoke, cause: " + (waitResult == WaitHandle.WaitTimeout ? "Timeout" : "Prod"));

                            // If we timed out with nothing to do, shut down. 
                            if (waitResult == WaitHandle.WaitTimeout && !haveNextTick) {
                                Interlocked.CompareExchange(ref s_ThreadState, (int) TimerThreadState.Idle, (int) TimerThreadState.Running);
                                // There could have been one more prod between the wait and the exchange.  Check, and abort if necessary.
                                if (s_ThreadReadyEvent.WaitOne(0, false)) {
                                    if (Interlocked.CompareExchange(ref s_ThreadState, (int) TimerThreadState.Running, (int) TimerThreadState.Idle) ==
                                        (int) TimerThreadState.Idle) {
                                        continue;
                                    }
                                }

                                running = false;
                                break;
                            }
                        }
                    }
                    catch (Exception exception) {
                        if (NclUtilities.IsFatal(exception)) throw;

                        if (Logging.On) Logging.PrintError(Logging.Web, "TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString(NumberFormatInfo.InvariantInfo) + "::ThreadProc() - Exception:" + exception.ToString());
                        GlobalLog.Print("TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString() + "::ThreadProc() exception: " + exception);

                        // The only options are to continue processing and likely enter an error-loop,
                        // shut down timers for this AppDomain, or shut down the AppDomain.  Go with shutting
                        // down the AppDomain in debug, and going into a loop in retail, but try to make the
                        // loop somewhat slow.  Note that in retail, this can only be triggered by OutOfMemory or StackOverflow,
                        // or an thrown within TimerThread - the rest are caught in Fire().
#if !DEBUG
                        Thread.Sleep(1000);
#else
                        throw;
#endif
                    }
                }
            }

            GlobalLog.Print("TimerThread#" + Thread.CurrentThread.ManagedThreadId.ToString() + "::ThreadProc() Stop");
#if DEBUG
            }
#endif
        }

        /* Currently unused.
        /// <summary>
        /// <para>Stops the timer thread and prevents a new one from forming.  No more timers can expire.</para>
        /// </summary>
        internal static void Shutdown() {
            StopTimerThread();

            // As long as TimerQueues are always created and added to s_NewQueues within the same lock,
            // this should catch all existing TimerQueues (and all new onew will see s_ThreadState).
            lock (s_NewQueues) {
                foreach (WeakReference node in s_NewQueues) {
                    TimerQueue queue = (TimerQueue)node.Target;
                    if(queue != null) {
                        queue.ReleaseHandle();
                    }
                }
            }

            // Once that thread is gone, release all the remaining GCHandles.
            lock (s_Queues) {
                foreach (WeakReference node in s_Queues) {
                    TimerQueue queue = (TimerQueue)node.Target;
                    if(queue != null) {
                        queue.ReleaseHandle();
                    }
                }
            }
        }
        */

        private static void StopTimerThread()
        {
            Interlocked.Exchange(ref s_ThreadState, (int) TimerThreadState.Stopped);
            s_ThreadShutdownEvent.Set();
        }

        /// <summary>
        /// <para>Helper for deciding whether a given TickCount is before or after a given expiration
        /// tick count assuming that it can't be before a given starting TickCount.</para>
        /// </summary>
        private static bool IsTickBetween(int start, int end, int comparand) {
            // Assumes that if start and end are equal, they are the same time.
            // Assumes that if the comparand and start are equal, no time has passed,
            // and that if the comparand and end are equal, end has occurred.
            return ((start <= comparand) == (end <= comparand)) != (start <= end);
        }

        /// <summary>
        /// <para>When the AppDomain is shut down, the timer thread is stopped.</para>
        /// <summary>
        private static void OnDomainUnload(object sender, EventArgs e) {
            try
            {
                StopTimerThread();
            }
            catch { }
        }

        /*
        /// <summary>
        /// <para>This thread static can be used to tell whether the current thread is the TimerThread thread.</para>
        /// </summary>
        [ThreadStatic]
        private static bool t_IsTimerThread;

        // Consider removing.
        internal static bool IsTimerThread
        {
            get
            {
                return t_IsTimerThread;
            }
        }
        */
    }
}
