// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  PipeStream
**
**
** Purpose: Base class for pipe streams.
**
**
===========================================================*/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Security;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes {
   
    [Serializable]
    internal enum PipeState {

        // Waiting to connect is the state before a live connection has been established. For named pipes, the 
        // transition from Waiting to Connect to Connected occurs after an explicit request to connect. For 
        // anonymous pipes this occurs as soon as both pipe handles are created (as soon as the anonymous pipe 
        // server ctor has completed).
        WaitingToConnect = 0,

        // For named pipes: the state we're in after calling Connect. For anonymous pipes: occurs as soon as 
        // both handles are created.
        Connected = 1,

        // It’s detected that the other side has broken the connection. Note that this effect isn’t immediate; we 
        // only detect this on the subsequent Win32 call, as indicated by the following error codes: 
        // ERROR_BROKEN_PIPE, ERROR_PIPE_NOT_CONNECTED.
        // A side can cause the connection to break in the following ways:
        //    - Named server calls Disconnect
        //    - One side calls Close/Dispose
        //    - One side closes the handle
        Broken = 2,

        // Valid only for named servers. The server transitions to this state immediately after Disconnect is called. 
        Disconnected = 3,

        // Close/Disposed are the same state. The Close method calls Dispose; both of these close the pipe handle
        // and perform other cleanup. The pipe object is no longer usable after this has been called.
        Closed = 4,
    }

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public abstract class PipeStream : Stream {

        private static readonly bool _canUseAsync = (Environment.OSVersion.Platform == PlatformID.Win32NT);
        [SecurityCritical]
        private unsafe static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(PipeStream.AsyncPSCallback);

        private SafePipeHandle m_handle;
        private bool m_canRead;
        private bool m_canWrite;
        private bool m_isAsync;
        private bool m_isMessageComplete;
        private bool m_isFromExistingHandle;
        private bool m_isHandleExposed;
        private PipeTransmissionMode m_readMode;
        private PipeTransmissionMode m_transmissionMode;
        private PipeDirection m_pipeDirection;
        private int m_outBufferSize;
        private PipeState m_state;

        [System.Security.SecurityCritical]
        static PipeStream()
        {
        }

        protected PipeStream(PipeDirection direction, int bufferSize) {
            if (direction < PipeDirection.In || direction > PipeDirection.InOut) {
                throw new ArgumentOutOfRangeException("direction", SR.GetString(SR.ArgumentOutOfRange_DirectionModeInOutOrInOut));
            }
            if (bufferSize < 0) {
                throw new ArgumentOutOfRangeException("bufferSize", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }

            Init(direction, PipeTransmissionMode.Byte, bufferSize);

        }

        protected PipeStream(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize) {
            if (direction < PipeDirection.In || direction > PipeDirection.InOut) {
                throw new ArgumentOutOfRangeException("direction", SR.GetString(SR.ArgumentOutOfRange_DirectionModeInOutOrInOut));
            }
            if (transmissionMode < PipeTransmissionMode.Byte || transmissionMode > PipeTransmissionMode.Message) {
                throw new ArgumentOutOfRangeException("transmissionMode", SR.GetString(SR.ArgumentOutOfRange_TransmissionModeByteOrMsg));
            }
            if (outBufferSize < 0) {
                throw new ArgumentOutOfRangeException("outBufferSize", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }

            Init(direction, transmissionMode, outBufferSize);
        }

        private void Init(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize) {
            Debug.Assert(direction >= PipeDirection.In && direction <= PipeDirection.InOut, "invalid pipe direction");
            Debug.Assert(transmissionMode >= PipeTransmissionMode.Byte && transmissionMode <= PipeTransmissionMode.Message, "transmissionMode is out of range");
            Debug.Assert(outBufferSize >= 0, "outBufferSize is negative");

            // always defaults to this until overriden
            m_readMode = transmissionMode;
            m_transmissionMode = transmissionMode;

            m_pipeDirection = direction;

            if ((m_pipeDirection & PipeDirection.In) != 0) {
                m_canRead = true;
            }
            if ((m_pipeDirection & PipeDirection.Out) != 0) {
                m_canWrite = true;
            }

            m_outBufferSize = outBufferSize;

            // This should always default to true
            m_isMessageComplete = true;

            m_state = PipeState.WaitingToConnect;
        }

        // Once a PipeStream has a handle ready, it should call this method to set up the PipeStream.  If
        // the pipe is in a connected state already, it should also set the IsConnected (protected) property.
        [System.Security.SecurityCritical]
        protected void InitializeHandle(SafePipeHandle handle, bool isExposed, bool isAsync) {
            Debug.Assert(handle != null, "handle is null");

            // Use Stream's async code for platforms that don't support it.
            isAsync &= _canUseAsync;

            // If the handle is of async type, bind the handle to the ThreadPool so that we can use 
            // the async operations (it's needed so that our native callbacks get called).
            if (isAsync) {
                bool b = false;
                // BindHandle requires UnmanagedCode permission
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();

                try {
                    b = ThreadPool.BindHandle(handle);
                }
                finally {
                    CodeAccessPermission.RevertAssert();
                }
                if (!b) {
                    throw new IOException(SR.GetString(SR.IO_IO_BindHandleFailed));
                }
            }

            m_handle = handle;
            m_isAsync = isAsync;
            // track these separately; m_isHandleExposed will get updated if accessed though the property
            m_isHandleExposed = isExposed;
            m_isFromExistingHandle = isExposed;
        }

        [System.Security.SecurityCritical]
        public override int Read([In, Out] byte[] buffer, int offset, int count) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (buffer.Length - offset < count) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));
            }
            if (!CanRead) {
                __Error.ReadNotSupported();
            }

            CheckReadOperations();

            return ReadCore(buffer, offset, count);
        }

        [System.Security.SecurityCritical]
        private unsafe int ReadCore(byte[] buffer, int offset, int count) {
            Debug.Assert(m_handle != null, "_handle is null");
            Debug.Assert(!m_handle.IsClosed, "_handle is closed");
            Debug.Assert(CanRead, "can't read");
            Debug.Assert(buffer != null, "buffer is null");
            Debug.Assert(offset >= 0, "offset is negative");
            Debug.Assert(count >= 0, "count is negative");

            if (m_isAsync) {
                IAsyncResult result = BeginReadCore(buffer, offset, count, null, null);
                return EndRead(result);
            }

            int hr = 0;
            int r = ReadFileNative(m_handle, buffer, offset, count, null, out hr);

            if (r == -1) {
                // If the other side has broken the connection, set state to Broken and return 0
                if (hr == UnsafeNativeMethods.ERROR_BROKEN_PIPE || 
                    hr == UnsafeNativeMethods.ERROR_PIPE_NOT_CONNECTED) {
                    State = PipeState.Broken;
                    r = 0;   
                }
                else {
                    __Error.WinIOError(hr, String.Empty);
                }
            }
            m_isMessageComplete = (hr != UnsafeNativeMethods.ERROR_MORE_DATA);

            Debug.Assert(r >= 0, "PipeStream's ReadCore is likely broken.");

            return r;
        }

        [System.Security.SecurityCritical]
        [HostProtection(ExternalThreading = true)]
        [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Consistent with security model")]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (buffer.Length - offset < count) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));
            }
            if (!CanRead) {
                __Error.ReadNotSupported();
            }
            CheckReadOperations();

            if (!m_isAsync) {
                // special case when this is called for [....] broken pipes because otherwise Stream's
                // Begin/EndRead hang. Reads return 0 bytes in this case so we can call the user's
                // callback immediately
                if (m_state == PipeState.Broken) {
                    PipeStreamAsyncResult asyncResult = new PipeStreamAsyncResult();
                    asyncResult._handle = m_handle;
                    asyncResult._userCallback = callback;
                    asyncResult._userStateObject = state;
                    asyncResult._isWrite = false;
                    asyncResult.CallUserCallback();
                    return asyncResult;
                }
                else {
                    return base.BeginRead(buffer, offset, count, callback, state);
                }
            }
            else {
                return BeginReadCore(buffer, offset, count, callback, state);
            }
        }

        [System.Security.SecurityCritical]
        unsafe private PipeStreamAsyncResult BeginReadCore(byte[] buffer, int offset, int count,
                AsyncCallback callback, Object state) {
            Debug.Assert(m_handle != null, "_handle is null");
            Debug.Assert(!m_handle.IsClosed, "_handle is closed");
            Debug.Assert(CanRead, "can't read");
            Debug.Assert(buffer != null, "buffer == null");
            Debug.Assert(m_isAsync, "BeginReadCore doesn't work on synchronous file streams!");
            Debug.Assert(offset >= 0, "offset is negative");
            Debug.Assert(count >= 0, "count is negative");

            // Create and store async stream class library specific data in the async result
            PipeStreamAsyncResult asyncResult = new PipeStreamAsyncResult();
            asyncResult._handle = m_handle;
            asyncResult._userCallback = callback;
            asyncResult._userStateObject = state;
            asyncResult._isWrite = false;

            // handle zero-length buffers separately; fixed keyword ReadFileNative doesn't like
            // 0-length buffers. Call user callback and we're done
            if (buffer.Length == 0) {
                asyncResult.CallUserCallback();
            }
            else {

                // For Synchronous IO, I could go with either a userCallback and using
                // the managed Monitor class, or I could create a handle and wait on it.
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                asyncResult._waitHandle = waitHandle;

                // Create a managed overlapped class; set the file offsets later
                Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

                // Pack the Overlapped class, and store it in the async result
                NativeOverlapped* intOverlapped;
                intOverlapped = overlapped.Pack(IOCallback, buffer);


                asyncResult._overlapped = intOverlapped;

                // Queue an async ReadFile operation and pass in a packed overlapped
                int hr = 0;

                int r = ReadFileNative(m_handle, buffer, offset, count, intOverlapped, out hr);

                // ReadFile, the OS version, will return 0 on failure, but this ReadFileNative wrapper
                // returns -1. This will return the following:
                // - On error, r==-1.
                // - On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
                // - On async requests that completed sequentially, r==0
                // 
                // You will NEVER RELIABLY be able to get the number of buffer read back from this call 
                // when using overlapped structures!  You must not pass in a non-null lpNumBytesRead to
                // ReadFile when using overlapped structures!  This is by design NT behavior.
                if (r == -1) {

                    // One side has closed its handle or server disconnected. Set the state to Broken 
                    // and do some cleanup work
                    if (hr == UnsafeNativeMethods.ERROR_BROKEN_PIPE || 
                        hr == UnsafeNativeMethods.ERROR_PIPE_NOT_CONNECTED) {

                        State = PipeState.Broken;

                        // Clear the overlapped status bit for this special case. Failure to do so looks 
                        // like we are freeing a pending overlapped.
                        intOverlapped->InternalLow = IntPtr.Zero;

                        // EndRead will free the Overlapped struct
                        asyncResult.CallUserCallback();
                    }
                    else if (hr != UnsafeNativeMethods.ERROR_IO_PENDING) {
                        __Error.WinIOError(hr, String.Empty);
                    }
                }
            }
            return asyncResult;
        }

        [System.Security.SecurityCritical]
        public unsafe override int EndRead(IAsyncResult asyncResult) {

            // There are 3 significantly different IAsyncResults we'll accept
            // here.  One is from Stream::BeginRead.  The other two are variations
            // on our PipeStreamAsyncResult.  One is from BeginReadCore,
            // while the other is from the BeginRead buffering wrapper.
            if (asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }
            if (!m_isAsync) {
                return base.EndRead(asyncResult);
            }

            PipeStreamAsyncResult afsar = asyncResult as PipeStreamAsyncResult;
            if (afsar == null || afsar._isWrite) {
                __Error.WrongAsyncResult();
            }

            // Ensure we can't get into any ----s by doing an interlocked
            // CompareExchange here.  Avoids corrupting memory via freeing the
            // NativeOverlapped class or GCHandle twice. 
            if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0)) {
                __Error.EndReadCalledTwice();
            }

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            WaitHandle wh = afsar._waitHandle;
            if (wh != null) {
                // We must block to ensure that AsyncPSCallback has completed,
                // and we should close the WaitHandle in here.  AsyncPSCallback
                // and the hand-ported imitation version in COMThreadPool.cpp 
                // are the only places that set this event.
                try {
                    wh.WaitOne();
                    Debug.Assert(afsar._isComplete == true,
                        "FileStream::EndRead - AsyncPSCallback didn't set _isComplete to true!");
                }
                finally {
                    wh.Close();
                }
            }

            // Free memory & GC handles.
            NativeOverlapped* overlappedPtr = afsar._overlapped;
            if (overlappedPtr != null) {
                Overlapped.Free(overlappedPtr);
            }

            // Now check for any error during the read.
            if (afsar._errorCode != 0) {
                WinIOError(afsar._errorCode);
            }

            // set message complete to true if the pipe is broken as well; need this to signal to readers
            // to stop reading
            m_isMessageComplete = m_state == PipeState.Broken || 
                                  afsar._isMessageComplete;

            return afsar._numBytes;
        }

        [System.Security.SecurityCritical]
        public override void Write(byte[] buffer, int offset, int count) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (buffer.Length - offset < count) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));
            }
            if (!CanWrite) {
                __Error.WriteNotSupported();
            }
            CheckWriteOperations();

            WriteCore(buffer, offset, count);

            return;
        }

        [System.Security.SecurityCritical]
        private unsafe void WriteCore(byte[] buffer, int offset, int count) {
            Debug.Assert(!m_handle.IsClosed, "_handle is closed");
            Debug.Assert(CanWrite, "can't write");
            Debug.Assert(buffer != null, "buffer is null");
            Debug.Assert(offset >= 0, "offset is negative");
            Debug.Assert(count >= 0, "count is negative");

            if (m_isAsync) {
                IAsyncResult result = BeginWriteCore(buffer, offset, count, null, null);
                EndWrite(result);
                return;
            }

            int hr = 0;
            int r = WriteFileNative(m_handle, buffer, offset, count, null, out hr);

            if (r == -1) { 
                WinIOError(hr); 
            }
            Debug.Assert(r >= 0, "PipeStream's WriteCore is likely broken.");
            return;
        }

        [System.Security.SecurityCritical]
        [HostProtection(ExternalThreading = true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer", SR.GetString(SR.ArgumentNull_Buffer));
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            if (buffer.Length - offset < count) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));
            }
            if (!CanWrite) {
                __Error.WriteNotSupported();
            }
            CheckWriteOperations();

            if (!m_isAsync) {
                return base.BeginWrite(buffer, offset, count, callback, state);
            }
            else {
                return BeginWriteCore(buffer, offset, count, callback, state);
            }
        }

        [System.Security.SecurityCritical]
        unsafe private PipeStreamAsyncResult BeginWriteCore(byte[] buffer, int offset, int count,
                AsyncCallback callback, Object state) {
            Debug.Assert(!m_handle.IsClosed, "_handle is closed");
            Debug.Assert(CanWrite, "can't write");
            Debug.Assert(buffer != null, "buffer == null");
            Debug.Assert(m_isAsync, "BeginWriteCore doesn't work on synchronous file streams!");
            Debug.Assert(offset >= 0, "offset is negative");
            Debug.Assert(count >= 0, "count is negative");

            // Create and store async stream class library specific data in the async result
            PipeStreamAsyncResult asyncResult = new PipeStreamAsyncResult();
            asyncResult._userCallback = callback;
            asyncResult._userStateObject = state;
            asyncResult._isWrite = true;
            asyncResult._handle = m_handle;

            // fixed doesn't work well with zero length arrays. Set the zero-byte flag in case
            // caller needs to do any cleanup
            if (buffer.Length == 0) {
                //intOverlapped->InternalLow = IntPtr.Zero;

                // EndRead will free the Overlapped struct
                asyncResult.CallUserCallback();
            }
            else {

                // For Synchronous IO, I could go with either a userCallback and using the managed 
                // Monitor class, or I could create a handle and wait on it.
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                asyncResult._waitHandle = waitHandle;

                // Create a managed overlapped class; set the file offsets later
                Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

                // Pack the Overlapped class, and store it in the async result
                NativeOverlapped* intOverlapped = overlapped.Pack(IOCallback, buffer);
                asyncResult._overlapped = intOverlapped;

                int hr = 0;

                // Queue an async WriteFile operation and pass in a packed overlapped
                int r = WriteFileNative(m_handle, buffer, offset, count, intOverlapped, out hr);

                // WriteFile, the OS version, will return 0 on failure, but this WriteFileNative 
                // wrapper returns -1. This will return the following:
                // - On error, r==-1.
                // - On async requests that are still pending, r==-1 w/ hr==ERROR_IO_PENDING
                // - On async requests that completed sequentially, r==0
                // 
                // You will NEVER RELIABLY be able to get the number of buffer written back from this 
                // call when using overlapped structures!  You must not pass in a non-null 
                // lpNumBytesWritten to WriteFile when using overlapped structures!  This is by design 
                // NT behavior.
                if (r == -1) {
                    if (hr != UnsafeNativeMethods.ERROR_IO_PENDING) {
                        // Clean up
                        if (intOverlapped != null) Overlapped.Free(intOverlapped);
                        WinIOError(hr);
                    }
                }
            }

            return asyncResult;
        }

        [System.Security.SecurityCritical]
        public unsafe override void EndWrite(IAsyncResult asyncResult) {
            if (asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }

            if (!m_isAsync) {
                base.EndWrite(asyncResult);
                return;
            }

            PipeStreamAsyncResult afsar = asyncResult as PipeStreamAsyncResult;
            if (afsar == null || !afsar._isWrite) {
                __Error.WrongAsyncResult();
            }

            // Ensure we can't get into any ----s by doing an interlocked
            // CompareExchange here.  Avoids corrupting memory via freeing the
            // NativeOverlapped class or GCHandle twice.  -- 
            if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0)) {
                __Error.EndWriteCalledTwice();
            }

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            WaitHandle wh = afsar._waitHandle;
            if (wh != null) {
                // We must block to ensure that AsyncPSCallback has completed,
                // and we should close the WaitHandle in here.  AsyncPSCallback
                // and the hand-ported imitation version in COMThreadPool.cpp 
                // are the only places that set this event.
                try {
                    wh.WaitOne();
                    Debug.Assert(afsar._isComplete == true, "PipeStream::EndWrite - AsyncPSCallback didn't set _isComplete to true!");
                }
                finally {
                    wh.Close();
                }
            }

            // Free memory & GC handles.
            NativeOverlapped* overlappedPtr = afsar._overlapped;
            if (overlappedPtr != null) {
                Overlapped.Free(overlappedPtr);
            }

            // Now check for any error during the write.
            if (afsar._errorCode != 0) {
                WinIOError(afsar._errorCode);
            }

            // Number of buffer written is afsar._numBytes.
            return;
        }

        [System.Security.SecurityCritical]
        private unsafe int ReadFileNative(SafePipeHandle handle, byte[] buffer, int offset, int count,
                NativeOverlapped* overlapped, out int hr) {
            Debug.Assert(handle != null, "handle is null");
            Debug.Assert(offset >= 0, "offset is negative");
            Debug.Assert(count >= 0, "count is negative");
            Debug.Assert(buffer != null, "buffer == null");
            Debug.Assert((m_isAsync && overlapped != null) || (!m_isAsync && overlapped == null), "Async IO parameter ----up in call to ReadFileNative.");
            Debug.Assert(buffer.Length - offset >= count, "offset + count >= buffer length");

            // You can't use the fixed statement on an array of length 0. Note that async callers
            // check to avoid calling this first, so they can call user's callback
            if (buffer.Length == 0) {
                hr = 0;
                return 0;
            }

            int r = 0;
            int numBytesRead = 0;

            fixed (byte* p = buffer) {
                if (m_isAsync) {
                    r = UnsafeNativeMethods.ReadFile(handle, p + offset, count, IntPtr.Zero, overlapped);
                }
                else {
                    r = UnsafeNativeMethods.ReadFile(handle, p + offset, count, out numBytesRead, IntPtr.Zero);
                }
            }

            if (r == 0) {
                // We should never silently ---- an error here without some
                // extra work.  We must make sure that BeginReadCore won't return an 
                // IAsyncResult that will cause EndRead to block, since the OS won't
                // call AsyncPSCallback for us.  
                hr = Marshal.GetLastWin32Error();

                // In message mode, the ReadFile can inform us that there is more data to come.
                if (hr == UnsafeNativeMethods.ERROR_MORE_DATA) {
                    return numBytesRead;
                }
                
                return -1;
            }
            else {
                hr = 0;
            }

            return numBytesRead;
        }

        [System.Security.SecurityCritical]
        private unsafe int WriteFileNative(SafePipeHandle handle, byte[] buffer, int offset, int count,
                NativeOverlapped* overlapped, out int hr) {
            Debug.Assert(handle != null, "handle is null");
            Debug.Assert(offset >= 0, "offset is negative");
            Debug.Assert(count >= 0, "count is negative");
            Debug.Assert(buffer != null, "buffer == null");
            Debug.Assert((m_isAsync && overlapped != null) || (!m_isAsync && overlapped == null), "Async IO parameter ----up in call to WriteFileNative.");
            Debug.Assert(buffer.Length - offset >= count, "offset + count >= buffer length");

            // You can't use the fixed statement on an array of length 0. Note that async callers
            // check to avoid calling this first, so they can call user's callback
            if (buffer.Length == 0) {
                hr = 0;
                return 0;
            }

            int numBytesWritten = 0;
            int r = 0;

            fixed (byte* p = buffer) {
                if (m_isAsync) {
                    r = UnsafeNativeMethods.WriteFile(handle, p + offset, count, IntPtr.Zero, overlapped);
                }
                else {
                    r = UnsafeNativeMethods.WriteFile(handle, p + offset, count, out numBytesWritten, IntPtr.Zero);
                }
            }

            if (r == 0) {
                // We should never silently ---- an error here without some
                // extra work.  We must make sure that BeginWriteCore won't return an 
                // IAsyncResult that will cause EndWrite to block, since the OS won't
                // call AsyncPSCallback for us.  
                hr = Marshal.GetLastWin32Error();
                return -1;
            }
            else {
                hr = 0;
            }

            return numBytesWritten;
        }

        // Reads a byte from the pipe stream.  Returns the byte cast to an int
        // or -1 if the connection has been broken.
        [System.Security.SecurityCritical]
        public override int ReadByte() {
            CheckReadOperations();
            if (!CanRead) {
                __Error.ReadNotSupported();
            }

            byte[] buffer = new byte[1];
            int n = ReadCore(buffer, 0, 1);

            if (n == 0) { return -1; }
            else return (int)buffer[0];
        }

        [System.Security.SecurityCritical]
        public override void WriteByte(byte value) {
            CheckWriteOperations();
            if (!CanWrite) {
                __Error.WriteNotSupported();
            }

            byte[] buffer = new byte[1];
            buffer[0] = value;
            WriteCore(buffer, 0, 1);
        }

        // Does nothing on PipeStreams.  We cannot call UnsafeNativeMethods.FlushFileBuffers here because we can deadlock
        // if the other end of the pipe is no longer interested in reading from the pipe. 
        [System.Security.SecurityCritical]
        public override void Flush() {
            CheckWriteOperations();
            if (!CanWrite) {
                __Error.WriteNotSupported();
            }
        }

        // Blocks until the other end of the pipe has read in all written buffer.
        [System.Security.SecurityCritical]
        public void WaitForPipeDrain() {
            CheckWriteOperations();
            if (!CanWrite) {
                __Error.WriteNotSupported();
            }

            // Block until other end of the pipe has read everything.
            if (!UnsafeNativeMethods.FlushFileBuffers(m_handle)) {
                WinIOError(Marshal.GetLastWin32Error());
            }
        }

        [System.Security.SecurityCritical]
        protected override void Dispose(bool disposing) {
            try {
                // Nothing will be done differently based on whether we are 
                // disposing vs. finalizing.  
                if (m_handle != null && !m_handle.IsClosed) {
                    m_handle.Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }

            m_state = PipeState.Closed;
        }

        // ********************** Public Properties *********************** //

        // Apis use coarser definition of connected, but these map to internal 
        // Connected/Disconnected states. Note that setter is protected; only
        // intended to be called by custom PipeStream concrete children
        public bool IsConnected {
            get { 
                return State == PipeState.Connected; 
            }
            protected set {
                m_state = (value) ? PipeState.Connected : PipeState.Disconnected;
            }
        }

        public bool IsAsync {
            get { return m_isAsync; }
        }

        // Set by the most recent call to Read or EndRead.  Will be false if there are more buffer in the
        // message, otherwise it is set to true. 
        public bool IsMessageComplete {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
            get {
                // omitting pipe broken exception to allow reader to finish getting message
                if (m_state == PipeState.WaitingToConnect) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotYetConnected));
                }
                if (m_state == PipeState.Disconnected) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeDisconnected));
                }
                if (m_handle == null) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
                }

                if (m_state == PipeState.Closed) {
                    __Error.PipeNotOpen();
                }
                if (m_handle.IsClosed) {
                    __Error.PipeNotOpen();
                }
                // don't need to check transmission mode; just care about read mode. Always use
                // cached mode; otherwise could throw for valid message when other side is shutting down
                if (m_readMode != PipeTransmissionMode.Message) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeReadModeNotMessage));
                }

                return m_isMessageComplete;
            }
        }

        // Gets the transmission mode for the pipe.  This is virtual so that subclassing types can 
        // override this in cases where only one mode is legal (such as anonymous pipes)
        public virtual PipeTransmissionMode TransmissionMode {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
            get {
                CheckPipePropertyOperations();

                if (m_isFromExistingHandle) {
                    int pipeFlags;
                    if (!UnsafeNativeMethods.GetNamedPipeInfo(m_handle, out pipeFlags, UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL,
                            UnsafeNativeMethods.NULL)) {
                        WinIOError(Marshal.GetLastWin32Error());
                    }
                    if ((pipeFlags & UnsafeNativeMethods.PIPE_TYPE_MESSAGE) != 0) {
                        return PipeTransmissionMode.Message;
                    }
                    else {
                        return PipeTransmissionMode.Byte;
                    }
                }
                else {
                    return m_transmissionMode;
                }
            }
        }

        // Gets the buffer size in the inbound direction for the pipe. This checks if pipe has read
        // access. If that passes, call to GetNamedPipeInfo will succeed.
        public virtual int InBufferSize {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
            get {
                CheckPipePropertyOperations();
                if (!CanRead) {
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_UnreadableStream));
                }

                int inBufferSize;
                if (!UnsafeNativeMethods.GetNamedPipeInfo(m_handle, UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL, out inBufferSize, UnsafeNativeMethods.NULL)) {
                    WinIOError(Marshal.GetLastWin32Error());
                }

                return inBufferSize;
            }
        }

        // Gets the buffer size in the outbound direction for the pipe. This uses cached version 
        // if it's an outbound only pipe because GetNamedPipeInfo requires read access to the pipe.
        // However, returning cached is good fallback, especially if user specified a value in 
        // the ctor.
        public virtual int OutBufferSize {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
            get {
                CheckPipePropertyOperations();
                if (!CanWrite) {
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_UnwritableStream));
                }

                int outBufferSize;

                // Use cached value if direction is out; otherwise get fresh version
                if (m_pipeDirection == PipeDirection.Out) {
                    outBufferSize = m_outBufferSize;
                }
                else if (!UnsafeNativeMethods.GetNamedPipeInfo(m_handle, UnsafeNativeMethods.NULL, out outBufferSize,
                    UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL)) {
                    WinIOError(Marshal.GetLastWin32Error());
                }

                return outBufferSize;
            }
        }

        public virtual PipeTransmissionMode ReadMode {
            [System.Security.SecurityCritical]
            get {
                CheckPipePropertyOperations();

                // get fresh value if it could be stale
                if (m_isFromExistingHandle || IsHandleExposed) {
                    UpdateReadMode();
                }
                return m_readMode;
            }
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
            set {
                // Nothing fancy here.  This is just a wrapper around the Win32 API.  Note, that NamedPipeServerStream
                // and the AnonymousPipeStreams override this.

                CheckPipePropertyOperations();
                if (value < PipeTransmissionMode.Byte || value > PipeTransmissionMode.Message) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ArgumentOutOfRange_TransmissionModeByteOrMsg));
                }

                unsafe {
                    int pipeReadType = (int)value << 1;
                    if (!UnsafeNativeMethods.SetNamedPipeHandleState(m_handle, &pipeReadType, UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL)) {
                        WinIOError(Marshal.GetLastWin32Error());
                    }
                    else {
                        m_readMode = value;
                    }
                }
            }
        }

        /// <summary>
        /// Determine pipe read mode from Win32 
        /// </summary>
        [System.Security.SecurityCritical]
        private void UpdateReadMode() {
            int flags;
            if (!UnsafeNativeMethods.GetNamedPipeHandleState(SafePipeHandle, out flags, UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL,
                    UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL, 0)) {
                WinIOError(Marshal.GetLastWin32Error());
            }

            if ((flags & UnsafeNativeMethods.PIPE_READMODE_MESSAGE) != 0) {
                m_readMode = PipeTransmissionMode.Message;
            }
            else {
                m_readMode = PipeTransmissionMode.Byte;
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [System.Security.SecurityCritical]
        public PipeSecurity GetAccessControl() {
            if (m_state == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
            if (m_handle == null) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
            }
            if (m_handle.IsClosed) {
                __Error.PipeNotOpen();
            }

            return new PipeSecurity(m_handle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        [System.Security.SecurityCritical]
        public void SetAccessControl(PipeSecurity pipeSecurity) {
            if (pipeSecurity == null) {
                throw new ArgumentNullException("pipeSecurity");
            }
            CheckPipePropertyOperations();

            pipeSecurity.Persist(m_handle);
        }

        public SafePipeHandle SafePipeHandle {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
            get {
                if (m_handle == null) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
                }
                if (m_handle.IsClosed) {
                    __Error.PipeNotOpen();
                }

                m_isHandleExposed = true;
                return m_handle;
            }
        }

        internal SafePipeHandle InternalHandle {
            [System.Security.SecurityCritical]
            get { 
                return m_handle; 
            }
        }

        protected bool IsHandleExposed {
            get { 
                return m_isHandleExposed; 
            }
        }

        public override bool CanRead {
            [Pure]
            get { 
                return m_canRead; 
            }
        }

        public override bool CanWrite {
            [Pure]
            get { 
                return m_canWrite; 
            }
        }

        public override bool CanSeek {
            [Pure]
            get { 
                return false; 
            }
        }

        public override long Length {
            get {
                __Error.SeekNotSupported();
                return 0;
            }
        }

        public override long Position {
            get {
                __Error.SeekNotSupported();
                return 0;
            }
            set {
                __Error.SeekNotSupported();
            }
        }

        public override void SetLength(long value) {
            __Error.SeekNotSupported();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            __Error.SeekNotSupported();
            return 0;
        }

        // anonymous pipe ends and named pipe server can get/set properties when broken 
        // or connected. Named client overrides
        [System.Security.SecurityCritical]
        protected virtual internal void CheckPipePropertyOperations() {

            if (m_handle == null) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
            }

            // these throw object disposed
            if (m_state == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
            if (m_handle.IsClosed) {
                __Error.PipeNotOpen();
            }
        }

        // Reads can be done in Connected and Broken. In the latter,
        // read returns 0 bytes
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Consistent with security model")]
        protected internal void CheckReadOperations() {

            // Invalid operation
            if (m_state == PipeState.WaitingToConnect) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotYetConnected));
            }
            if (m_state == PipeState.Disconnected) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeDisconnected));
            }
            if (m_handle == null) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
            }

            // these throw object disposed
            if (m_state == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
            if (m_handle.IsClosed) {
                __Error.PipeNotOpen();
            }
        }

        // Writes can only be done in connected state
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Consistent with security model")]
        protected internal void CheckWriteOperations() {

            // Invalid operation
            if (m_state == PipeState.WaitingToConnect) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotYetConnected));
            }
            if (m_state == PipeState.Disconnected) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeDisconnected));
            }
            if (m_handle == null) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
            }

            // IOException
            if (m_state == PipeState.Broken) {
                throw new IOException(SR.GetString(SR.IO_IO_PipeBroken));
            }

            // these throw object disposed
            if (m_state == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
            if (m_handle.IsClosed) {
                __Error.PipeNotOpen();
            }
        }

        /// <summary>
        /// Filter out all pipe related errors and do some cleanup before calling __Error.WinIOError.
        /// </summary>
        /// <param name="errorCode"></param>
        [System.Security.SecurityCritical]
        internal void WinIOError(int errorCode) {
            
            if (errorCode == UnsafeNativeMethods.ERROR_BROKEN_PIPE ||
                errorCode == UnsafeNativeMethods.ERROR_PIPE_NOT_CONNECTED ||
                errorCode == UnsafeNativeMethods.ERROR_NO_DATA
                ) {
                // Other side has broken the connection
                m_state = PipeState.Broken;
                throw new IOException(SR.GetString(SR.IO_IO_PipeBroken), UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));

            }
            else if (errorCode == UnsafeNativeMethods.ERROR_HANDLE_EOF) {
                __Error.EndOfFile();
            }
            else {
                // For invalid handles, detect the error and mark our handle
                // as invalid to give slightly better error messages.  Also
                // help ensure we avoid handle recycling bugs.
                if (errorCode == UnsafeNativeMethods.ERROR_INVALID_HANDLE) {
                    m_handle.SetHandleAsInvalid();
                    m_state = PipeState.Broken;
                }

                __Error.WinIOError(errorCode, String.Empty);
            }
        }

        internal PipeState State {
            get {
                return m_state;
            }
            set {
                m_state = value;
            }
        }

        // ************************ Static Methods ************************ //

        [System.Security.SecurityCritical]
        internal unsafe static UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability, PipeSecurity pipeSecurity, out Object pinningHandle) {
            pinningHandle = null;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = null;
            if ((inheritability & HandleInheritability.Inheritable) != 0 || pipeSecurity != null) {
                secAttrs = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);

                if ((inheritability & HandleInheritability.Inheritable) != 0) {
                    secAttrs.bInheritHandle = 1;
                }

                // For ACLs, get the security descriptor from the PipeSecurity.
                if (pipeSecurity != null) {
                    byte[] sd = pipeSecurity.GetSecurityDescriptorBinaryForm();
                    pinningHandle = GCHandle.Alloc(sd, GCHandleType.Pinned);
                    fixed (byte* pSecDescriptor = sd) {
                        secAttrs.pSecurityDescriptor = pSecDescriptor;
                    }
                }
            }
            return secAttrs;
        }

        [System.Security.SecurityCritical]
        internal static UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability) {
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = null;
            if ((inheritability & HandleInheritability.Inheritable) != 0) {
                secAttrs = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);
                secAttrs.bInheritHandle = 1;
            }
            return secAttrs;
        }

        // When doing IO asynchronously (i.e., m_isAsync==true), this callback is 
        // called by a free thread in the threadpool when the IO operation 
        // completes.  
        [System.Security.SecurityCritical]
        unsafe private static void AsyncPSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped) {
            // Unpack overlapped
            Overlapped overlapped = Overlapped.Unpack(pOverlapped);
            // Free the overlapped struct in EndRead/EndWrite.

            // Extract async result from overlapped 
            PipeStreamAsyncResult asyncResult = (PipeStreamAsyncResult)overlapped.AsyncResult;
            asyncResult._numBytes = (int)numBytes;

            // Allow async read to finish
            if (!asyncResult._isWrite) {
                if (errorCode == UnsafeNativeMethods.ERROR_BROKEN_PIPE ||
                    errorCode == UnsafeNativeMethods.ERROR_PIPE_NOT_CONNECTED ||
                    errorCode == UnsafeNativeMethods.ERROR_NO_DATA) {

                    errorCode = 0;
                    numBytes = 0;
                }
            }

            // For message type buffer.
            if (errorCode == UnsafeNativeMethods.ERROR_MORE_DATA) {
                errorCode = 0;
                asyncResult._isMessageComplete = false;
            }
            else {
                asyncResult._isMessageComplete = true;
            }

            asyncResult._errorCode = (int)errorCode;

            // Call the user-provided callback.  It can and often should
            // call EndRead or EndWrite.  There's no reason to use an async 
            // delegate here - we're already on a threadpool thread.  
            // IAsyncResult's completedSynchronously property must return
            // false here, saying the user callback was called on another thread.
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;

            // The OS does not signal this event.  We must do it ourselves.
            ManualResetEvent wh = asyncResult._waitHandle;
            if (wh != null) {
                Debug.Assert(!wh.SafeWaitHandle.IsClosed, "ManualResetEvent already closed!");
                bool r = wh.Set();
                Debug.Assert(r, "ManualResetEvent::Set failed!");
                if (!r) {
                    __Error.WinIOError();
                }
            }

            AsyncCallback callback = asyncResult._userCallback;
            if (callback != null) {
                callback(asyncResult);
            }
        }
    }

    unsafe internal sealed class PipeStreamAsyncResult : IAsyncResult {
        internal AsyncCallback _userCallback;   // User callback
        internal Object _userStateObject;
        internal ManualResetEvent _waitHandle;
        [SecurityCritical]
        internal SafePipeHandle _handle;        // For cancellation support.
        [SecurityCritical]
        internal NativeOverlapped* _overlapped;

        internal int _EndXxxCalled;             // Whether we've called EndXxx already.
        internal int _numBytes;                 // number of buffer read OR written
        internal int _errorCode;

        internal bool _isMessageComplete;
        internal bool _isWrite;                 // Whether this is a read or a write
        internal bool _isComplete;              // Value for IsCompleted property        
        internal bool _completedSynchronously;  // Which thread called callback

        public Object AsyncState {
            get { return _userStateObject; }
        }

        public bool IsCompleted {
            get { return _isComplete; }
        }

        public WaitHandle AsyncWaitHandle {
            [System.Security.SecurityCritical]
            get {
                if (_waitHandle == null) {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    if (_overlapped != null && _overlapped->EventHandle != IntPtr.Zero) {
                        mre.SafeWaitHandle = new SafeWaitHandle(_overlapped->EventHandle, true);
                    }
                    if (_isComplete) {
                        mre.Set();
                    }
                    _waitHandle = mre;
                }
                return _waitHandle;
            }
        }

        public bool CompletedSynchronously {
            get { return _completedSynchronously; }
        }

        private void CallUserCallbackWorker(Object callbackState) {
            _isComplete = true;
            if (_waitHandle != null) {
                _waitHandle.Set();
            }
            _userCallback(this);
        }

        internal void CallUserCallback() {
            if (_userCallback != null) {
                _completedSynchronously = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(CallUserCallbackWorker));
            }
            else {
                _isComplete = true;
                if (_waitHandle != null) {
                    _waitHandle.Set();
                }
            }
        }
    }

}


