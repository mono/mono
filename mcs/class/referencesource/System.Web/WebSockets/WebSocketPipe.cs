//------------------------------------------------------------------------------
// <copyright file="WebSocketPipe.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.WebSockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using System.Web.Util;

    // Used to send and receive messages over a WebSocket connection

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class WebSocketPipe : IWebSocketPipe {

        // Managed representation (bindable as an anonymous delegate) of work that can be called by the thunk
        private delegate void CompletionCallback(int hrError, int cbIO, bool fUtf8Encoded, bool fFinalFragment, bool fClose);

        // Corresponds to the unmanaged PFN_WEBSOCKET_COMPLETION delegate
        private delegate void CompletionCallbackThunk(int hrError, IntPtr pvCompletionContext, int cbIO, bool fUtf8Encoded, bool fFinalFragment, bool fClose);
        private static readonly CompletionCallbackThunk _asyncThunk = AsyncCallbackThunk; // need to root the delegate itself so not collected while unmanaged code executing

        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"This is a function pointer whose lifetime lasts for the entire duration of this AppDomain. We never need to release it.")]
        private static readonly IntPtr _asyncThunkAddress = Marshal.GetFunctionPointerForDelegate(_asyncThunk);

        private readonly IUnmanagedWebSocketContext _context;
        private readonly IPerfCounters _perfCounters;

        internal WebSocketPipe(IUnmanagedWebSocketContext context, IPerfCounters perfCounters) {
            _context = context;
            _perfCounters = perfCounters;
        }

        public Task WriteFragmentAsync(ArraySegment<byte> buffer, bool isUtf8Encoded, bool isFinalFragment) {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            // The buffer will be read from asynchronously by unmanaged code, so we require that it remain pinned
            PinnedArraySegment<byte> pinnedBuffer = new PinnedArraySegment<byte>(buffer);

            // Callback will always be called (since it is responsible for cleanup), even if completed synchronously
            CompletionCallback callback = (hrError, cbIO, fUtf8Encoded, fFinalFragment, fClose) => {
                try {
                    ThrowExceptionForHR(hrError);
                    tcs.TrySetResult(null); // regular completion
                }
                catch (Exception ex) {
                    tcs.TrySetException(ex); // exceptional completion
                }
                finally {
                    // Always free the buffer to prevent a memory leak
                    pinnedBuffer.Dispose();
                }
            };
            IntPtr completionContext = GCUtil.RootObject(callback);

            // update perf counter with count of data written to wire
            _perfCounters.IncrementCounter(AppPerfCounter.REQUEST_BYTES_OUT_WEBSOCKETS, pinnedBuffer.Count);

            // Call the underlying implementation; WriteFragment should never throw an exception
            int bytesSent = pinnedBuffer.Count;
            bool completionExpected;
            int hr = _context.WriteFragment(
                    pData: pinnedBuffer.Pointer,
                    pcbSent: ref bytesSent,
                    fAsync: true,
                    fUtf8Encoded: isUtf8Encoded,
                    fFinalFragment: isFinalFragment,
                    pfnCompletion: _asyncThunkAddress,
                    pvCompletionContext: completionContext,
                    pfCompletionExpected: out completionExpected);

            if (!completionExpected) {
                // Completed synchronously or error; the thunk and callback together handle cleanup
                AsyncCallbackThunk(hr, completionContext, bytesSent, isUtf8Encoded, isFinalFragment, fClose: false);
            }

            return tcs.Task;
        }

        public Task WriteCloseFragmentAsync(WebSocketCloseStatus closeStatus, string statusDescription) {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            // Callback will always be called (since it is responsible for cleanup), even if completed synchronously
            CompletionCallback callback = (hrError, cbIO, fUtf8Encoded, fFinalFragment, fClose) => {
                try {
                    ThrowExceptionForHR(hrError);
                    tcs.TrySetResult(null); // regular completion
                }
                catch (Exception ex) {
                    tcs.TrySetException(ex); // exceptional completion
                }
            };
            IntPtr completionContext = GCUtil.RootObject(callback);

            // Call the underlying implementation; SendConnectionClose should never throw an exception
            bool completionExpected;
            int hr = _context.SendConnectionClose(
                fAsync: true,
                uStatusCode: (ushort)closeStatus,
                szReason: statusDescription, // don't need to pin string: CLR marshaler handles managed to unmanaged conversion, and IIS makes local copy for duration of async operation
                pfnCompletion: _asyncThunkAddress,
                pvCompletionContext: completionContext,
                pfCompletionExpected: out completionExpected);

            if (!completionExpected) {
                // Completed synchronously or error; the thunk and callback together handle cleanup
                AsyncCallbackThunk(hr, completionContext, cbIO: 0, fUtf8Encoded: true, fFinalFragment: true, fClose: false);
            }

            return tcs.Task;
        }

        public Task<WebSocketReceiveResult> ReadFragmentAsync(ArraySegment<byte> buffer) {
            TaskCompletionSource<WebSocketReceiveResult> tcs = new TaskCompletionSource<WebSocketReceiveResult>();

            // The buffer will be written to asynchronously by unmanaged code, so we require that it remain pinned
            PinnedArraySegment<byte> pinnedBuffer = new PinnedArraySegment<byte>(buffer);

            // Callback will always be called (since it is responsible for cleanup), even if completed synchronously
            CompletionCallback callback = (hrError, cbIO, fUtf8Encoded, fFinalFragment, fClose) => {
                try {
                    ThrowExceptionForHR(hrError);

                    WebSocketCloseStatus? closeStatus = null;
                    string closeStatusDescription = null;
                    WebSocketMessageType messageType = (fUtf8Encoded) ? WebSocketMessageType.Text : WebSocketMessageType.Binary;

                    if (fClose) {
                        // this is a CLOSE frame
                        messageType = WebSocketMessageType.Close;
                        WebSocketCloseStatus statusCode;
                        GetCloseStatus(out statusCode, out closeStatusDescription);
                        closeStatus = statusCode;
                    }
                    else {
                        // this is a data frame, so update perf counter with count of data read from wire
                        _perfCounters.IncrementCounter(AppPerfCounter.REQUEST_BYTES_IN_WEBSOCKETS, cbIO);
                    }

                    tcs.TrySetResult(new WebSocketReceiveResult(
                        count: cbIO,
                        messageType: messageType,
                        endOfMessage: fFinalFragment,
                        closeStatus: closeStatus,
                        closeStatusDescription: closeStatusDescription));
                }
                catch (Exception ex) {
                    tcs.TrySetException(ex); // exceptional completion
                }
                finally {
                    // Always free the buffer to prevent a memory leak
                    pinnedBuffer.Dispose();
                }
            };
            IntPtr completionContext = GCUtil.RootObject(callback);

            // Call the underlying implementation; ReadFragment should never throw an exception
            int bytesRead = pinnedBuffer.Count;
            bool isUtf8Encoded;
            bool isFinalFragment;
            bool isConnectionClose;
            bool completionExpected;
            int hr = _context.ReadFragment(
                pData: pinnedBuffer.Pointer,
                pcbData: ref bytesRead,
                fAsync: true,
                pfUtf8Encoded: out isUtf8Encoded,
                pfFinalFragment: out isFinalFragment,
                pfConnectionClose: out isConnectionClose,
                pfnCompletion: _asyncThunkAddress,
                pvCompletionContext: completionContext,
                pfCompletionExpected: out completionExpected);

            if (!completionExpected) {
                // Completed synchronously or error; the thunk and callback together handle cleanup
                AsyncCallbackThunk(hr, completionContext, bytesRead, isUtf8Encoded, isFinalFragment, isConnectionClose);
            }

            return tcs.Task;
        }

        // Gets the reason (numeric + textual) the client sent a CLOSE frame to the server.
        // Returns false if no reason was given.
        private void GetCloseStatus(out WebSocketCloseStatus closeStatus, out string closeStatusDescription) {
            ushort statusCode;
            IntPtr reasonPtr;
            ushort reasonLength;
            int hr = _context.GetCloseStatus(out statusCode, out reasonPtr, out reasonLength);

            if (hr == HResults.E_NOT_SET) {
                // This HRESULT is special-cased to mean that a status code has not been provided.
                statusCode = 0;
                reasonPtr = IntPtr.Zero;
            }
            else {
                // Any other HRESULTs must be treated as exceptional.
                ThrowExceptionForHR(hr);
            }

            // convert the status code and description string
            closeStatus = (WebSocketCloseStatus)statusCode;
            if (reasonPtr != IntPtr.Zero) {
                unsafe {
                    // return a managed copy of the string (IIS will free the original memory)
                    closeStatusDescription = new String((char*)reasonPtr, 0, reasonLength);
                }
            }
            else {
                closeStatusDescription = null;
            }
        }

        public void CloseTcpConnection() {
            _context.CloseTcpConnection();
        }

        // This thunk dispatches to the appropriate instance continuation when an asynchronous event completes
        private static void AsyncCallbackThunk(int hrError, IntPtr pvCompletionContext, int cbIO, bool fUtf8Encoded, bool fFinalFragment, bool fClose) {
            // Calling UnrootObject also makes the callback and everything it references eligible for garbage collection
            CompletionCallback callback = (CompletionCallback)GCUtil.UnrootObject(pvCompletionContext);
            callback(hrError, cbIO, fUtf8Encoded, fFinalFragment, fClose);
        }

        private static void ThrowExceptionForHR(int hrError) {
            // We should homogenize errors coming from the native layer into a WebSocketException.
            if (hrError < 0) {
                throw new WebSocketException(hrError);
            }
        }

    }
}
