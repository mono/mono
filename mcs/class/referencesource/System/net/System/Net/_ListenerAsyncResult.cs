//------------------------------------------------------------------------------
// <copyright file="_ListenerAsyncResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.ComponentModel;
    using System.Threading;
    using System.Runtime.InteropServices;

    unsafe class ListenerAsyncResult : LazyAsyncResult
    {
        private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(WaitCallback);

        internal static IOCompletionCallback IOCallback
        {
            get
            {
                return s_IOCallback;
            }
        }

        private AsyncRequestContext m_RequestContext;

        internal ListenerAsyncResult(object asyncObject, object userState, AsyncCallback callback) :
            base(asyncObject, userState, callback)
        {
            m_RequestContext = new AsyncRequestContext(this);
        }

        private static void IOCompleted(ListenerAsyncResult asyncResult, uint errorCode, uint numBytes)
        {
            object result = null;
            try
            {
                GlobalLog.Print("ListenerAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() errorCode:[" + errorCode.ToString() + "] numBytes:[" + numBytes.ToString() + "]");

                if (errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                    errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA)
                {
                    asyncResult.ErrorCode = (int)errorCode;
                    result = new HttpListenerException((int)errorCode);
                }
                else
                {
                    HttpListener httpWebListener = asyncResult.AsyncObject as HttpListener;
                    if (errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
                    {
                        // at this point we have received an unmanaged HTTP_REQUEST and memoryBlob
                        // points to it we need to hook up our authentication handling code here.
                        bool stoleBlob = false;
                        try
                        {
                            if (httpWebListener.ValidateRequest(asyncResult.m_RequestContext))
                            {
                                result = httpWebListener.HandleAuthentication(asyncResult.m_RequestContext, out stoleBlob);
                            }
                        }
                        finally
                        {
                            if (stoleBlob)
                            {
                                // The request has been handed to the user, which means this code can't reuse the blob.  Reset it here.
                                asyncResult.m_RequestContext = result == null ? new AsyncRequestContext(asyncResult) : null;
                            }
                            else
                            {
                                asyncResult.m_RequestContext.Reset(0, 0);
                            }
                        }
                    }
                    else
                    {
                        asyncResult.m_RequestContext.Reset(asyncResult.m_RequestContext.RequestBlob->RequestId, numBytes);
                    }

                    // We need to issue a new request, either because auth failed, or because our buffer was too small the first time.
                    if (result==null)
                    {
                        uint statusCode = asyncResult.QueueBeginGetContext();
                        if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                            statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING)
                        {
                            // someother bad error, possible(?) return values are:
                            // ERROR_INVALID_HANDLE, ERROR_INSUFFICIENT_BUFFER, ERROR_OPERATION_ABORTED
                            result = new HttpListenerException((int)statusCode);
                        }
                    }
                    if (result==null) {
                        return;
                    }
                }

                // complete the async IO and invoke the callback
                GlobalLog.Print("ListenerAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() calling Complete()");
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;

                GlobalLog.Print("ListenerAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() Caught exception:" + exception.ToString());
                result = exception;
            }
            asyncResult.InvokeCallback(result);
        }

        private static unsafe void WaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            // take the ListenerAsyncResult object from the state
            Overlapped callbackOverlapped = Overlapped.Unpack(nativeOverlapped);
            ListenerAsyncResult asyncResult = (ListenerAsyncResult) callbackOverlapped.AsyncResult;

            IOCompleted(asyncResult, errorCode, numBytes);
        }

        internal uint QueueBeginGetContext()
        {
            uint statusCode = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;
            while (true)
            {
                GlobalLog.Print("ListenerAsyncResult#" + ValidationHelper.HashString(this) + "::QueueBeginGetContext() calling UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest RequestId:" + m_RequestContext.RequestBlob->RequestId + " Buffer:0x" + ((IntPtr) m_RequestContext.RequestBlob).ToString("x") + " Size:" + m_RequestContext.Size.ToString());
                (AsyncObject as HttpListener).EnsureBoundHandle();
                uint bytesTransferred = 0;
                statusCode = UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest(
                    (AsyncObject as HttpListener).RequestQueueHandle,
                    m_RequestContext.RequestBlob->RequestId,
                    (uint) UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY,
                    m_RequestContext.RequestBlob,
                    m_RequestContext.Size,
                    &bytesTransferred,
                    m_RequestContext.NativeOverlapped);

                GlobalLog.Print("ListenerAsyncResult#" + ValidationHelper.HashString(this) + "::QueueBeginGetContext() call to UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest returned:" + statusCode);
                if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER && m_RequestContext.RequestBlob->RequestId != 0)
                {
                    // we might get this if somebody stole our RequestId,
                    // set RequestId to 0 and start all over again with the buffer we just allocated
                    // 
                    m_RequestContext.RequestBlob->RequestId = 0;
                    continue;
                }
                else if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA)
                {
                    // the buffer was not big enough to fit the headers, we need
                    // to read the RequestId returned, allocate a new buffer of the required size
                    m_RequestContext.Reset(m_RequestContext.RequestBlob->RequestId, bytesTransferred);
                    continue;
                }
                else if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS && 
                    HttpListener.SkipIOCPCallbackOnSuccess)
                {
                    // IO operation completed synchronously - callback won't be called to signal completion.
                    IOCompleted(this, statusCode, bytesTransferred);
                }
                break;
            }
            return statusCode;
        }

        // Will be called from the base class upon InvokeCallback()
        protected override void Cleanup()
        {
            if (m_RequestContext != null)
            {
                m_RequestContext.ReleasePins();
                m_RequestContext.Close();
            }
            base.Cleanup();
        }
    }
}
