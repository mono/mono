//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    delegate void OverlappedIOCompleteCallback(bool haveResult, int error, int bytesRead);

    unsafe class OverlappedContext
    {
        const int HandleOffsetFromOverlapped32 = -4;
        const int HandleOffsetFromOverlapped64 = -3;

        static IOCompletionCallback completeCallback;
        static WaitOrTimerCallback eventCallback;
        static WaitOrTimerCallback cleanupCallback;
        static byte[] dummyBuffer = new byte[0];

        object[] bufferHolder;
        byte* bufferPtr;
        NativeOverlapped* nativeOverlapped;
        GCHandle pinnedHandle;
        object pinnedTarget;
        Overlapped overlapped;
        RootedHolder rootedHolder;
        OverlappedIOCompleteCallback pendingCallback;  // Null when no async I/O is pending.
        bool deferredFree;
        bool syncOperationPending;
        ManualResetEvent completionEvent;
        IntPtr eventHandle;

        // Only used by unbound I/O.
        RegisteredWaitHandle registration;

#if DEBUG_EXPENSIVE
        StackTrace freeStack;
#endif


        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public OverlappedContext()
        {
            if (OverlappedContext.completeCallback == null)
            {
                OverlappedContext.completeCallback = Fx.ThunkCallback(new IOCompletionCallback(CompleteCallback));
            }
            if (OverlappedContext.eventCallback == null)
            {
                OverlappedContext.eventCallback = Fx.ThunkCallback(new WaitOrTimerCallback(EventCallback));
            }
            if (OverlappedContext.cleanupCallback == null)
            {
                OverlappedContext.cleanupCallback = Fx.ThunkCallback(new WaitOrTimerCallback(CleanupCallback));
            }

            this.bufferHolder = new object[] { OverlappedContext.dummyBuffer };
            this.overlapped = new Overlapped();
            this.nativeOverlapped = this.overlapped.UnsafePack(OverlappedContext.completeCallback, this.bufferHolder);

            // When replacing the buffer, we need to provoke the CLR to fix up the handle of the pin.
            this.pinnedHandle = GCHandle.FromIntPtr(*((IntPtr*)nativeOverlapped +
                (IntPtr.Size == 4 ? HandleOffsetFromOverlapped32 : HandleOffsetFromOverlapped64)));
            this.pinnedTarget = this.pinnedHandle.Target;

            // Create the permanently rooted holder and put it in the Overlapped.
            this.rootedHolder = new RootedHolder();
            this.overlapped.AsyncResult = rootedHolder;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        ~OverlappedContext()
        {
            if (this.nativeOverlapped != null && !AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
            {
                if (this.syncOperationPending)
                {
                    Fx.Assert(this.rootedHolder != null, "rootedHolder null in Finalize.");
                    Fx.Assert(this.rootedHolder.EventHolder != null, "rootedHolder.EventHolder null in Finalize.");
                    Fx.Assert(OverlappedContext.cleanupCallback != null, "cleanupCallback null in Finalize.");

                    // Can't free the overlapped.  Register a callback to deal with this.
                    // This will ressurect the OverlappedContext.
                    // The completionEvent will still be alive (not finalized) since it's rooted by the pending Overlapped in the holder.
                    // We own it now and will close it in the callback.
                    ThreadPool.UnsafeRegisterWaitForSingleObject(this.rootedHolder.EventHolder, OverlappedContext.cleanupCallback, this, Timeout.Infinite, true);
                }
                else
                {
                    Overlapped.Free(this.nativeOverlapped);
                }
            }
        }

        // None of the OverlappedContext methods are threadsafe.
        // Free or FreeOrDefer can only be called once.  FreeIfDeferred can be called any number of times, as long as it's only
        // called once after FreeOrDefer.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void Free()
        {
            if (this.pendingCallback != null)
            {
                throw Fx.AssertAndThrow("OverlappedContext.Free called while async operation is pending.");
            }
            if (this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("OverlappedContext.Free called while [....] operation is pending.");
            }
            if (this.nativeOverlapped == null)
            {
                throw Fx.AssertAndThrow("OverlappedContext.Free called multiple times.");
            }

#if DEBUG_EXPENSIVE
            this.freeStack = new StackTrace();
#endif

            // The OverlappedData is cached and reused.  It looks weird if there's still a reference to it form here.
            this.pinnedTarget = null;

            NativeOverlapped* nativePtr = this.nativeOverlapped;
            this.nativeOverlapped = null;
            Overlapped.Free(nativePtr);

            if (this.completionEvent != null)
            {
                this.completionEvent.Close();
            }

            GC.SuppressFinalize(this);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public bool FreeOrDefer()
        {
            if (this.pendingCallback != null || this.syncOperationPending)
            {
                this.deferredFree = true;
                return false;
            }

            Free();
            return true;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public bool FreeIfDeferred()
        {
            if (this.deferredFree)
            {
                return FreeOrDefer();
            }

            return false;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void StartAsyncOperation(byte[] buffer, OverlappedIOCompleteCallback callback, bool bound)
        {
            if (callback == null)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called with null callback.");
            }
            if (this.pendingCallback != null)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called while another is in progress.");
            }
            if (this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called while a [....] operation was already pending.");
            }
            if (this.nativeOverlapped == null)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called on freed OverlappedContext.");
            }

            this.pendingCallback = callback;

            if (buffer != null)
            {
                Fx.Assert(object.ReferenceEquals(this.bufferHolder[0], OverlappedContext.dummyBuffer), "StartAsyncOperation: buffer holder corrupted.");
                this.bufferHolder[0] = buffer;
                this.pinnedHandle.Target = this.pinnedTarget;
                this.bufferPtr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            }

            if (bound)
            {
                this.overlapped.EventHandleIntPtr = IntPtr.Zero;

                // For completion ports, the back-reference is this member.
                this.rootedHolder.ThisHolder = this;
            }
            else
            {
                // Need to do this since we register the wait before posting the I/O.
                if (this.completionEvent != null)
                {
                    this.completionEvent.Reset();
                }

                this.overlapped.EventHandleIntPtr = EventHandle;

                // For unbound, the back-reference is this registration.
                this.registration = ThreadPool.UnsafeRegisterWaitForSingleObject(this.completionEvent, OverlappedContext.eventCallback, this, Timeout.Infinite, true);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void CancelAsyncOperation()
        {
            this.rootedHolder.ThisHolder = null;
            if (this.registration != null)
            {
                this.registration.Unregister(null);
                this.registration = null;
            }
            this.bufferPtr = null;
            this.bufferHolder[0] = OverlappedContext.dummyBuffer;
            this.pendingCallback = null;
        }

        //  public void StartSyncOperation(byte[] buffer)
        //  {
        //      StartSyncOperation(buffer, ref this.bufferHolder[0], false);
        //  }

        // The only holder allowed is Holder[0].  It can be passed in as a ref to prevent repeated expensive array lookups.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void StartSyncOperation(byte[] buffer, ref object holder)
        {
            if (this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("StartSyncOperation called while an operation was already pending.");
            }
            if (this.pendingCallback != null)
            {
                throw Fx.AssertAndThrow("StartSyncOperation called while an async operation was already pending.");
            }
            if (this.nativeOverlapped == null)
            {
                throw Fx.AssertAndThrow("StartSyncOperation called on freed OverlappedContext.");
            }

            this.overlapped.EventHandleIntPtr = EventHandle;

            // [....] operations do NOT root this object.  If it gets finalized, we need to know not to free the buffer.
            // We do root the event.
            this.rootedHolder.EventHolder = this.completionEvent;
            this.syncOperationPending = true;

            if (buffer != null)
            {
                Fx.Assert(object.ReferenceEquals(holder, OverlappedContext.dummyBuffer), "StartSyncOperation: buffer holder corrupted.");
                holder = buffer;
                this.pinnedHandle.Target = this.pinnedTarget;
                this.bufferPtr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            }
        }

        // If this returns false, the OverlappedContext is no longer usable.  It shouldn't be freed or anything.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public bool WaitForSyncOperation(TimeSpan timeout)
        {
            return WaitForSyncOperation(timeout, ref this.bufferHolder[0]);
        }

        // The only holder allowed is Holder[0].  It can be passed in as a ref to prevent repeated expensive array lookups.
        [SecurityCritical]
        public bool WaitForSyncOperation(TimeSpan timeout, ref object holder)
        {
            if (!this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("WaitForSyncOperation called while no operation was pending.");
            }

            if (!UnsafeNativeMethods.HasOverlappedIoCompleted(this.nativeOverlapped))
            {
                if (!TimeoutHelper.WaitOne(this.completionEvent, timeout))
                {
                    // We can't free ourselves until the operation is done.  The only way to do that is register a callback.
                    // This will root the object.  No longer any need for the finalizer.  This instance is unusable after this.
                    GC.SuppressFinalize(this);
                    ThreadPool.UnsafeRegisterWaitForSingleObject(this.completionEvent, OverlappedContext.cleanupCallback, this, Timeout.Infinite, true);
                    return false;
                }
            }

            Fx.Assert(this.bufferPtr == null || this.bufferPtr == (byte*)Marshal.UnsafeAddrOfPinnedArrayElement((byte[])holder, 0),
                "The buffer moved during a [....] call!");

            CancelSyncOperation(ref holder);
            return true;
        }

        //  public void CancelSyncOperation()
        //  {
        //      CancelSyncOperation(ref this.bufferHolder[0]);
        //  }

        // The only holder allowed is Holder[0].  It can be passed in as a ref to prevent repeated expensive array lookups.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void CancelSyncOperation(ref object holder)
        {
            this.bufferPtr = null;
            holder = OverlappedContext.dummyBuffer;
            Fx.Assert(object.ReferenceEquals(this.bufferHolder[0], OverlappedContext.dummyBuffer), "Bad holder passed to CancelSyncOperation.");

            this.syncOperationPending = false;
            this.rootedHolder.EventHolder = null;
        }

        // This should ONLY be used to make a 'ref object' parameter to the zeroth element, to prevent repeated expensive array lookups.
        public object[] Holder
        {
            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            get
            {
                return this.bufferHolder;
            }
        }

        public byte* BufferPtr
        {
            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            get
            {
                byte* ptr = this.bufferPtr;
                if (ptr == null)
                {
#pragma warning suppress 56503 // [....], not a publicly accessible API
                    throw Fx.AssertAndThrow("Pointer requested while no operation pending or no buffer provided.");
                }
                return ptr;
            }
        }

        public NativeOverlapped* NativeOverlapped
        {
            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            get
            {
                NativeOverlapped* ptr = this.nativeOverlapped;
                if (ptr == null)
                {
#pragma warning suppress 56503 // [....], not a publicly accessible API
                    throw Fx.AssertAndThrow("NativeOverlapped pointer requested after it was freed.");
                }
                return ptr;
            }
        }

        IntPtr EventHandle
        {
            get
            {
                if (this.completionEvent == null)
                {
                    this.completionEvent = new ManualResetEvent(false);
                    this.eventHandle = (IntPtr)(1 | (long)this.completionEvent.SafeWaitHandle.DangerousGetHandle());
                }
                return this.eventHandle;
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        static void CompleteCallback(uint error, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            // Empty out the AsyncResult ASAP to close the leak window.
            Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
            OverlappedContext pThis = ((RootedHolder)overlapped.AsyncResult).ThisHolder;
            Fx.Assert(pThis != null, "Overlapped.AsyncResult not set. I/O completed multiple times, or cancelled I/O completed.");
            Fx.Assert(object.ReferenceEquals(pThis.overlapped, overlapped), "CompleteCallback completed with corrupt OverlappedContext.overlapped.");
            Fx.Assert(object.ReferenceEquals(pThis.rootedHolder, overlapped.AsyncResult), "CompleteCallback completed with corrupt OverlappedContext.rootedHolder.");
            pThis.rootedHolder.ThisHolder = null;

            Fx.Assert(pThis.bufferPtr == null || pThis.bufferPtr == (byte*)Marshal.UnsafeAddrOfPinnedArrayElement((byte[])pThis.bufferHolder[0], 0),
                "Buffer moved during bound async operation!");

            // Release the pin.
            pThis.bufferPtr = null;
            pThis.bufferHolder[0] = OverlappedContext.dummyBuffer;

            OverlappedIOCompleteCallback callback = pThis.pendingCallback;
            pThis.pendingCallback = null;
            Fx.Assert(callback != null, "PendingCallback not set. I/O completed multiple times, or cancelled I/O completed.");

            callback(true, (int)error, checked((int)numBytes));
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        static void EventCallback(object state, bool timedOut)
        {
            OverlappedContext pThis = state as OverlappedContext;
            Fx.Assert(pThis != null, "OverlappedContext.EventCallback registered wait doesn't have an OverlappedContext as state.");

            if (timedOut)
            {
                Fx.Assert("OverlappedContext.EventCallback registered wait timed out.");

                // Turn this into a leak.  Don't let ourselves get cleaned up - could scratch the heap.
                if (pThis == null || pThis.rootedHolder == null)
                {
                    // We're doomed to do a wild write and corrupt the process.
                    DiagnosticUtility.FailFast("Can't prevent heap corruption.");
                }
                pThis.rootedHolder.ThisHolder = pThis;
                return;
            }

            pThis.registration = null;

            Fx.Assert(pThis.bufferPtr == null || pThis.bufferPtr == (byte*)Marshal.UnsafeAddrOfPinnedArrayElement((byte[])pThis.bufferHolder[0], 0),
                "Buffer moved during unbound async operation!");

            // Release the pin.
            pThis.bufferPtr = null;
            pThis.bufferHolder[0] = OverlappedContext.dummyBuffer;

            OverlappedIOCompleteCallback callback = pThis.pendingCallback;
            pThis.pendingCallback = null;
            Fx.Assert(callback != null, "PendingCallback not set. I/O completed multiple times, or cancelled I/O completed.");

            callback(false, 0, 0);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        static void CleanupCallback(object state, bool timedOut)
        {
            OverlappedContext pThis = state as OverlappedContext;
            Fx.Assert(pThis != null, "OverlappedContext.CleanupCallback registered wait doesn't have an OverlappedContext as state.");

            if (timedOut)
            {
                Fx.Assert("OverlappedContext.CleanupCallback registered wait timed out.");

                // Turn this into a leak.
                return;
            }

            Fx.Assert(pThis.bufferPtr == null || pThis.bufferPtr == (byte*)Marshal.UnsafeAddrOfPinnedArrayElement((byte[])pThis.bufferHolder[0], 0),
                "Buffer moved during synchronous deferred cleanup!");

            Fx.Assert(pThis.syncOperationPending, "OverlappedContext.CleanupCallback called with no [....] operation pending.");
            pThis.pinnedTarget = null;
            pThis.rootedHolder.EventHolder.Close();
            Overlapped.Free(pThis.nativeOverlapped);
        }

        // This class is always held onto (rooted) by the packed Overlapped.  The OverlappedContext instance moves itself in and out of
        // this object to root itself.  It's also used to root the ManualResetEvent during [....] operations.
        // It needs to be an IAsyncResult since that's what Overlapped takes.
        class RootedHolder : IAsyncResult
        {
            OverlappedContext overlappedBuffer;
            ManualResetEvent eventHolder;


            public OverlappedContext ThisHolder
            {
                get
                {
                    return this.overlappedBuffer;
                }

                set
                {
                    this.overlappedBuffer = value;
                }
            }

            public ManualResetEvent EventHolder
            {
                get
                {
                    return this.eventHolder;
                }

                set
                {
                    this.eventHolder = value;
                }
            }


            // Unused IAsyncResult implementation.
            object IAsyncResult.AsyncState
            {
                get
                {
#pragma warning suppress 56503 // [....], not a publicly accessible API
                    throw Fx.AssertAndThrow("RootedHolder.AsyncState called.");
                }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
#pragma warning suppress 56503 // [....], not a publicly accessible API
                    throw Fx.AssertAndThrow("RootedHolder.AsyncWaitHandle called.");
                }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
#pragma warning suppress 56503 // [....], not a publicly accessible API
                    throw Fx.AssertAndThrow("RootedHolder.CompletedSynchronously called.");
                }
            }

            bool IAsyncResult.IsCompleted
            {
                get
                {
#pragma warning suppress 56503 // [....], not a publicly accessible API
                    throw Fx.AssertAndThrow("RootedHolder.IsCompleted called.");
                }
            }
        }
    }
}
