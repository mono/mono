//------------------------------------------------------------------------------
// <copyright file="_OverlappedAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Sockets {
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32;

    //
    //  BaseOverlappedAsyncResult - used to enable async Socket operation
    //  such as the BeginSend, BeginSendTo, BeginReceive, BeginReceiveFrom, BeginSendFile,
    //  BeginAccept, calls.
    //

    internal class BaseOverlappedAsyncResult : ContextAwareResult
    {
        //
        // internal class members
        //
        private SafeOverlappedFree m_UnmanagedBlob;  // Handle for global memory.
        private AutoResetEvent  m_OverlappedEvent;
        private int             m_CleanupCount;
        private bool            m_DisableOverlapped;
        private bool            m_UseOverlappedIO;
        private GCHandle []     m_GCHandles;
        private OverlappedCache m_Cache;

        //
        // The WinNT Completion Port callback.
        //
#if SOCKETTHREADPOOL
        internal
#else
        private
#endif
        unsafe static readonly   IOCompletionCallback s_IOCallback = new IOCompletionCallback(CompletionPortCallback);

        //
        // Constructor. We take in the socket that's creating us, the caller's
        // state object, and callback. We save the socket and state, and allocate
        // an event for the WaitHandle.
        //
        internal BaseOverlappedAsyncResult(Socket socket, Object asyncState, AsyncCallback asyncCallback)
        : base(socket, asyncState, asyncCallback) {
            //
            // BeginAccept() allocates and returns an AcceptOverlappedAsyncResult that will call
            // this constructor.
            //
            m_UseOverlappedIO = Socket.UseOverlappedIO || socket.UseOnlyOverlappedIO;
            if (m_UseOverlappedIO)
            {
                //
                // since the binding between the event handle and the callback
                // happens after the IO was queued to the OS, there is no ----
                // condition and the Cleanup code can be called at most once.
                //
                m_CleanupCount = 1;
            }
            else {
                //
                // since the binding between the event handle and the callback
                // has already happened there is a race condition and so the
                // Cleanup code can be called more than once and at most twice.
                //
                m_CleanupCount = 2;
            }
        }

        //
        // Constructor. We take in the socket that's creating us, and turn off Async
        // We save the socket and state.
        internal BaseOverlappedAsyncResult(Socket socket)
        : base(socket, null, null) {
            m_CleanupCount = 1;
            m_DisableOverlapped = true;
        }


        //PostCompletion returns the result object to be set before the user's callback is invoked.
        internal virtual object PostCompletion(int numBytes)
        {
            return numBytes;
        }


        //
        // SetUnmanagedStructures -
        //
        //  This needs to be called for overlapped IO to function properly.
        //
        //  Fills in Overlapped Structures used in an Async Overlapped Winsock call
        //  these calls are outside the runtime and are unmanaged code, so we need
        //  to prepare specific structures and ints that lie in unmanaged memory
        //  since the Overlapped calls can be Async
        //
        internal void SetUnmanagedStructures(object objectsToPin)
        {
            if (!m_DisableOverlapped)
            {
                // Casting to object[] is very expensive.  Only do it once.
                object[] objectsToPinArray = null;
                bool triedCastingToArray = false;

                bool useCache = false;
                if (m_Cache != null)
                {
                    if (objectsToPin == null && m_Cache.PinnedObjects == null)
                    {
                        useCache = true;
                    }
                    else if (m_Cache.PinnedObjects != null)
                    {
                        if (m_Cache.PinnedObjectsArray == null)
                        {
                            if (objectsToPin == m_Cache.PinnedObjects)
                            {
                                useCache = true;
                            }
                        }
                        else if (objectsToPin != null)
                        {
                            triedCastingToArray = true;
                            objectsToPinArray = objectsToPin as object[];
                            if (objectsToPinArray != null && objectsToPinArray.Length == 0)
                            {
                                objectsToPinArray = null;
                            }
                            if (objectsToPinArray != null && objectsToPinArray.Length == m_Cache.PinnedObjectsArray.Length)
                            {
                                useCache = true;
                                for (int i = 0; i < objectsToPinArray.Length; i++)
                                {
                                    if (objectsToPinArray[i] != m_Cache.PinnedObjectsArray[i])
                                    {
                                        useCache = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!useCache && m_Cache != null)
                {
                    GlobalLog.Print("BaseOverlappedAsyncResult#" + ValidationHelper.HashString(this) + "::SetUnmanagedStructures() Cache miss - freeing cache.");
                    m_Cache.Free();
                    m_Cache = null;
                }

                Socket s = (Socket) AsyncObject;
                if (m_UseOverlappedIO)
                {
                    //
                    // we're using overlapped IO, allocate an overlapped structure
                    // consider using a static growing pool of allocated unmanaged memory.
                    //
                    GlobalLog.Assert(m_UnmanagedBlob == null, "BaseOverlappedAsyncResult#{0}::SetUnmanagedStructures()|Unmanaged blob already allocated. (Called twice?)", ValidationHelper.HashString(this));
                    m_UnmanagedBlob = SafeOverlappedFree.Alloc(s.SafeHandle);

                    PinUnmanagedObjects(objectsToPin);

                    //
                    // create the event handle
                    //

                    m_OverlappedEvent = new AutoResetEvent(false);

                    //
                    // fill in the overlapped structure with the event handle.
                    //

                    Marshal.WriteIntPtr( m_UnmanagedBlob.DangerousGetHandle(), Win32.OverlappedhEventOffset, m_OverlappedEvent.SafeWaitHandle.DangerousGetHandle() );
                }
                else
                {
                    //
                    // Bind the Win32 Socket Handle to the ThreadPool
                    //
                    s.BindToCompletionPort();

                    if (m_Cache == null)
                    {
                        GlobalLog.Print("BaseOverlappedAsyncResult#" + ValidationHelper.HashString(this) + "::EnableCompletionPort() Creating new overlapped cache.");
                        if (objectsToPinArray != null)
                        {
                            m_Cache = new OverlappedCache(new Overlapped(), objectsToPinArray, s_IOCallback);
                        }
                        else
                        {
                            m_Cache = new OverlappedCache(new Overlapped(), objectsToPin, s_IOCallback, triedCastingToArray);
                        }
                    }
                    else
                    {
                        GlobalLog.Print("BaseOverlappedAsyncResult#" + ValidationHelper.HashString(this) + "::EnableCompletionPort() Using cached overlapped.");
                    }

                    m_Cache.Overlapped.AsyncResult = this;

#if DEBUG
                    m_InitialNativeOverlapped = m_Cache.NativeOverlapped;
#endif

                    GlobalLog.Print("BaseOverlappedAsyncResult#" + ValidationHelper.HashString(this) + "::EnableCompletionPort() overlapped:" + ValidationHelper.HashString(m_Cache.Overlapped) + " NativeOverlapped = " + m_Cache.NativeOverlapped.DangerousGetHandle().ToString("x"));
                }
            }
        }

        /*
        // Consider removing.
        internal void SetUnmanagedStructures(object objectsToPin, ref OverlappedCache overlappedCache)
        {
            SetupCache(ref overlappedCache);
            SetUnmanagedStructures(objectsToPin);
        }
        */

        protected void SetupCache(ref OverlappedCache overlappedCache)
        {
#if !NO_OVERLAPPED_CACHE
            GlobalLog.Assert(m_Cache == null, "BaseOverlappedAsyncResult#{0}::SetUnmanagedStructures()|Cache already set up. (Called twice?)", ValidationHelper.HashString(this));
            if (!m_UseOverlappedIO && !m_DisableOverlapped)
            {
                m_Cache = overlappedCache == null ? null : Interlocked.Exchange<OverlappedCache>(ref overlappedCache, null);

                // Need to hold on to the unmanaged structures until the cache is extracted.
                m_CleanupCount++;
            }
#endif
        }

        //
        // This method pins unmanaged objects for Win9x and systems where completion ports are not found
        //
        protected void PinUnmanagedObjects(object objectsToPin)
        {
            if (m_Cache != null)
            {
                m_Cache.Free();
                m_Cache = null;
            }

            if (objectsToPin != null)
            {
                if (objectsToPin.GetType() == typeof(object[]))
                {
                    object [] objectsArray = (object []) objectsToPin;
                    m_GCHandles = new GCHandle[objectsArray.Length];
                    for (int i=0; i<objectsArray.Length; i++) {
                        if (objectsArray[i] != null) {
                            m_GCHandles[i] = GCHandle.Alloc(objectsArray[i], GCHandleType.Pinned);
                        }
                    }
                }
                else
                {
                    m_GCHandles = new GCHandle[1];
                    m_GCHandles[0] = GCHandle.Alloc(objectsToPin, GCHandleType.Pinned);
                }
            }
        }

        internal void ExtractCache(ref OverlappedCache overlappedCache)
        {
#if !NO_OVERLAPPED_CACHE
            if (!m_UseOverlappedIO && !m_DisableOverlapped)
            {
                // Have to be super careful.  Socket isn't synchronized, so if a user calls End() twice, we don't want to
                // copy out this cache twice which could result in posting an IO with a deleted NativeOverlapped.
                OverlappedCache cache = m_Cache == null ? null : Interlocked.Exchange<OverlappedCache>(ref m_Cache, null);
                if (cache != null)
                {
                    // If overlappedCache is null, just slap it in there.  There's a chance for a conflict,
                    // resulting in a OverlappedCache getting finalized, but it's better than
                    // the interlocked.  This won't be an issue in most 'correct' cases.
                    if (overlappedCache == null)
                    {
                        overlappedCache = cache;
                    }
                    else
                    {
                        OverlappedCache oldCache = Interlocked.Exchange<OverlappedCache>(ref overlappedCache, cache);
                        if (oldCache != null)
                        {
                            oldCache.Free();
                        }
                    }
                }

                ReleaseUnmanagedStructures();
            }
#endif
        }

        private unsafe static void CompletionPortCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped) {
#if DEBUG
            GlobalLog.SetThreadSource(ThreadKinds.CompletionPort);
            using (GlobalLog.SetThreadKind(ThreadKinds.System)) {
#if TRAVE
            try
            {
#endif
#endif
            //
            // Create an Overlapped object out of the native pointer we're provided with.
            // (this will NOT free the unmanaged memory in the native overlapped structure)
            //
            Overlapped callbackOverlapped = Overlapped.Unpack(nativeOverlapped);
            BaseOverlappedAsyncResult asyncResult = (BaseOverlappedAsyncResult)callbackOverlapped.AsyncResult;
            Debug.Assert((IntPtr)nativeOverlapped == asyncResult.m_Cache.NativeOverlapped.DangerousGetHandle(), "Handle mismatch");

            // The AsyncResult must be cleared before the callback is called (i.e. before ExtractCache is called).
            // Not doing so leads to a leak where the pinned cached OverlappedData continues to point to the async result object,
            // which points to the Socket (as well as user data), which points to the OverlappedCache, preventing the OverlappedCache
            // finalizer from freeing the pinned OverlappedData.
            callbackOverlapped.AsyncResult = null;

            object returnObject = null;

            GlobalLog.Assert(!asyncResult.InternalPeekCompleted, "BaseOverlappedAsyncResult#{0}::CompletionPortCallback()|asyncResult.IsCompleted", ValidationHelper.HashString(asyncResult));

            GlobalLog.Print("BaseOverlappedAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::CompletionPortCallback" +
                             " errorCode:" + errorCode.ToString() +
                             " numBytes:" + numBytes.ToString() +
                             " pOverlapped:" + ((int)nativeOverlapped).ToString());

            //
            // complete the IO and invoke the user's callback
            //
            SocketError socketError = (SocketError)errorCode;

            if (socketError != SocketError.Success && socketError != SocketError.OperationAborted)
            {
                // There are cases where passed errorCode does not reflect the details of the underlined socket error.
                // "So as of today, the key is the difference between WSAECONNRESET and ConnectionAborted,
                //  .e.g remote party or network causing the connection reset or something on the local host (e.g. closesocket
                // or receiving data after shutdown (SD_RECV)).  With Winsock/TCP stack rewrite in longhorn, there may
                // be other differences as well."

                Socket socket = asyncResult.AsyncObject as Socket;
                if (socket == null) {
                    socketError = SocketError.NotSocket;
                }
                else if (socket.CleanedUp) {
                    socketError = SocketError.OperationAborted;
                }
                else {
                    try {
                        //
                        // The Async IO completed with a failure.
                        // here we need to call WSAGetOverlappedResult() just so Marshal.GetLastWin32Error() will return the correct error.
                        //
                        SocketFlags ignore;
                        bool success = UnsafeNclNativeMethods.OSSOCK.WSAGetOverlappedResult(
                                socket.SafeHandle,
                                asyncResult.m_Cache.NativeOverlapped,
                                out numBytes,
                                false,
                                out ignore);
                        if (!success)
                        {
                            socketError = (SocketError)Marshal.GetLastWin32Error();
                            GlobalLog.Assert(socketError != 0, "BaseOverlappedAsyncResult#{0}::CompletionPortCallback()|socketError:0 numBytes:{1}", ValidationHelper.HashString(asyncResult), numBytes);
                        }

                        GlobalLog.Assert(!success, "BaseOverlappedAsyncResult#{0}::CompletionPortCallback()|Unexpectedly succeeded. errorCode:{1} numBytes:{2}", ValidationHelper.HashString(asyncResult), errorCode, numBytes);
                    }
                    // CleanedUp check above does not always work since this code is subject to race conditions
                    catch (ObjectDisposedException)
                    {
                        socketError = SocketError.OperationAborted;
                    }
                }
            }
            asyncResult.ErrorCode = (int)socketError;
            returnObject = asyncResult.PostCompletion((int)numBytes);
            asyncResult.ReleaseUnmanagedStructures();
            asyncResult.InvokeCallback(returnObject);
#if DEBUG
#if TRAVE
            }
            catch(Exception exception)
            {
                if (!NclUtilities.IsFatal(exception)){
                    GlobalLog.Assert("BaseOverlappedAsyncResult::CompletionPortCallback", "Exception in completion callback type:" + exception.GetType().ToString() + " message:" + exception.Message);
                }
                throw;
            }
#endif
            }
#endif
        }


        //
        // The overlapped function called (either by the thread pool or the socket)
        // when IO completes. (only called on Win9x)
        //
        private void OverlappedCallback(object stateObject, bool Signaled) {
#if DEBUG
            // GlobalLog.SetThreadSource(ThreadKinds.Worker);  Because of change 1077887, need logic to determine thread type here.
            using (GlobalLog.SetThreadKind(ThreadKinds.System)) {
#endif
            BaseOverlappedAsyncResult asyncResult = (BaseOverlappedAsyncResult)stateObject;

            GlobalLog.Assert(!asyncResult.InternalPeekCompleted, "AcceptOverlappedAsyncResult#{0}::OverlappedCallback()|asyncResult.IsCompleted", ValidationHelper.HashString(asyncResult));
            //
            // the IO completed asynchronously, see if there was a failure the Internal
            // field in the Overlapped structure will be non zero. to optimize the non
            // error case, we look at it without calling WSAGetOverlappedResult().
            //
            uint errorCode = (uint)Marshal.ReadInt32(IntPtrHelper.Add(asyncResult.m_UnmanagedBlob.DangerousGetHandle(),
                                                                      Win32.OverlappedInternalOffset));
            uint numBytes = errorCode!=0 ? unchecked((uint)-1) : (uint)Marshal.ReadInt32(IntPtrHelper.Add(asyncResult.m_UnmanagedBlob.DangerousGetHandle(),
                                                                                                          Win32.OverlappedInternalHighOffset));
            //
            // this will release the unmanaged pin handles and unmanaged overlapped ptr
            //
            asyncResult.ErrorCode = (int)errorCode;
            object returnObject = asyncResult.PostCompletion((int)numBytes);
            asyncResult.ReleaseUnmanagedStructures();
            asyncResult.InvokeCallback(returnObject);
#if DEBUG
            }
#endif
        }

#if DEBUG
        private SocketError m_SavedErrorCode = unchecked((SocketError) 0xDEADBEEF);
        private SafeNativeOverlapped m_InitialNativeOverlapped;
        private SafeNativeOverlapped m_IntermediateNativeOverlapped;
#endif

        //
        // This method is called after an asynchronous call is made for the user,
        // it checks and acts accordingly if the IO:
        // 1) completed synchronously.
        // 2) was pended.
        // 3) failed.
        //
        internal unsafe SocketError CheckAsyncCallOverlappedResult(SocketError errorCode)
        {
#if DEBUG
            m_SavedErrorCode = errorCode;
#endif

            //
            // Check if the Async IO call:
            // 1) was pended.
            // 2) completed synchronously.
            // 3) failed.
            //
            if (m_UseOverlappedIO)
            {
                //
                // we're using overlapped IO under Win9x (or NT with registry setting overriding
                // completion port usage)
                //
                switch (errorCode) {

                case 0:
                case SocketError.IOPending:

                    //
                    // the Async IO call was pended:
                    // Queue our event to the thread pool.
                    //
                    GlobalLog.Assert(m_UnmanagedBlob != null, "BaseOverlappedAsyncResult#{0}::CheckAsyncCallOverlappedResult()|Unmanaged blob isn't allocated.", ValidationHelper.HashString(this));
                    ThreadPool.UnsafeRegisterWaitForSingleObject(
                                                          m_OverlappedEvent,
                                                          new WaitOrTimerCallback(OverlappedCallback),
                                                          this,
                                                          -1,
                                                          true );

                    //
                    // we're done, completion will be asynchronous
                    // in the callback. return
                    //
                    return SocketError.Success;

                default:
                    //
                    // the Async IO call failed:
                    // set the number of bytes transferred to -1 (error)
                    //
                    ErrorCode = (int)errorCode;
                    Result = -1;
                    ReleaseUnmanagedStructures();
                    break;
                }
            }
            else
            {
#if DEBUG
                OverlappedCache cache = m_Cache;
                if (cache != null)
                {
                    SafeNativeOverlapped nativeOverlappedPtr = cache.NativeOverlapped;
                    if (nativeOverlappedPtr != null)
                        m_IntermediateNativeOverlapped = nativeOverlappedPtr;
                }
#endif
                //
                // We're using completion ports under WinNT.  Release one reference on the structures for
                // the main thread.
                //
                ReleaseUnmanagedStructures();

                switch (errorCode) {
                //
                // ignore cases in which a completion packet will be queued:
                // we'll deal with this IO in the callback
                //
                case 0:
                case SocketError.IOPending:
                    //
                    // ignore, do nothing
                    //
                    return SocketError.Success;

                    //
                    // in the remaining cases a completion packet will NOT be queued:
                    // we'll have to call the callback explicitly signaling an error
                    //
                default:
                    //
                    // call the callback with error code
                    //
                    ErrorCode = (int)errorCode;
                    Result = -1;

                    // The AsyncResult must be cleared since the callback isn't going to be called.
                    // Not doing so leads to a leak where the pinned cached OverlappedData continues to point to the async result object,
                    // which points to the Socket (as well as user data) and to the OverlappedCache, preventing the OverlappedCache
                    // finalizer from freeing the pinned OverlappedData.
                    if (m_Cache != null)
                    {
                        // Could be null only if SetUnmanagedStructures weren't called.
                        m_Cache.Overlapped.AsyncResult = null;
                    }

                    ReleaseUnmanagedStructures();  // Additional release for the completion that won't happen.
                    break;
                }
            }
            return errorCode;
        }

        //
        // The following property returns the Win32 unsafe pointer to
        // whichever Overlapped structure we're using for IO.
        //
        internal SafeHandle OverlappedHandle {
            get {
                if (m_UseOverlappedIO)
                {
                    //
                    // on Win9x we allocate our own overlapped structure
                    // and we use a win32 event for IO completion
                    // return the native pointer to unmanaged memory
                    //
                    return (m_UnmanagedBlob == null || m_UnmanagedBlob.IsInvalid)? SafeOverlappedFree.Zero : m_UnmanagedBlob;
                }
                else {
                    //
                    // on WinNT we need to use (due to the current implementation)
                    // an Overlapped object in order to bind the socket to the
                    // ThreadPool's completion port, so return the native handle
                    //
                    return m_Cache == null ? SafeNativeOverlapped.Zero : m_Cache.NativeOverlapped;
                }
            }
        } // OverlappedHandle


        private void ReleaseUnmanagedStructures() {
            if (Interlocked.Decrement(ref m_CleanupCount) == 0) {
                ForceReleaseUnmanagedStructures();
            }
        }

        protected override void Cleanup()
        {
            base.Cleanup();

            // If we get all the way to here and it's still not cleaned up...
            if (m_CleanupCount > 0 && Interlocked.Exchange(ref m_CleanupCount, 0) > 0)
            {
                ForceReleaseUnmanagedStructures();
            }
        }

        // Utility cleanup routine. Frees the overlapped structure.
        // This should be overriden to free pinned and unmanaged memory in the subclass.
        // It needs to also be invoked from the subclass.
        protected virtual void ForceReleaseUnmanagedStructures()
        {
            //
            // free the unmanaged memory if allocated.
            //
            ReleaseGCHandles();
            GC.SuppressFinalize(this);

            if (m_UnmanagedBlob != null && !m_UnmanagedBlob.IsInvalid) {
                m_UnmanagedBlob.Close(true);
                m_UnmanagedBlob = null;
            }

            // This is interlocked because Cleanup() can be called simultaneously with ExtractCache().
            OverlappedCache.InterlockedFree(ref m_Cache);

            if (m_OverlappedEvent != null)
            {
                m_OverlappedEvent.Close();
                m_OverlappedEvent = null;
            }
        }

        ~BaseOverlappedAsyncResult()
        {
            ReleaseGCHandles();
        }

        private void ReleaseGCHandles()
        {
            GCHandle[] gcHandles = m_GCHandles;
            if (gcHandles != null)
            {
                for (int i = 0; i < gcHandles.Length; i++)
                {
                    if (gcHandles[i].IsAllocated)
                    {
                        gcHandles[i].Free();
                    }
                }
            }
        }
    }

    internal class OverlappedCache
    {
        internal Overlapped m_Overlapped;
        internal SafeNativeOverlapped m_NativeOverlapped;
        internal object m_PinnedObjects;
        internal object[] m_PinnedObjectsArray;

        internal OverlappedCache(Overlapped overlapped, object[] pinnedObjectsArray, IOCompletionCallback callback)
        {
            m_Overlapped = overlapped;
            m_PinnedObjects = pinnedObjectsArray;
            m_PinnedObjectsArray = pinnedObjectsArray;

            unsafe
            {
                m_NativeOverlapped = new SafeNativeOverlapped(overlapped.UnsafePack(callback, pinnedObjectsArray));
            }
        }

        internal OverlappedCache(Overlapped overlapped, object pinnedObjects, IOCompletionCallback callback, bool alreadyTriedCast)
        {
            m_Overlapped = overlapped;
            m_PinnedObjects = pinnedObjects;
            m_PinnedObjectsArray = alreadyTriedCast ? null : NclConstants.EmptyObjectArray;

            unsafe
            {
                m_NativeOverlapped = new SafeNativeOverlapped(overlapped.UnsafePack(callback, pinnedObjects));
            }
        }

        internal Overlapped Overlapped
        {
            get
            {
                return m_Overlapped;
            }
        }

        internal SafeNativeOverlapped NativeOverlapped
        {
            get
            {
                return m_NativeOverlapped;
            }
        }

        internal object PinnedObjects
        {
            get
            {
                return m_PinnedObjects;
            }
        }

        internal object[] PinnedObjectsArray
        {
            get
            {
                object[] pinnedObjectsArray = m_PinnedObjectsArray;
                if (pinnedObjectsArray != null && pinnedObjectsArray.Length == 0)
                {
                    pinnedObjectsArray = m_PinnedObjects as object[];
                    if (pinnedObjectsArray != null && pinnedObjectsArray.Length == 0)
                    {
                        m_PinnedObjectsArray = null;
                    }
                    else
                    {
                        m_PinnedObjectsArray = pinnedObjectsArray;
                    }
                }
                return m_PinnedObjectsArray;
            }
        }

        // This must only be called once.
        internal void Free()
        {
            InternalFree();
            GC.SuppressFinalize(this);
        }

        private void InternalFree()
        {
            m_Overlapped = null;
            m_PinnedObjects = null;

            if (m_NativeOverlapped != null)
            {
                if (!m_NativeOverlapped.IsInvalid)
                {
                    m_NativeOverlapped.Dispose();
                }
                m_NativeOverlapped = null;
            }
        }

        internal static void InterlockedFree(ref OverlappedCache overlappedCache)
        {
            OverlappedCache cache = overlappedCache == null ? null : Interlocked.Exchange<OverlappedCache>(ref overlappedCache, null);
            if (cache != null)
            {
                cache.Free();
            }
        }

        ~OverlappedCache()
        {
            if (!NclUtilities.HasShutdownStarted)
            {
                InternalFree();
            }
        }
    }
}
