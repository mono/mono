//------------------------------------------------------------------------------
// <copyright file="_Connection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Security;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Diagnostics.CodeAnalysis;

    internal enum ReadState {
        Start,
        StatusLine, // about to parse status line
        Headers,    // reading headers
        Data        // now read data
    }

    internal enum DataParseStatus {
        NeedMoreData = 0,   // need more data
        ContinueParsing,    // continue parsing
        Done,               // done
        Invalid,            // bad data format
        DataTooBig,         // data exceeds the allowed size
    }

    internal enum WriteBufferState {
        Disabled,
        Headers,
        Buffer,
        Playback,
    }

    // The enum lietrals will be displayed to the user in the exception message
    internal enum WebParseErrorSection {
        Generic,
        ResponseHeader,
        ResponseStatusLine,
        ResponseBody
    }

    // The enum literal will be used to look up an error string in the resource file
    internal enum WebParseErrorCode {
        Generic,
        InvalidHeaderName,
        InvalidContentLength,
        IncompleteHeaderLine,
        CrLfError,
        InvalidChunkFormat,
        UnexpectedServerResponse
    }

    // Only defined for DataParseStatus.Invalid
    struct WebParseError {
        public WebParseErrorSection  Section;
        public WebParseErrorCode     Code;
    }


    struct TunnelStateObject {
        internal TunnelStateObject(HttpWebRequest r, Connection c){
            Connection = c;
            OriginalRequest = r;
        }

        internal Connection Connection;
        internal HttpWebRequest OriginalRequest;
    }

    //
    // ConnectionReturnResult - used to spool requests that have been completed,
    //  and need to be notified.
    //

    internal class ConnectionReturnResult {

        private static readonly WaitCallback s_InvokeConnectionCallback = new WaitCallback(InvokeConnectionCallback);

        private struct RequestContext {
            internal HttpWebRequest      Request;
            internal object              CoreResponse;

            internal RequestContext(HttpWebRequest request, object coreResponse)
            {
                Request = request;
                CoreResponse = coreResponse;
            }
        }

        private List<RequestContext>  m_Context;

        internal ConnectionReturnResult()
        {
            m_Context = new List<RequestContext>(5);
        }

        internal ConnectionReturnResult(int capacity)
        {
            m_Context = new List<RequestContext>(capacity);
        }

        internal bool IsNotEmpty {
            get {
                return m_Context.Count != 0;
            }
        }

        internal static void Add(ref ConnectionReturnResult returnResult, HttpWebRequest request, CoreResponseData coreResponseData)
        {
            if (coreResponseData == null)
                throw new InternalException(); //This may cause duplicate requests if we let it through in retail

            if (returnResult == null) {
                returnResult = new ConnectionReturnResult();
            }

#if DEBUG
            //This may cause duplicate requests if we let it through in retail but it's may be expensive to catch here
            for (int j = 0; j <  returnResult.m_Context.Count; ++j)
                if ((object)returnResult.m_Context[j].Request == (object) request)
                    throw new InternalException();
#endif

            returnResult.m_Context.Add(new RequestContext(request, coreResponseData));
        }

        internal static void AddExceptionRange(ref ConnectionReturnResult returnResult, HttpWebRequest [] requests, Exception exception)
        {
            AddExceptionRange(ref returnResult, requests, exception, exception);
        }
        internal static void AddExceptionRange(ref ConnectionReturnResult returnResult, HttpWebRequest [] requests, Exception exception, Exception firstRequestException)
        {

            //This may cause duplicate requests if we let it through in retail
            if (exception == null)
                throw new InternalException();

            if (returnResult == null) {
                returnResult = new ConnectionReturnResult(requests.Length);
            }
            // "abortedRequestExeption" is assigned to the "abortedRequest" or to the very first request if the latest is null
            // Everyone else will get "exception"
            for (int i = 0; i < requests.Length; ++i)
            {
#if DEBUG
                //This may cause duplicate requests if we let it through in retail but it's may be expensive to catch here
                for (int j = 0; j <  returnResult.m_Context.Count; ++j)
                    if ((object)returnResult.m_Context[j].Request == (object) requests[i])
                        throw new InternalException();
#endif

                if (i == 0)
                    returnResult.m_Context.Add(new RequestContext(requests[i], firstRequestException));
                else
                    returnResult.m_Context.Add(new RequestContext(requests[i], exception));
            }
        }

        internal static void SetResponses(ConnectionReturnResult returnResult) {
            if (returnResult==null){
                return;
            }

            GlobalLog.Print("ConnectionReturnResult#" + ValidationHelper.HashString(returnResult) + "::SetResponses() count=" + returnResult.m_Context.Count.ToString());
            for (int i = 0; i < returnResult.m_Context.Count; i++)
            {
                try {
                    HttpWebRequest request = returnResult.m_Context[i].Request;
#if DEBUG
                    CoreResponseData coreResponseData = returnResult.m_Context[i].CoreResponse as CoreResponseData;
                    if (coreResponseData == null)
                        GlobalLog.DebugRemoveRequest(request);
#endif
                    request.SetAndOrProcessResponse(returnResult.m_Context[i].CoreResponse);
                }
                catch(Exception e) {
                    //ASYNCISSUE
                    // on error, with more than one callback need to queue others off to another thread

                    GlobalLog.Print("ConnectionReturnResult#" + ValidationHelper.HashString(returnResult) + "::Exception"+e);
                    returnResult.m_Context.RemoveRange(0,(i+1));
                    if (returnResult.m_Context.Count > 0)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(s_InvokeConnectionCallback, returnResult);
                    }
                    throw;
                }
            }

            returnResult.m_Context.Clear();
        }

        private static void InvokeConnectionCallback(object objectReturnResult)
        {
            ConnectionReturnResult returnResult = (ConnectionReturnResult)objectReturnResult;
            SetResponses(returnResult);
        }
    }

    //
    // Connection - this is the Connection used to parse
    //   server responses, queue requests, and pipeline requests
    //
    internal class Connection : PooledStream {

        //
        // thread statics - these values must be per thread, because
        //  other requests and operations can take place concurrently on this Connection.
        //  Our concern is to make sure that a nested call does not get confused with an
        //  operation on another thread.   Parameter passing cannot be used, because
        //  the call stack may exit and then reenter the same Connection object.
        //
        [ThreadStatic]
        private static int t_SyncReadNesting;

        private const int CRLFSize = 2;
        private const long c_InvalidContentLength = -2L;
        
        //
        // Buffer manager that allocates and reuses 4k buffers.
        //
        private const int CachedBufferSize = 4096;
        private static PinnableBufferCache s_PinnableBufferCache = new PinnableBufferCache("System.Net.Connection", CachedBufferSize);
        
        //
        // Little status line holder.
        //
        private class StatusLineValues
        {
            internal int MajorVersion;
            internal int MinorVersion;
            internal int StatusCode;
            internal string StatusDescription;
        }

        private class WaitListItem
        {
            private HttpWebRequest request;
            private long queueStartTime;

            public HttpWebRequest Request
            {
                get { return request; }
            }

            public long QueueStartTime
            {
                get { return queueStartTime; }
            }

            public WaitListItem(HttpWebRequest request, long queueStartTime)
            {
                this.request = request;
                this.queueStartTime = queueStartTime;
            }
        }

        //
        // class members
        //
        private WebExceptionStatus  m_Error;
        internal Exception           m_InnerException;


        internal int                m_IISVersion = -1; //-1 means unread
        private byte[]              m_ReadBuffer;
        private bool                m_ReadBufferFromPinnableCache; // If we get our m_readBuffer from the Pinnable cache we have to explicitly free it
        private int                 m_BytesRead;
        private int                 m_BytesScanned;
        private int                 m_TotalResponseHeadersLength;
        private int                 m_MaximumResponseHeadersLength;
        private long                m_MaximumUnauthorizedUploadLength;
        private CoreResponseData    m_ResponseData;
        private ReadState           m_ReadState;
        private StatusLineValues    m_StatusLineValues;
        private int                 m_StatusState;
        private List<WaitListItem>  m_WaitList;
        private ArrayList           m_WriteList;
        private IAsyncResult        m_LastAsyncResult;
        private TimerThread.Timer   m_RecycleTimer;
        private WebParseError       m_ParseError;
        private bool                m_AtLeastOneResponseReceived;

        private static readonly WaitCallback m_PostReceiveDelegate = new WaitCallback(PostReceiveWrapper);
        private static readonly AsyncCallback m_ReadCallback = new AsyncCallback(ReadCallbackWrapper);
        private static readonly AsyncCallback m_TunnelCallback = new AsyncCallback(TunnelThroughProxyWrapper);
        private static byte[] s_NullBuffer = new byte[0];

        //
        // Abort handling variables. When trying to abort the
        // connection, we set Aborted = true, and close m_AbortSocket
        // if its non-null. m_AbortDelegate, is returned to every
        // request from our SubmitRequest method.  Calling m_AbortDelegate
        // drives us into Abort mode.
        //
        private HttpAbortDelegate m_AbortDelegate;
        private ConnectionGroup   m_ConnectionGroup;

        private UnlockConnectionDelegate m_ConnectionUnlock;

        //
        // ReadDone and m_Write - no two vars are so complicated,
        //  as these two. Used for m_WriteList managment, most be under crit
        //  section when accessing.
        //
        // ReadDone tracks the item at the end or
        //  just recenlty removed from the m_WriteList. While a
        //  pending BeginRead is in place, we need this to be false, in
        //  order to indicate to tell the WriteDone callback, that we can
        //  handle errors/resets.  The only exception is when the m_WriteList
        //  is empty, and there are no outstanding requests, then all it can
        //  be true.
        //
        // WriteDone tracks the item just added at the begining of the m_WriteList.
        //  this needs to be false while we about to write something, but have not
        //  yet begin or finished the write.  Upon completion, its set to true,
        //  so that DoneReading/ReadStartNextRequest can close the socket, without fear
        //  of a errand writer still banging away on another thread.
        //

        private DateTime        m_IdleSinceUtc;
        private HttpWebRequest  m_LockedRequest;
        private HttpWebRequest  m_CurrentRequest; // This is the request whose response is being parsed, same as WriteList[0] but could be different if request was aborted.
        private bool m_CanPipeline;
        private bool m_Free = true;
        private bool m_Idle = true;
        private bool m_KeepAlive = true;
        private bool m_Pipelining;
        private int m_ReservedCount;
        private bool m_ReadDone;
        private bool m_WriteDone;
        private bool m_RemovedFromConnectionList;
        private bool m_NonKeepAliveRequestPipelined;

        // Pipeline Throttling: m_IsPipelinePaused==true when we stopped and false when it's ok to add to the pipeline.
        private bool                m_IsPipelinePaused;
        private static int          s_MaxPipelinedCount = 10;
        private static int          s_MinPipelinedCount = 5;

#if TRAVE
        private bool q_Tunnelling;
#endif

        internal override ServicePoint ServicePoint {
            get {
                return ConnectionGroup.ServicePoint;
            }
        }

        private ConnectionGroup ConnectionGroup {
            get {
                return m_ConnectionGroup;
            }
        }

        //
        // LockedRequest is the request that needs exclusive access to this connection
        // the ConnectionGroup should proctect the Connection object from any new
        // Requests being queued, until this m_LockedRequest is finished.
        //
        internal HttpWebRequest LockedRequest {
            get {
                return m_LockedRequest;
            }
            set {
                HttpWebRequest myLock = m_LockedRequest;

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::LockedRequest_set() old#"+ ((myLock!=null)?myLock.GetHashCode().ToString():"null") +  " new#" + ((value!=null)?value.GetHashCode().ToString():"null"));

                if ((object)value == (object)myLock)
                {
                    if (value != null && (object)value.UnlockConnectionDelegate != (object) m_ConnectionUnlock)
                    {
                        throw new InternalException();
                    }
                    return;
                }

                object myDelegate = myLock == null? null: myLock.UnlockConnectionDelegate;
                if (myDelegate != null && (value != null || (object)m_ConnectionUnlock != (object)myDelegate))
                    throw new InternalException();

                if (value == null)
                {
                    m_LockedRequest = null;
                    myLock.UnlockConnectionDelegate = null;
                    return;
                }

                UnlockConnectionDelegate chkDelegate = value.UnlockConnectionDelegate;
                //
                // If "value" request was already locking a connection that is not "this", unlock that other connection
                //
                if ((object)chkDelegate != null)
                {
                    if ((object)chkDelegate == (object)m_ConnectionUnlock)
                        throw new InternalException();

                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::LockedRequest_set() Unlocking old request Connection");
                    chkDelegate();
                }

                value.UnlockConnectionDelegate = m_ConnectionUnlock;
                m_LockedRequest = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Delegate called when the request is finished using this Connection
        ///         exclusively.  Called in Abort cases and after NTLM authenticaiton completes.
        ///    </para>
        /// </devdoc>
        private void UnlockRequest() {
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::UnlockRequest() LockedRequest#" + ValidationHelper.HashString(LockedRequest));

            LockedRequest = null;

            if (ConnectionGroup != null) {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::UnlockRequest() - forcing call to ConnectionGoneIdle()");
                ConnectionGroup.ConnectionGoneIdle();
            }
        }


#if TRAVE
        /*
        private string MyLocalEndPoint {
            get {
                try {
                    return NetworkStream.InternalSocket.LocalEndPoint.ToString();
                }
                catch {
                    return "no connection";
                }
            }
        }
        */

        private string MyLocalPort {
            get {
                try {
                    if (NetworkStream == null || !NetworkStream.Connected) {
                        return "no connection";
                    }
                    return ((IPEndPoint)NetworkStream.InternalSocket.LocalEndPoint).Port.ToString();
                }
                catch {
                    return "no connection";
                }
            }
        }
#endif

        internal Connection(ConnectionGroup connectionGroup) : base(null) {
            //
            // add this Connection to the pool in the connection group,
            //  keep a weak reference to it
            //
            m_MaximumUnauthorizedUploadLength = SettingsSectionInternal.Section.MaximumUnauthorizedUploadLength;
            if(m_MaximumUnauthorizedUploadLength > 0){
                m_MaximumUnauthorizedUploadLength*=1024;
            }
            m_ResponseData = new CoreResponseData();
            m_ConnectionGroup = connectionGroup;
            m_ReadBuffer = s_PinnableBufferCache.AllocateBuffer();
            m_ReadBufferFromPinnableCache = true;
            m_ReadState = ReadState.Start;
            m_WaitList = new List<WaitListItem>();
            m_WriteList = new ArrayList();
            m_AbortDelegate = new HttpAbortDelegate(AbortOrDisassociate);
            m_ConnectionUnlock = new UnlockConnectionDelegate(UnlockRequest);

            // for status line parsing
            m_StatusLineValues = new StatusLineValues();
            m_RecycleTimer = ConnectionGroup.ServicePoint.ConnectionLeaseTimerQueue.CreateTimer();
            // the following line must be the last line of the constructor
            ConnectionGroup.Associate(this);
            m_ReadDone = true;
            m_WriteDone = true;
            m_Error = WebExceptionStatus.Success;
            if (PinnableBufferCacheEventSource.Log.IsEnabled())
            {
                PinnableBufferCacheEventSource.Log.DebugMessage1("CTOR: In System.Net.Connection.Connnection", this.GetHashCode());
            }
        }

        ~Connection() {
            if (m_ReadBufferFromPinnableCache)
            {
                if (PinnableBufferCacheEventSource.Log.IsEnabled())
                {
                    PinnableBufferCacheEventSource.Log.DebugMessage1("DTOR: ERROR Needing to Free m_ReadBuffer in Connection Destructor", m_ReadBuffer.GetHashCode());
                }
            }
            FreeReadBuffer();
        }

        // If the buffer came from the the pinnable cache, return it to the cache.
        // NOTE: This method is called from this object's finalizer and should not access any member objects.
        void FreeReadBuffer() {
            if (m_ReadBufferFromPinnableCache) {
                s_PinnableBufferCache.FreeBuffer(m_ReadBuffer);
                m_ReadBufferFromPinnableCache = false;
            }
            m_ReadBuffer = null;
        }

        protected override void Dispose(bool disposing) {
            if (PinnableBufferCacheEventSource.Log.IsEnabled()) {
                PinnableBufferCacheEventSource.Log.DebugMessage1("In System.Net.Connection.Dispose()", this.GetHashCode());
            }
            FreeReadBuffer();
            base.Dispose(disposing);
        }

        internal int BusyCount {
            get {
                return (m_ReadDone?0:1) + 2 * (m_WaitList.Count + m_WriteList.Count) + m_ReservedCount;
            }
        }

        internal int IISVersion{
            get{
                return m_IISVersion;
            }
        }

        internal bool AtLeastOneResponseReceived {
            get {
                return m_AtLeastOneResponseReceived;
            }
        }

        /*++

            SubmitRequest       - Submit a request for sending.

            The core submit handler. This is called when a request needs to be
            submitted to the network. This routine is asynchronous; the caller
            passes in an HttpSubmitDelegate that we invoke when the caller
            can use the underlying network. The delegate is invoked with the
            stream that it can right to.

            On the Sync path, we work by attempting to gain control of the Connection
            for writing and reading.  If some other thread is using the Connection,
            We wait inside of a LazyAsyncResult until it is availble.


            Input:
                    request                 - request that's being submitted.
                    SubmitDelegate          - Delegate to be invoked.
                    forcedsubmit            - Queue the request even if connection is going to close.

            Returns:
                    true when the request was correctly submitted

        --*/
        // userReqeustThread says whether we can post IO from this thread or not.
        [SuppressMessage("Microsoft.Reliability","CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification="Re-Baseline System violations from 3.5 SP1 due to added parameter")]
        internal bool SubmitRequest(HttpWebRequest request, bool forcedsubmit)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest", "request#" + ValidationHelper.HashString(request));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest");
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest() Free:" + m_Free.ToString() + " m_WaitList.Count:" + m_WaitList.Count.ToString());

            TriState startRequestResult = TriState.Unspecified;
            ConnectionReturnResult returnResult = null;
            bool expiredIdleConnection = false;

            // See if the connection is free, and if the underlying socket or
            // stream is set up. If it is, we can assign this connection to the
            // request right now. Otherwise we'll have to put this request on
            // on the wait list until it its turn.

            lock(this)
            {
                request.AbortDelegate = m_AbortDelegate;

                if (request.Aborted)
                {
                    // Note that request is not on the connection list yet and Abort() will push the response on the request
                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest - (Request was aborted before being submitted)", true);
                    UnlockIfNeeded(request);
                    return true;
                }
                //
                // There is a race condition between FindConnection and PrepareCloseConnectionSocket
                // Some request may already try to submit themselves while the connection is dying.
                //
                // Retry if that's the case
                //
                if (!CanBePooled)
                {
                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest", "false - can't be pooled");
                    UnlockIfNeeded(request);
                    return false;
                }

                //
                // There is a race condition between SubmitRequest and FindConnection. A non keep-alive request may
                // get submitted on this connection just after we check for it. So make sure that if we are queueing
                // behind non keep-alive request then its a forced submit.
                // Retry if that's not the case.
                //
                if (!forcedsubmit && NonKeepAliveRequestPipelined)
                {
                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest", "false - behind non keep-alive request");
                    UnlockIfNeeded(request);
                    return false;
                }

                // See if our timer still matches the SerivcePoint.  If not, get rid of it.
                if (m_RecycleTimer.Duration != ServicePoint.ConnectionLeaseTimerQueue.Duration) {
                    m_RecycleTimer.Cancel();
                    m_RecycleTimer = ServicePoint.ConnectionLeaseTimerQueue.CreateTimer();
                }

                if (m_RecycleTimer.HasExpired) {
                    request.KeepAlive = false;
                }

                //
                // If the connection has already been locked by another request, then
                // we fail the submission on this Connection.
                //

                if (LockedRequest != null && LockedRequest != request) {
                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest", "false");
                    return false;
                }


                //free means no one in the wait list.  We should only add a request
                //if the request can pipeline, or pipelining isn't available

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest WriteDone:" + m_WriteDone.ToString() + ", ReadDone:" + m_ReadDone.ToString() + ", m_WriteList.Count:" + m_WriteList.Count.ToString());

                //
                // If this request is marked as non keep-alive, we should stop pipelining more requests on this
                // connection. The keep-alive context is transfered to the connection from request only after we start
                // receiving response for the request.
                //
                if (!forcedsubmit && !m_NonKeepAliveRequestPipelined) {
                    m_NonKeepAliveRequestPipelined = (!request.KeepAlive && !request.NtlmKeepAlive);
                }

                if (m_Free && m_WriteDone && !forcedsubmit && (m_WriteList.Count == 0 || (request.Pipelined && !request.HasEntityBody && m_CanPipeline && m_Pipelining && !m_IsPipelinePaused))) {

                    // Connection is free. Mark it as busy and see if underlying
                    // socket is up.
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest - Free ");
                    m_Free = false;

                    // This codepath handles the case where the server has closed the Connection by
                    // returning false below: the request will be resubmitted on a different Connection.
                    startRequestResult = StartRequest(request, true);
                    if (startRequestResult == TriState.Unspecified)
                    {
                        expiredIdleConnection = true;
                        PrepareCloseConnectionSocket(ref returnResult);
                        // Hard Close the socket.
                        Close(0);
                        FreeReadBuffer();	// Do it after close completes to insure buffer not in use
                    }
                }
                else {
                    m_WaitList.Add(new WaitListItem(request, NetworkingPerfCounters.GetTimestamp()));
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.HttpWebRequestQueued);
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest - Request added to WaitList#"+ValidationHelper.HashString(request));
#if TRAVE
                    if (q_Tunnelling) {
                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest() MyLocalPort:" + MyLocalPort + " ERROR: adding HttpWebRequest#" + ValidationHelper.HashString(request) +" to tunnelling WaitList");
                    }
                    else {
                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest() MyLocalPort:" + MyLocalPort + " adding HttpWebRequest#" + ValidationHelper.HashString(request) +" to non-tunnelling WaitList m_WaitList.Count:" + m_WaitList.Count);
                    }
#endif
                    CheckNonIdle();
                }
            }

            if (expiredIdleConnection)
            {
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest(), expired idle connection", false);
                ConnectionReturnResult.SetResponses(returnResult);
                return false;
            }

            GlobalLog.DebugAddRequest(request, this, 0);
            if(Logging.On)Logging.Associate(Logging.Web, this, request);

            if (startRequestResult != TriState.Unspecified) {
                CompleteStartRequest(true, request, startRequestResult);
            }
            // On Sync, we wait for the Connection to be come availble here,
            if (!request.Async)
            {
                object responseObject = request.ConnectionAsyncResult.InternalWaitForCompletion();
                ConnectStream writeStream = responseObject as ConnectStream;
                AsyncTriState triStateAsync = null;
                if (writeStream == null)
                    triStateAsync = responseObject as AsyncTriState;

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest() Pipelining:"+m_Pipelining);

                if (startRequestResult == TriState.Unspecified && triStateAsync != null) {
                    // May need to recreate Connection here (i.e. call Socket.Connect)
                    CompleteStartRequest(true, request, triStateAsync.Value);
                }
                else if (writeStream != null)
                {
                    // return the Stream to the Request
                    request.SetRequestSubmitDone(writeStream);
                }
#if DEBUG
                else if (responseObject is Exception)
                {
                    Exception exception = responseObject as Exception;
                    WebException webException = responseObject as WebException;
                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest (SYNC) - Error waiting for a connection: " + exception.Message,
                                    "Status:" + (webException == null? exception.GetType().FullName: (webException.Status.ToString() +  " Internal Status: " + webException.InternalStatus.ToString())));
                    return true;
                }
#endif
            }

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SubmitRequest", true);
            return true;
        }

        private void UnlockIfNeeded(HttpWebRequest request) {
            if (LockedRequest == request) {
                UnlockRequest();
            }
        }

        // Wrapper for TriState for marhshalling across Thread boundaries
        private class AsyncTriState {
            public TriState Value;
            public AsyncTriState(TriState newValue) {
                Value = newValue;
            }
        }

        /*++

            StartRequest       - Start a request going.

            Routine to start a request. Called when we know the connection is
            free and we want to get a request going. This routine initializes
            some state, adds the request to the write queue, and checks to
            see whether or not the underlying connection is alive. If it's
            not, it queues a request to get it going. If the connection
            was alive we call the callback delegate of the request.

            This routine MUST BE called with the critcal section held.

            Input:
                    request                 - request that's being started.
                    canPollRead             - whether the calling code handles
                                              Unspecified due to the Connection
                                              being closed by the server.

            Returns:
                    True if request was started, false otherwise.

        --*/

        private TriState StartRequest(HttpWebRequest request, bool canPollRead)
        {
            GlobalLog.Enter(
                "Connection#" + ValidationHelper.HashString(this) +
                "::StartRequest",
                "HttpWebRequest#" + ValidationHelper.HashString(request) +
                " WriteDone:"+ m_WriteDone +
                " ReadDone:" + m_ReadDone +
                " WaitList:" + m_WaitList.Count +
                " WriteList:" + m_WriteList.Count);
            GlobalLog.ThreadContract(ThreadKinds.Unknown,
                "Connection#" + ValidationHelper.HashString(this) +
                "::StartRequest");

            if (m_WriteList.Count == 0)
            {
                // check if we consider connection timed out
                if (ServicePoint.MaxIdleTime != -1 &&
                    m_IdleSinceUtc != DateTime.MinValue &&
                    m_IdleSinceUtc +
                        TimeSpan.FromMilliseconds(ServicePoint.MaxIdleTime) <
                        DateTime.UtcNow)
                {
                    // This idle keep-alive connection timed out.
                    GlobalLog.Leave(
                        "Connection#" + ValidationHelper.HashString(this) +
                        "::StartRequest()" +
                        " Expired connection was idle for "
                        + (int)
                            ((DateTime.UtcNow - m_IdleSinceUtc).TotalSeconds) +
                        " secs, request will be retried: #" +
                        ValidationHelper.HashString(request));
                    return TriState.Unspecified; // don't use it
                } else if (canPollRead) {
                    // Not timed out from our perspective but...
                    // Check if remote has:
                    //   1) closed an idle connection (TCP FIN)
                    // or
                    //   2) sent some errant data on an idle connection.
                    bool pollRead = PollRead();
                    if (pollRead) {
                        GlobalLog.Leave(
                            "Connection#" + ValidationHelper.HashString(this) +
                            "::StartRequest() " +
                            "Idle connection remotely closed, " +
                            "request will be retried: #" +
                            ValidationHelper.HashString(request));
                        return TriState.Unspecified; // don't use it
                    }
                }
            }

            TriState needReConnect = TriState.False;
             // Starting a request means the connection is not idle anymore
            m_IdleSinceUtc = DateTime.MinValue;

             // Initialze state, and add the request to the write queue.

             //
             // Note that m_Pipelining shold be only set here but the sanity check is made by the caller
             // means if the caller has found that it is safe to pipeline the below result must be true as well
             //
            if (!m_IsPipelinePaused)
                m_IsPipelinePaused = m_WriteList.Count >= s_MaxPipelinedCount;

            m_Pipelining = m_CanPipeline && request.Pipelined && (!request.HasEntityBody);

            // start of write process, disable done-ness flag
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::StartRequest() setting WriteDone:" + m_WriteDone.ToString() + " to false");
            m_WriteDone = false;
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::StartRequest() m_WriteList adding HttpWebRequest#" + ValidationHelper.HashString(request));
            m_WriteList.Add(request);

            GlobalLog.Print(m_WriteList.Count+" requests queued");
            CheckNonIdle();

            // with no network stream around, we will have to create one, therefore, we can't have
            //  the possiblity to even have a DoneReading().

            if (IsInitalizing)
                needReConnect = TriState.True;

#if TRAVE
            if (request.IsTunnelRequest) {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::StartRequest() MyLocalPort:" + MyLocalPort + " setting Tunnelling to true HttpWebRequest#" + ValidationHelper.HashString(request));
                q_Tunnelling = true;
            }
            else {
                GlobalLog.Assert(!q_Tunnelling, "Connection#{0}::StartRequest()|MyLocalPort:{1} ERROR: Already tunnelling during non-tunnel request HttpWebRequest#{2}.", ValidationHelper.HashString(this), MyLocalPort, ValidationHelper.HashString(request));
            }
#endif

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::StartRequest", needReConnect.ToString());
            return needReConnect;
        }

        private void CompleteStartRequest(bool onSubmitThread, HttpWebRequest request, TriState needReConnect) {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest", ValidationHelper.HashString(request));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest");

            if (needReConnect == TriState.True) {
                // Socket is not alive.

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest() Queue StartConnection Delegate ");
                try {
                    if (request.Async) {
                        CompleteStartConnection(true, request);
                    }
                    else if (onSubmitThread) {
                        CompleteStartConnection(false, request);
                    }
                    // else - fall through and wake up other thread
                }
                catch (Exception exception) {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest(): exception: " + exception.ToString());
                    if (NclUtilities.IsFatal(exception)) throw;
                    //
                    // Should not be here because CompleteStartConnection and below tries to catch everything
                    //
                    GlobalLog.Assert(exception.ToString());
                }

                // If neeeded wake up other thread where SubmitRequest was called
                if (!request.Async) {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest() Invoking Async Result");
                    request.ConnectionAsyncResult.InvokeCallback(new AsyncTriState(needReConnect));
                }


                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest", "needReConnect");
                return;
            }


            //
            // From now on the request.SetRequestSubmitDone must be called or it may hang
            // For a sync request the write side reponse windowwas opened in HttpWebRequest.SubmitRequest
            if (request.Async)
                request.OpenWriteSideResponseWindow();


            ConnectStream writeStream = new ConnectStream(this, request);

            // Call the request to let them know that we have a write-stream, this might invoke Send() call
            if (request.Async || onSubmitThread) {
                request.SetRequestSubmitDone(writeStream);
            }
            else {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest() Invoking Async Result");
                request.ConnectionAsyncResult.InvokeCallback(writeStream);
            }
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartRequest");
        }

        /*++

            CheckNextRequest

            Gets the next request from the wait queue, if there is one.

            Must be called with the crit sec held.


        --*/
        private HttpWebRequest CheckNextRequest()
        {
        GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::CheckNextRequest");

            if (m_WaitList.Count == 0) {
                // We're free now, if we're not going to close the connection soon.
                m_Free = m_KeepAlive;
                return null;
            }
            if (!CanBePooled) {
                return null;
            }

            WaitListItem item = m_WaitList[0];
            HttpWebRequest nextRequest = item.Request;

            if (m_IsPipelinePaused)
                m_IsPipelinePaused = m_WriteList.Count > s_MinPipelinedCount;

            if (!nextRequest.Pipelined || nextRequest.HasEntityBody || !m_CanPipeline || !m_Pipelining || m_IsPipelinePaused) {
                if (m_WriteList.Count != 0) {
                    nextRequest = null;
                }
            }
            if (nextRequest != null) {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckNextRequest() Removing request#" + ValidationHelper.HashString(nextRequest) + " from m_WaitList. New Count:" + (m_WaitList.Count - 1).ToString());

                NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgQueueTime,
                    item.QueueStartTime);
                m_WaitList.RemoveAt(0);
                CheckIdle();
            }
            return nextRequest;
        }

        private void CompleteStartConnection(bool async, HttpWebRequest httpWebRequest)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartConnection",  "async:" + async.ToString() + " httpWebRequest:" + ValidationHelper.HashString(httpWebRequest));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::CompleteStartConnection");

            WebExceptionStatus ws = WebExceptionStatus.ConnectFailure;
            m_InnerException = null;
            bool success = true;

            try {
#if DEBUG
                lock (this)
                {
                    // m_WriteList can be empty if request got aborted.  In that case no new requests can come in so it should remain zero.
                    if (m_WriteList.Count != 0)
                    {
                        GlobalLog.Assert(m_WriteList.Count == 1, "Connection#{0}::CompleteStartConnection()|WriteList is not sized 1.", ValidationHelper.HashString(this));
                        GlobalLog.Assert((m_WriteList[0] as HttpWebRequest) == httpWebRequest, "Connection#{0}::CompleteStartConnection()|Last request on write list does not match.", ValidationHelper.HashString(this));
                    }
                }
#endif

                //
                // we will create a tunnel through a proxy then create
                // and connect the socket we will use for the connection
                // otherwise we will just create a socket and use it
                //
                if ((httpWebRequest.IsWebSocketRequest || httpWebRequest.Address.Scheme == Uri.UriSchemeHttps) && 
                    ServicePoint.InternalProxyServicePoint) 
                {
                    if(!TunnelThroughProxy(ServicePoint.InternalAddress, httpWebRequest,async)) {
                        ws = WebExceptionStatus.ConnectFailure;
                        success = false;
                    }
                    if (async && success) {
                        return;
                    }
                } else {
                    if (!Activate(httpWebRequest, async, new GeneralAsyncDelegate(CompleteConnectionWrapper)))
                    {
                        return;
                    }
                }
            }
            catch (Exception exception) {
                if (m_InnerException == null)
                    m_InnerException = exception;

                if (exception is WebException) {
                    ws = ((WebException)exception).Status;
                }
                success = false;
            }
            if(!success)
            {
                ConnectionReturnResult returnResult = null;
                HandleError(false, false, ws, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartConnection Failed to connect.");
                return;
            }

            // Getting here means we connected synchronously.  Continue with the next step.

            CompleteConnection(async, httpWebRequest);
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::CompleteStartConnection");
        }

        private void CompleteConnectionWrapper(object request, object state)
        {
#if DEBUG
            using (GlobalLog.SetThreadKind(ThreadKinds.System | ThreadKinds.Async)) {
#endif
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(state) + "::CompleteConnectionWrapper", "request:" + ValidationHelper.HashString(request));

            Exception stateException = state as Exception;
            if (stateException != null)
            {
                GlobalLog.Print("CompleteConnectionWrapper() Request#" + ValidationHelper.HashString(request) + " Connection is in error: " + stateException.ToString());
                ConnectionReturnResult returnResult = null;

                if (m_InnerException == null)
                    m_InnerException = stateException;

                HandleError(false, false, WebExceptionStatus.ConnectFailure, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
            }
            CompleteConnection(true, (HttpWebRequest) request);

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(state) + "::CompleteConnectionWrapper" + (stateException == null? string.Empty: " failed"));
#if DEBUG
            }
#endif
        }

        private void CompleteConnection(bool async, HttpWebRequest request)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::CompleteConnection", "async:" + async.ToString() + " request:" + ValidationHelper.HashString(request));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::CompleteConnection");

            WebExceptionStatus ws = WebExceptionStatus.ConnectFailure;
            //
            // From now on the request.SetRequestSubmitDone must be called or it may hang
            // For a sync request the write side reponse windowwas opened in HttpWebRequest.SubmitRequest
            if (request.Async)
                request.OpenWriteSideResponseWindow();

            try
            {
                try {
#if !FEATURE_PAL
                    if (request.Address.Scheme == Uri.UriSchemeHttps) {
                        TlsStream tlsStream = new TlsStream(request.GetRemoteResourceUri().IdnHost,
                            NetworkStream, request.ClientCertificates, ServicePoint, request,
                            request.Async ? request.GetConnectingContext().ContextCopy : null);
                        NetworkStream = tlsStream;
                    }
#endif
                    ws = WebExceptionStatus.Success;
                }
                catch {
                    // The TLS stream could not be created.  Close the current non-TLS stream immediately
                    // to prevent any future use of it.  Due to race conditions, the error handling will sometimes
                    // try to write (flush) out some of the HTTP headers to the stream as it is closing down the failed 
                    // HttpWebRequest. This would cause plain text to go on the wire even though the stream should
                    // have been TLS encrypted.
                    NetworkStream.Close();
                    throw;
                }
                finally {
                    //
                    // There is a ---- with Abort so TlsStream ctor may throw.
                    // SetRequestSubmitDone will deal with this kind of errors.
                    // 

                    m_ReadState = ReadState.Start;
                    ClearReaderState();

                    request.SetRequestSubmitDone(new ConnectStream(this, request));
                }
            }
            catch (Exception exception)
            {
                if (m_InnerException == null)
                    m_InnerException = exception;
                WebException webException = exception as WebException;
                if (webException != null)
                {
                    ws = webException.Status;
                }
            }

            if (ws != WebExceptionStatus.Success)
            {
                ConnectionReturnResult returnResult = null;
                HandleError(false, false, ws, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);

                if (Logging.On) Logging.PrintError(Logging.Web, this, "CompleteConnection", "on error");
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::CompleteConnection", "on error");
            }
            else
            {
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::CompleteConnection");
            }
        }

        private void InternalWriteStartNextRequest(HttpWebRequest request, ref bool calledCloseConnection, ref TriState startRequestResult, ref HttpWebRequest nextRequest, ref ConnectionReturnResult returnResult) {
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::InternalWriteStartNextRequest");

            lock(this) {

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest() setting WriteDone:" + m_WriteDone.ToString() + " to true");
                m_WriteDone = true;

                //
                // If we're not doing keep alive, and the read on this connection
                // has already completed, now is the time to close the
                // connection.
                //
                //need to wait for read to set the error
                if (!m_KeepAlive || m_Error != WebExceptionStatus.Success || !CanBePooled) {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest() m_WriteList.Count:" + m_WriteList.Count);
                    if (m_ReadDone) {
                        // We could be closing because of an unexpected keep-alive
                        // failure, ie we pipelined a few requests and in the middle
                        // the remote server stopped doing keep alive. In this
                        // case m_Error could be success, which would be misleading.
                        // So in that case we'll set it to connection closed.

                        if (m_Error == WebExceptionStatus.Success) {
                            // Only reason we could have gotten here is because
                            // we're not keeping the connection alive.
                            m_Error = WebExceptionStatus.KeepAliveFailure;
                        }

                        // PrepareCloseConnectionSocket is called with the critical section
                        // held. Note that we know since it's not a keep-alive
                        // connection the read half wouldn't have posted a receive
                        // for this connection, so it's OK to call PrepareCloseConnectionSocket now.
                        PrepareCloseConnectionSocket(ref returnResult);
                        calledCloseConnection = true;
                        Close();
                    }
                    else {
                        if (m_Error!=WebExceptionStatus.Success) {
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest() a Failure, m_Error = " + m_Error.ToString());
                        }
                    }
                }
                else {
                    // If we're pipelining, we get get the next request going
                    // as soon as the write is done. Otherwise we have to wait
                    // until both read and write are done.


                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest() Non-Error m_WriteList.Count:" + m_WriteList.Count + " m_WaitList.Count:" + m_WaitList.Count);

                    if (m_Pipelining || m_ReadDone)
                    {
                        nextRequest = CheckNextRequest();
                    }
                    if (nextRequest != null)
                    {
                        // This codepath doesn't handle the case where the server has closed the Connection because we
                        // just finished using it and didn't get a Connection: close header.
                        startRequestResult = StartRequest(nextRequest, false);
                        GlobalLog.Assert(startRequestResult != TriState.Unspecified, "WriteStartNextRequest got TriState.Unspecified from StartRequest, things are about to hang!");
                    }
                }
            } // lock
        }

        internal void WriteStartNextRequest(HttpWebRequest request, ref ConnectionReturnResult returnResult) {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest" + " WriteDone:" + m_WriteDone + " ReadDone:" + m_ReadDone + " WaitList:" + m_WaitList.Count + " WriteList:" + m_WriteList.Count);
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest");

            TriState startRequestResult = TriState.Unspecified;
            HttpWebRequest nextRequest = null;
            bool calledCloseConnection = false;

            InternalWriteStartNextRequest(request, ref calledCloseConnection, ref startRequestResult, ref nextRequest, ref returnResult);

            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest: Pipelining:" + m_Pipelining + " nextRequest#"+ValidationHelper.HashString(nextRequest));

            if (!calledCloseConnection && startRequestResult != TriState.Unspecified)
            {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest calling CompleteStartRequest");
                CompleteStartRequest(false, nextRequest, startRequestResult);
            }

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::WriteStartNextRequest");
        }


        internal void SetLeftoverBytes(byte[] buffer, int bufferOffset, int bufferCount)
        {
            // The ConnectStream read past the response of its HTTP response (can happen in chunked scenarios). 
            // Get the buffer containing bytes belonging to the next request and use them for the next request.

            if (bufferOffset > 0)
            {
                // We need to move leftover bytes to the beginning of the buffer.
                Buffer.BlockCopy(buffer, bufferOffset, buffer, 0, bufferCount);
            }

            // If we had to reallocate the buffer, we are going to clobber the one that was allocated from the pin friendly cache.  
            // give it back
            if (m_ReadBuffer != buffer)
            {
                // if m_ReadBuffer is from the pinnable cache, give it back
                FreeReadBuffer();
                m_ReadBuffer = buffer;
            }

            m_BytesScanned = 0;
            m_BytesRead = bufferCount;
        }

        /*++

            ReadStartNextRequest

            This method is called by a stream interface when it's done reading.
            We might possible free up the connection for another request here.

            Called when we think we might need to start another request because
            a read completed.

        --*/
        internal void ReadStartNextRequest(WebRequest currentRequest, ref ConnectionReturnResult returnResult)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest" + " WriteDone:" + m_WriteDone + " ReadDone:" + m_ReadDone + " WaitList:" + m_WaitList.Count + " WriteList:" + m_WriteList.Count);
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest");

            HttpWebRequest nextRequest = null;
            TriState startRequestResult = TriState.Unspecified;
            bool calledCloseConnection = false;
            bool mustExit = false;

            // ReadStartNextRequest is called by ConnectStream.CallDone: This guarantees that the request
            // is done and the response (response stream) was closed. Remove the reservation for the request.
            int currentCount = Interlocked.Decrement(ref m_ReservedCount);
            GlobalLog.Assert(currentCount >= 0, "m_ReservedCount must not be < 0 when decremented.");

            try {
                lock(this) {
                    if (m_WriteList.Count > 0 && (object)currentRequest == m_WriteList[0])
                    {
                        // advance back to state 0
                        m_ReadState = ReadState.Start;
                        m_WriteList.RemoveAt(0);

                        // Must reset ConnectStream here to prevent a leak through the stream of the last request on each connection.
                        m_ResponseData.m_ConnectStream = null;

                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest() Removed request#" + ValidationHelper.HashString(currentRequest) + " from m_WriteList. New m_WriteList.Count:" + m_WriteList.Count.ToString());
                    }
                    else
                    {
                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest() The request#" + ValidationHelper.HashString(currentRequest) + " was disassociated so do nothing.  m_WriteList.Count:" + m_WriteList.Count.ToString());
                        mustExit = true;;
                    }

                    //
                    // Since this is called after we're done reading the current
                    // request, if we're not doing keepalive and we're done
                    // writing we can close the connection now.
                    //
                    if(!mustExit)
                    {
                        //
                        // m_ReadDone==true is implied because we just finished a request but really the value must still be false here
                        //
                        if (m_ReadDone)
                            throw new InternalException();  // other requests may already started reading on this connection, need a QFE

                        if (!m_KeepAlive || m_Error != WebExceptionStatus.Success || !CanBePooled)
                        {
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest() KeepAlive:" + m_KeepAlive + " WriteDone:" + m_WriteDone);
                            // Finished one request and connection is closing.
                            // We will not read from this connection so set readDone = true
                            m_ReadDone = true;

                            if (m_WriteDone)
                            {

                                // We could be closing because of an unexpected keep-alive
                                // failure, ie we pipelined a few requests and in the middle
                                // the remote server stopped doing keep alive. In this
                                // case m_Error could be success, which would be misleading.
                                // So in that case we'll set it to KeepAliveFailure.

                                if (m_Error == WebExceptionStatus.Success) {
                                    // Only reason we could have gotten here is because
                                    // we're not keeping the connection alive.
                                    m_Error = WebExceptionStatus.KeepAliveFailure;
                                }

                                // PrepareCloseConnectionSocket has to be called with the critical section held.
                                PrepareCloseConnectionSocket(ref returnResult);
                                calledCloseConnection = true;
                                Close();
                            }
                        }
                        else
                        {
                            // We try to sort out KeepAliveFailure thing (search by context)
                            m_AtLeastOneResponseReceived = true;

                            if (m_WriteList.Count != 0)
                            {
                                // If a *pipelined* request that is being submitted has finished with the headers, post a receive
                                nextRequest = m_WriteList[0] as HttpWebRequest;
                                // If the active request has not finished its headers we can set m_ReadDone = true
                                // and that will be changed when said request will call CheckStartReceive
                                if (!nextRequest.HeadersCompleted)
                                {
                                    nextRequest = null;
                                    m_ReadDone = true;
                                }
                            }
                            // If there are no requests left to write (means pipeline),
                            // we can get the next request from wait list going now.
                            else
                            {
                                m_ReadDone = true;

                                // Sometime we get a response before completing the body in which case
                                // we defer next request to WriteStartNextRequest
                                if (m_WriteDone)
                                {
                                    nextRequest = CheckNextRequest();

                                    if (nextRequest != null )
                                    {
                                        // We cannot have HeadersCompleted on the request that was not placed yet on the write list
                                        if(nextRequest.HeadersCompleted) // 
                                            throw new InternalException();

                                        // This codepath doesn't handle the case where the server has closed the
                                        // Connection because we just finished using it and didn't get a
                                        // Connection: close header.
                                        startRequestResult = StartRequest(nextRequest, false);
                                        GlobalLog.Assert(startRequestResult != TriState.Unspecified, "ReadStartNextRequest got TriState.Unspecified from StartRequest, things are about to hang!");
                                    }
                                    else
                                    {
                                        //There are no other requests to process, so make connection avaliable for all
                                        m_Free = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                CheckIdle();
                //set result here to prevent nesting of readstartnextrequest.
                if(returnResult != null){
                    ConnectionReturnResult.SetResponses(returnResult);
                }
            }

            if(!mustExit && !calledCloseConnection)
            {
                if (startRequestResult != TriState.Unspecified)
                {
                    CompleteStartRequest(false, nextRequest, startRequestResult);
                }
                else if (nextRequest != null)
                {
                    // Handling receive, note that is for pipelinning case only !
                    if (!nextRequest.Async)
                    {
                        nextRequest.ConnectionReaderAsyncResult.InvokeCallback();
                    }
                    else
                    {
                        if (m_BytesScanned < m_BytesRead)
                        {
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest() Calling ReadComplete, bytes unparsed = " + (m_BytesRead - m_BytesScanned));
                            ReadComplete(0, WebExceptionStatus.Success);
                        }
                        else if (Thread.CurrentThread.IsThreadPoolThread)
                        {
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest() Calling PostReceive().");
                            PostReceive();
                        }
                        else
                        {
                            // Offload to the threadpool to protect against the case where one request's thread posts IO that another request
                            // depends on, but the first thread dies in the mean time.
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest() ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this)");
                            ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this);
                        }
                    }
                }
            }
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ReadStartNextRequest");
        }

        internal void MarkAsReserved()
        {
            // We use an interlock here rather than a lock() to avoid deadlocks in the following situation:
            // - ConnectionGroup is holding lock(obj1), calls into Connection which is waiting for lock(obj2)
            // - on another thread Connection is holding lock(obj2) and calls into ConnectionGroup which will wait
            //   for lock(obj1).
            int currentCount = Interlocked.Increment(ref m_ReservedCount);
            GlobalLog.Assert(currentCount > 0, "m_ReservedCount must not be less or equal zero after incrementing.");
        }

        //
        //
        //
        internal void CheckStartReceive(HttpWebRequest request)
        {
            lock (this)
            {
                request.HeadersCompleted = true;
                if (m_WriteList.Count == 0)
                {
                    // aborted request, was already dispatched.
                    // Note it could have been aborted softly if not the first one in the pipeline
                    return;
                }

                // Note we do NOT allow receive if pipelining and the passed request is not the first one on the write queue
                if (!m_ReadDone || m_WriteList[0] != (object)request)
                {
                    // ReadStartNextRequest should take care of these cases
                    return;
                }
                // Start a receive
                m_ReadDone = false;
                m_CurrentRequest = (HttpWebRequest)m_WriteList[0];
            }

            if (!request.Async)
            {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckStartReceive() SYNC request, calling ConnectionReaderAsyncResult.InvokeCallback()");
                request.ConnectionReaderAsyncResult.InvokeCallback();
            }
            else if (m_BytesScanned < m_BytesRead)
            {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckStartReceive() Calling ReadComplete, bytes unparsed = " + (m_BytesRead - m_BytesScanned));
                ReadComplete(0, WebExceptionStatus.Success);
            }
            else if (Thread.CurrentThread.IsThreadPoolThread)
            {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckStartReceive() Calling PostReceive().");
                PostReceive();
            }
            else
            {
                // Offload to the threadpool to protect against the case where one request's thread posts IO that another request
                // depends on, but the first thread dies in the mean time.
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckStartReceive() ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this)");
                ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this);
            }
        }

        /*++

        Routine Description:

           Clears out common member vars used for Status Line parsing

        Arguments:

           None.

        Return Value:

           None.

        --*/

        private void InitializeParseStatusLine() {
            m_StatusState = BeforeVersionNumbers;
            m_StatusLineValues.MajorVersion = 0;
            m_StatusLineValues.MinorVersion = 0;
            m_StatusLineValues.StatusCode = 0;
            m_StatusLineValues.StatusDescription = null;
        }

        /*++

        Routine Description:

           Performs status line parsing on incomming server responses

        Arguments:

           statusLine - status line that we wish to parse
           statusLineLength - length of the array
           statusLineInts - array of ints contanes result
           statusDescription - string with discription
           statusStatus     - state stored between parse attempts

        Return Value:

           bool - Success true/false

        --*/

        private const int BeforeVersionNumbers = 0;
        private const int MajorVersionNumber   = 1;
        private const int MinorVersionNumber   = 2;
        private const int StatusCodeNumber     = 3;
        private const int AfterStatusCode      = 4;
        private const int AfterCarriageReturn  = 5;

        private const string BeforeVersionNumberBytes = "HTTP/";

        private DataParseStatus ParseStatusLine(
                byte [] statusLine,
                int statusLineLength,
                ref int bytesParsed,
                ref int [] statusLineInts,
                ref string statusDescription,
                ref int statusState,
                ref WebParseError parseError) {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::ParseStatusLine", statusLineLength.ToString(NumberFormatInfo.InvariantInfo) + ", " + bytesParsed.ToString(NumberFormatInfo.InvariantInfo) +", " +statusState.ToString(NumberFormatInfo.InvariantInfo));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::ParseStatusLine");
            GlobalLog.Assert((statusLineLength - bytesParsed) >= 0, "Connection#{0}::ParseStatusLine()|(statusLineLength - bytesParsed) < 0", ValidationHelper.HashString(this));
            //GlobalLog.Dump(statusLine, bytesParsed, statusLineLength);

            DataParseStatus parseStatus = DataParseStatus.Done;
            int statusLineSize = 0;
            int startIndexStatusDescription = -1;
            int lastUnSpaceIndex = 0;

            //
            // While walking the Status Line looking for terminating \r\n,
            //   we extract the Major.Minor Versions and Status Code in that order.
            //   text and spaces will lie between/before/after the three numbers
            //   but the idea is to remember which number we're calculating based on a numeric state
            //   If all goes well the loop will churn out an array with the 3 numbers plugged in as DWORDs
            //

            while ((bytesParsed < statusLineLength) && (statusLine[bytesParsed] != '\r') && (statusLine[bytesParsed] != '\n')) {

                // below should be wrapped in while (response[i] != ' ') to be more robust???
                switch (statusState) {
                    case BeforeVersionNumbers:
                        if (statusLine[bytesParsed] == '/') {
                            //INET_ASSERT(statusState == BeforeVersionNumbers);
                            statusState++; // = MajorVersionNumber
                        }
                        else if (statusLine[bytesParsed] == ' ') {
                            statusState = StatusCodeNumber;
                        }

                        break;

                    case MajorVersionNumber:

                        if (statusLine[bytesParsed] == '.') {
                            //INET_ASSERT(statusState == MajorVersionNumber);
                            statusState++; // = MinorVersionNumber
                            break;
                        }
                        // fall through
                        goto case MinorVersionNumber;

                    case MinorVersionNumber:

                        if (statusLine[bytesParsed] == ' ') {
                            //INET_ASSERT(statusState == MinorVersionNumber);
                            statusState++; // = StatusCodeNumber
                            break;
                        }
                        // fall through
                        goto case StatusCodeNumber;

                    case StatusCodeNumber:

                        if (Char.IsDigit((char)statusLine[bytesParsed])) {
                            int val = statusLine[bytesParsed] - '0';
                            statusLineInts[statusState] = statusLineInts[statusState] * 10 + val;
                        }
                        else if (statusLineInts[StatusCodeNumber] > 0) {
                            //
                            // we eat spaces before status code is found,
                            //  once we have the status code we can go on to the next
                            //  state on the next non-digit. This is done
                            //  to cover cases with several spaces between version
                            //  and the status code number.
                            //

                            statusState++; // = AfterStatusCode
                            break;
                        }
                        else if (!Char.IsWhiteSpace((char) statusLine[bytesParsed])) {
                            statusLineInts[statusState] = (int)-1;
                        }

                        break;

                    case AfterStatusCode:
                        if (statusLine[bytesParsed] != ' ') {
                            lastUnSpaceIndex = bytesParsed;
                            if (startIndexStatusDescription == -1) {
                                startIndexStatusDescription = bytesParsed;
                            }
                        }
                        break;

                }
                ++bytesParsed;
                if (m_MaximumResponseHeadersLength>=0 && ++m_TotalResponseHeadersLength>=m_MaximumResponseHeadersLength) {
                    parseStatus = DataParseStatus.DataTooBig;
                    goto quit;
                }
            }

            statusLineSize = bytesParsed;

            // add to Description if already partialy parsed
            if (startIndexStatusDescription != -1) {
                statusDescription +=
                    WebHeaderCollection.HeaderEncoding.GetString(
                        statusLine,
                        startIndexStatusDescription,
                        lastUnSpaceIndex - startIndexStatusDescription + 1 );
            }

            if (bytesParsed == statusLineLength) {
                //
                // response now points one past the end of the buffer. We may be looking
                // over the edge...
                //
                // if we're at the end of the connection then the server sent us an
                // incorrectly formatted response. Probably an error.
                //
                // Otherwise its a partial response. We need more
                //
                parseStatus = DataParseStatus.NeedMoreData;
                //
                // if we really hit the end of the response then update the amount of
                // headers scanned
                //
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ParseStatusLine", parseStatus.ToString());
                return parseStatus;
            }

            while ((bytesParsed < statusLineLength)
                   && ((statusLine[bytesParsed] == '\r') || (statusLine[bytesParsed] == ' '))) {
                ++bytesParsed;
                if (m_MaximumResponseHeadersLength>=0 && ++m_TotalResponseHeadersLength>=m_MaximumResponseHeadersLength) {
                    parseStatus = DataParseStatus.DataTooBig;
                    goto quit;
                }
            }

            if (bytesParsed == statusLineLength) {

                //
                // hit end of buffer without finding LF
                //

                parseStatus = DataParseStatus.NeedMoreData;
                goto quit;

            }
            else if (statusLine[bytesParsed] == '\n') {
                ++bytesParsed;
                if (m_MaximumResponseHeadersLength>=0 && ++m_TotalResponseHeadersLength>=m_MaximumResponseHeadersLength) {
                    parseStatus = DataParseStatus.DataTooBig;
                    goto quit;
                }
                //
                // if we found the empty line then we are done
                //
                parseStatus = DataParseStatus.Done;
            }


            //
            // Now we have our parsed header to add to the array
            //

quit:
            if (parseStatus == DataParseStatus.Done && statusState != AfterStatusCode) {
                // need to handle the case where we parse the StatusCode,
                //  but didn't get a status Line, and there was no space afer it.
                if (statusState != StatusCodeNumber || statusLineInts[StatusCodeNumber] <= 0) {
                    //
                    // we're done with the status line, if we didn't parse all the
                    // numbers needed this is invalid protocol on the server
                    //
                    parseStatus = DataParseStatus.Invalid;
                }
            }

            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseStatusLine() StatusCode:" + statusLineInts[StatusCodeNumber] + " MajorVersionNumber:" + statusLineInts[MajorVersionNumber] + " MinorVersionNumber:" + statusLineInts[MinorVersionNumber] + " StatusDescription:" + ValidationHelper.ToString(statusDescription));
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ParseStatusLine", parseStatus.ToString());

            if (parseStatus == DataParseStatus.Invalid) {
                parseError.Section = WebParseErrorSection.ResponseStatusLine;
                parseError.Code = WebParseErrorCode.Generic;
            }

            return parseStatus;
        }

        // Must all start with a different first character.
        private static readonly string[] s_ShortcutStatusDescriptions = new string[] { "OK", "Continue", "Unauthorized" };

        //
        // Updated version of ParseStatusLine() - secure and fast
        //
        private static unsafe DataParseStatus ParseStatusLineStrict(
                byte[] statusLine,
                int statusLineLength,
                ref int bytesParsed,
                ref int statusState,
                StatusLineValues statusLineValues,
                int maximumHeaderLength,
                ref int totalBytesParsed,
                ref WebParseError parseError)
        {
            GlobalLog.Enter("Connection::ParseStatusLineStrict", statusLineLength.ToString(NumberFormatInfo.InvariantInfo) + ", " + bytesParsed.ToString(NumberFormatInfo.InvariantInfo) + ", " + statusState.ToString(NumberFormatInfo.InvariantInfo));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection::ParseStatusLineStrict");
            GlobalLog.Assert((statusLineLength - bytesParsed) >= 0, "Connection::ParseStatusLineStrict()|(statusLineLength - bytesParsed) < 0");
            GlobalLog.Assert(maximumHeaderLength <= 0 || totalBytesParsed <= maximumHeaderLength, "Connection::ParseStatusLineStrict()|Headers already read exceeds limit.");

            // Remember where we started.
            int initialBytesParsed = bytesParsed;

            // Set up parsing status with what will happen if we exceed the buffer.
            DataParseStatus parseStatus = DataParseStatus.DataTooBig;
            int effectiveMax = maximumHeaderLength <= 0 ? int.MaxValue : (maximumHeaderLength - totalBytesParsed + bytesParsed);
            if (statusLineLength < effectiveMax)
            {
                parseStatus = DataParseStatus.NeedMoreData;
                effectiveMax = statusLineLength;
            }

            // sanity check
            if (bytesParsed >= effectiveMax)
                goto quit;

            fixed (byte* byteBuffer = statusLine)
            {
                // Use this switch to jump midway into the action.  They all fall through until the end of the buffer is reached or
                // the status line is fully parsed.
                switch (statusState)
                {
                    case BeforeVersionNumbers:
                        // This takes advantage of the fact that this token must be the very first thing in the response.
                        while (totalBytesParsed - initialBytesParsed + bytesParsed < BeforeVersionNumberBytes.Length)
                        {
                            if ((byte)BeforeVersionNumberBytes[totalBytesParsed - initialBytesParsed + bytesParsed] != byteBuffer[bytesParsed])
                            {
                                parseStatus = DataParseStatus.Invalid;
                                goto quit;
                            }

                            if(++bytesParsed == effectiveMax)
                                goto quit;
                        }

                        // When entering the MajorVersionNumber phase, make sure at least one digit is present.
                        if (byteBuffer[bytesParsed] == '.')
                        {
                            parseStatus = DataParseStatus.Invalid;
                            goto quit;
                        }

                        statusState = MajorVersionNumber;
                        goto case MajorVersionNumber;

                    case MajorVersionNumber:
                        while (byteBuffer[bytesParsed] != '.')
                        {
                            if (byteBuffer[bytesParsed] < '0' || byteBuffer[bytesParsed] > '9')
                            {
                                parseStatus = DataParseStatus.Invalid;
                                goto quit;
                            }

                            statusLineValues.MajorVersion = statusLineValues.MajorVersion * 10 + byteBuffer[bytesParsed] - '0';

                            if (++bytesParsed == effectiveMax)
                                goto quit;
                        }

                        // Need visibility past the dot.
                        if (bytesParsed + 1 == effectiveMax)
                            goto quit;
                        bytesParsed++;

                        // When entering the MinorVersionNumber phase, make sure at least one digit is present.
                        if (byteBuffer[bytesParsed] == ' ')
                        {
                            parseStatus = DataParseStatus.Invalid;
                            goto quit;
                        }

                        statusState = MinorVersionNumber;
                        goto case MinorVersionNumber;

                    case MinorVersionNumber:
                        // Only a single SP character is allowed to delimit fields in the status line.
                        while (byteBuffer[bytesParsed] != ' ')
                        {
                            if (byteBuffer[bytesParsed] < '0' || byteBuffer[bytesParsed] > '9')
                            {
                                parseStatus = DataParseStatus.Invalid;
                                goto quit;
                            }

                            statusLineValues.MinorVersion = statusLineValues.MinorVersion * 10 + byteBuffer[bytesParsed] - '0';

                            if (++bytesParsed == effectiveMax)
                                goto quit;
                        }

                        statusState = StatusCodeNumber;

                        // Start the status code out as "1".  This will effectively add 1000 to the code.  It's used to count
                        // the number of digits to make sure it's three.  At the end, subtract 1000.
                        statusLineValues.StatusCode = 1;

                        // Move past the space.
                        if (++bytesParsed == effectiveMax)
                            goto quit;

                        goto case StatusCodeNumber;

                    case StatusCodeNumber:
                        // RFC2616 says codes with an unrecognized first digit
                        // should be rejected.  We're allowing the application to define their own "understanding" of
                        // 0, 6, 7, 8, and 9xx codes.
                        while (byteBuffer[bytesParsed] >= '0' && byteBuffer[bytesParsed] <= '9')
                        {
                            // Make sure it isn't too big.  The leading '1' will be removed after three digits are read.
                            if (statusLineValues.StatusCode >= 1000)
                            {
                                parseStatus = DataParseStatus.Invalid;
                                goto quit;
                            }

                            statusLineValues.StatusCode = statusLineValues.StatusCode * 10 + byteBuffer[bytesParsed] - '0';

                            if (++bytesParsed == effectiveMax)
                                goto quit;
                        }

                        // Make sure there was enough, and exactly one space.
                        if (byteBuffer[bytesParsed] != ' ' || statusLineValues.StatusCode < 1000)
                        {
                            if(byteBuffer[bytesParsed] == '\r' && statusLineValues.StatusCode >= 1000){
                                statusLineValues.StatusCode -= 1000;
                                statusState = AfterCarriageReturn;
                                if (++bytesParsed == effectiveMax)
                                    goto quit;
                                goto case AfterCarriageReturn;
                            }
                            parseStatus = DataParseStatus.Invalid;
                            goto quit;
                        }

                        // Remove the extra leading 1.
                        statusLineValues.StatusCode -= 1000;

                        statusState = AfterStatusCode;

                        // Move past the space.
                        if (++bytesParsed == effectiveMax)
                            goto quit;

                        goto case AfterStatusCode;

                    case AfterStatusCode:
                    {
                        // Check for shortcuts.
                        if (statusLineValues.StatusDescription == null)
                        {
                            foreach (string s in s_ShortcutStatusDescriptions)
                            {
                                if (bytesParsed < effectiveMax - s.Length && byteBuffer[bytesParsed] == (byte) s[0])
                                {
                                    int i;
                                    byte *pBuffer = byteBuffer + bytesParsed + 1;
                                    for(i = 1; i < s.Length; i++)
                                        if (*(pBuffer++) != (byte) s[i])
                                            break;
                                    if (i == s.Length)
                                    {
                                        statusLineValues.StatusDescription = s;
                                        bytesParsed += s.Length;
                                    }
                                    break;
                                }
                            }
                        }

                        int beginning = bytesParsed;

                        while (byteBuffer[bytesParsed] != '\r')
                        {
                            if (byteBuffer[bytesParsed] < ' ' || byteBuffer[bytesParsed] == 127)
                            {
                                parseStatus = DataParseStatus.Invalid;
                                goto quit;
                            }

                            if (++bytesParsed == effectiveMax)
                            {
                                string s = WebHeaderCollection.HeaderEncoding.GetString(byteBuffer + beginning, bytesParsed - beginning);
                                if (statusLineValues.StatusDescription == null)
                                    statusLineValues.StatusDescription = s;
                                else
                                    statusLineValues.StatusDescription += s;

                                goto quit;
                            }
                        }

                        if (bytesParsed > beginning)
                        {
                            string s = WebHeaderCollection.HeaderEncoding.GetString(byteBuffer + beginning, bytesParsed - beginning);
                            if (statusLineValues.StatusDescription == null)
                                statusLineValues.StatusDescription = s;
                            else
                                statusLineValues.StatusDescription += s;
                        }
                        else if (statusLineValues.StatusDescription == null)
                        {
                            statusLineValues.StatusDescription = "";
                        }

                        statusState = AfterCarriageReturn;

                        // Move past the CR.
                        if (++bytesParsed == effectiveMax)
                            goto quit;

                        goto case AfterCarriageReturn;
                    }

                    case AfterCarriageReturn:
                        if (byteBuffer[bytesParsed] != '\n')
                        {
                            parseStatus = DataParseStatus.Invalid;
                            goto quit;
                        }

                        parseStatus = DataParseStatus.Done;
                        bytesParsed++;
                        break;
                }
            }

quit:
            totalBytesParsed += bytesParsed - initialBytesParsed;

            GlobalLog.Print("Connection::ParseStatusLineStrict() StatusCode:" + statusLineValues.StatusCode + " MajorVersionNumber:" + statusLineValues.MajorVersion + " MinorVersionNumber:" + statusLineValues.MinorVersion + " StatusDescription:" + ValidationHelper.ToString(statusLineValues.StatusDescription));
            GlobalLog.Leave("Connection::ParseStatusLineStrict", parseStatus.ToString());

            if (parseStatus == DataParseStatus.Invalid) {
                parseError.Section = WebParseErrorSection.ResponseStatusLine;
                parseError.Code = WebParseErrorCode.Generic;
            }

            return parseStatus;
        }


        /*++

        Routine Description:

           SetStatusLineParsed - processes the result of status line,
             after it has been parsed, reads vars and formats result of parsing

        Arguments:

           None - uses member vars

        Return Value:

           None

        --*/

        private void SetStatusLineParsed() {
            // transfer this to response data
            m_ResponseData.m_StatusCode = (HttpStatusCode) m_StatusLineValues.StatusCode;
            m_ResponseData.m_StatusDescription = m_StatusLineValues.StatusDescription;
            m_ResponseData.m_IsVersionHttp11 = m_StatusLineValues.MajorVersion >= 1 && m_StatusLineValues.MinorVersion >= 1;
            if (ServicePoint.HttpBehaviour==HttpBehaviour.Unknown || ServicePoint.HttpBehaviour==HttpBehaviour.HTTP11 && !m_ResponseData.m_IsVersionHttp11) {
                // it's only safe to start doing HTTP/1.1 behaviour if the server's version was unknown
                // or if we need to downgrade
                ServicePoint.HttpBehaviour = m_ResponseData.m_IsVersionHttp11 ? HttpBehaviour.HTTP11 : HttpBehaviour.HTTP10;
            }

            m_CanPipeline = ServicePoint.SupportsPipelining;
        }

        /*++

            ProcessHeaderData - Pulls out Content-length, and other critical
                data from the newly parsed headers

            Input:

                Nothing.

            Returns:

                long - size of contentLength that we are to use

        --*/
        private long ProcessHeaderData(ref bool fHaveChunked, HttpWebRequest request, out bool dummyResponseStream)
        {
            long contentLength = -1;
            fHaveChunked = false;
            //
            // Check for the "Transfer-Encoding" header to contain the "chunked" string
            //
            string transferEncodingString = m_ResponseData.m_ResponseHeaders[HttpKnownHeaderNames.TransferEncoding];
            if (transferEncodingString!=null) {
                transferEncodingString = transferEncodingString.ToLower(CultureInfo.InvariantCulture);
                fHaveChunked = transferEncodingString.IndexOf(HttpWebRequest.ChunkedHeader) != -1;
            }

            if (!fHaveChunked) {
                //
                // If the response is not chunked, parse the "Content-Length" into a long for data size.
                //
                string contentLengthString = m_ResponseData.m_ResponseHeaders.ContentLength;
                if (contentLengthString!=null) {
                    int index = contentLengthString.IndexOf(':');
                    if (index!=-1) {
                        contentLengthString = contentLengthString.Substring(index + 1);
                    }
                    bool success = long.TryParse(contentLengthString, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out contentLength);
                    if (!success) {
                        contentLength = -1;
                        //   in some very rare cases, a proxy server may
                        //   send us a pair of numbers in comma delimated
                        //   fashion, so we need to handle this case
                        index = contentLengthString.LastIndexOf(',');
                        if (index!=-1) {
                            contentLengthString = contentLengthString.Substring(index + 1);
                            success = long.TryParse(contentLengthString, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out contentLength);
                            if (!success) {
                                contentLength = -1;
                            }
                        }
                    }
                    if (contentLength < 0)
                    {
                        GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ProcessHeaderData - ContentLength value in header: " + contentLengthString + ", HttpWebRequest#"+ValidationHelper.HashString(m_CurrentRequest));
                        contentLength = c_InvalidContentLength; // This will indicate a CL error to the caller
                    }
                }
            }

            // ** else ** signal no content-length present??? or error out?
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ProcessHeaderData() Content-Length parsed:" + contentLength.ToString(NumberFormatInfo.InvariantInfo));

            dummyResponseStream = !request.CanGetResponseStream || m_ResponseData.m_StatusCode < HttpStatusCode.OK ||
                                  m_ResponseData.m_StatusCode == HttpStatusCode.NoContent || (m_ResponseData.m_StatusCode == HttpStatusCode.NotModified && contentLength < 0) ;

            if (m_KeepAlive)
            {
                //
                // Deciding on  KEEP ALIVE
                //
                bool resetKeepAlive = false;

                //(1) if no content-length and no chunked, then turn off keep-alive
                //    In some cases, though, Content-Length should be assumed to be 0 based on HTTP RFC 2616
                if (!dummyResponseStream && contentLength < 0 && !fHaveChunked)
                {
                    resetKeepAlive = true;
                }
                //(2) A workaround for a failed client ssl session on IIS6
                //    The problem is that we cannot change the connection group name after it gets created.
                //    IIS6 does not close the connection on 403 so all subsequent requests will fail to be authorized on THAT connection.
                //-----------------------------------------------------------------------------------------------
                //5/15/2006
                //Microsoft
                //The DTS Issue 595216 claims that we are unnecessarily closing the
                //connection on 403 - even if it is a non SSL request. It seems
                //that the original intention is to close the request for SSL requests
                //The following code change would enforce closing onl fo SSL requests.
                //-----------------------------------------------------------------------------------------------
                else if (m_ResponseData.m_StatusCode == HttpStatusCode.Forbidden && NetworkStream is TlsStream)
                {
                    resetKeepAlive = true;
                }
                // (3) Possibly cease posting a big body on the connection, was invented mainly for the very first 401 response
                //
                //     This optimization is for the discovery legs only.  For ntlm this is fine, because the 1st actual authleg
                //     is always sent w/ content-length = 0.
                //     For Kerberos preauth, it there could be 1 or 2 auth legs, but we don't know how many there are in advance,
                //     so we don't have a way of eliminating the 1st auth leg.
                else if (m_ResponseData.m_StatusCode > HttpWebRequest.MaxOkStatus &&
                         ((request.CurrentMethod == KnownHttpVerb.Post || request.CurrentMethod == KnownHttpVerb.Put) &&
                            m_MaximumUnauthorizedUploadLength >= 0 && request.ContentLength > m_MaximumUnauthorizedUploadLength
                            && (request.CurrentAuthenticationState == null || request.CurrentAuthenticationState.Module == null)))
                {
                    resetKeepAlive = true;
                }
                //(4) for Http/1.0 servers, we can't be sure what their behavior
                //    in this case, so the best thing is to disable KeepAlive unless explicitly set
                //
                else
                {
                    //QFE: 4599.
                    //Author: Microsoft
                    //in v2.0, in case of SSL Requests through proxy that require NTLM authentication,
                    //we are not honoring the Proxy-Connection: Keep-Alive header and
                    //closing the connection.
                    //
                    //In v1.1 we did not have this issue because in v1.1, we would have set an
                    //EmptyProxy on the CONNECT request which kind of made it look like the
                    //service point is a proxy service point
                    //
                    //In v2.0, we don't use the GlobalProxySelection.GetEmptyWebProxy we use null
                    //to indicate we are not using a proxy.
                    //The CONNECT request is a proxy request and the service point is to the
                    //proxy.
                    //Design Notes
                    //------------
                    //This is a surgical fix. The "UsesProxySemantics is defined as
                    //ServicePoint is a Proxy Service point && (scheme is != https || the request is a tunnel request)
                    //Ideally we use one definition of whether we are going trough a proxy or not.
                    //The fact is that if you are connecting to a proxy, it is a proxy request and
                    //you should honor the Proxy-Connection header.
                    //
                    //For the purpose of this QFE, when we receive a header we test
                    //if this is a Proxy Service Point OR if this is a TUNNEL request



                    bool haveClose = false;
                    bool haveKeepAlive = false;
                    string connection = m_ResponseData.m_ResponseHeaders[HttpKnownHeaderNames.Connection];
                    if (connection == null && (
                        (ServicePoint.InternalProxyServicePoint) ||
                        (request.IsTunnelRequest)))
                    {
                        connection = m_ResponseData.m_ResponseHeaders[HttpKnownHeaderNames.ProxyConnection];
                    }

                    if (connection != null) {
                        connection = connection.ToLower(CultureInfo.InvariantCulture);
                        if (connection.IndexOf("keep-alive") != -1) {
                            haveKeepAlive = true;
                        }
                        else if (connection.IndexOf("close") != -1) {
                            haveClose = true;
                        }
                    }

                    if ((haveClose && ServicePoint.HttpBehaviour==HttpBehaviour.HTTP11) ||
                        (!haveKeepAlive && ServicePoint.HttpBehaviour<=HttpBehaviour.HTTP10))
                    {
                        resetKeepAlive = true;
                    }
                }


                if (resetKeepAlive)
                {
                    lock (this) {
                        m_KeepAlive = false;
                        m_Free = false;
                    }
                }
            }

            return contentLength;
        }

        internal bool KeepAlive
        {
            get
            {
                return m_KeepAlive;
            }
        }

        internal bool NonKeepAliveRequestPipelined
        {
            get
            {
                return m_NonKeepAliveRequestPipelined;
            }
        }

        /*++

            ParseStreamData

            Handles parsing of the blocks of data received after buffer,
             distributes the data to stream constructors as needed

            returnResult - contains a object containing Requests
                that must be notified upon return from callback

        --*/
        private DataParseStatus ParseStreamData(ref ConnectionReturnResult returnResult)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::ParseStreamData");

            if (m_CurrentRequest == null)
            {
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ParseStreamData - Aborted Request, return DataParseStatus.Invalid");
                m_ParseError.Section = WebParseErrorSection.Generic;
                m_ParseError.Code    = WebParseErrorCode.UnexpectedServerResponse;
                return DataParseStatus.Invalid;
            }

            bool fHaveChunked = false;
            bool dummyResponseStream;
            // content-length if there is one
            long contentLength = ProcessHeaderData(ref fHaveChunked, m_CurrentRequest, out dummyResponseStream);

            GlobalLog.Assert(!fHaveChunked || contentLength == -1, "Connection#{0}::ParseStreamData()|fHaveChunked but contentLength != -1", ValidationHelper.HashString(this));

            if (contentLength == c_InvalidContentLength)
            {
                m_ParseError.Section = WebParseErrorSection.ResponseHeader;
                m_ParseError.Code    = WebParseErrorCode.InvalidContentLength;
                return DataParseStatus.Invalid;
            }

            // bytes left over that have not been parsed
            int bufferLeft = (m_BytesRead - m_BytesScanned);

            if (m_ResponseData.m_StatusCode > HttpWebRequest.MaxOkStatus)
            {
                // This will tell the request to be prepared for possible connection drop
                // Also that will stop writing on the wire if the connection is not kept alive
                m_CurrentRequest.ErrorStatusCodeNotify(this, m_KeepAlive, false);
            }

            int bytesToCopy;
            //
            //  If pipelining, then look for extra data that could
            //  be part of of another stream, if its there,
            //  then we need to copy it, add it to a stream,
            //  and then continue with the next headers
            //

            if (dummyResponseStream)
            {
                bytesToCopy = 0;
                fHaveChunked = false;
            }
            else
            {
                bytesToCopy = -1;

                if (!fHaveChunked && (contentLength <= (long)Int32.MaxValue))
                {
                    bytesToCopy = (int)contentLength;
                }
            }

            DataParseStatus result;

            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseStreamData() bytesToCopy:" + bytesToCopy + " bufferLeft:" + bufferLeft);

            if (m_CurrentRequest.IsWebSocketRequest && m_ResponseData.m_StatusCode == HttpStatusCode.SwitchingProtocols)
            {
                m_ResponseData.m_ConnectStream = new ConnectStream(this, m_ReadBuffer, m_BytesScanned, bufferLeft, bufferLeft, fHaveChunked, m_CurrentRequest);

                // The parsing will be resumed from m_BytesScanned when response stream is closed.
                result = DataParseStatus.Done;
                ClearReaderState();
            }
            else if (bytesToCopy != -1 && bytesToCopy <= bufferLeft)
            {
                m_ResponseData.m_ConnectStream = new ConnectStream(this, m_ReadBuffer, m_BytesScanned, bytesToCopy, dummyResponseStream? 0: contentLength, fHaveChunked, m_CurrentRequest);

                // The parsing will be resumed from m_BytesScanned when response stream is closed.
                result = DataParseStatus.ContinueParsing;
                m_BytesScanned += bytesToCopy;
            }
            else
            {
                m_ResponseData.m_ConnectStream = new ConnectStream(this, m_ReadBuffer, m_BytesScanned, bufferLeft, dummyResponseStream? 0: contentLength, fHaveChunked, m_CurrentRequest);

                // This is the default case where we have a buffer with no more streams except the last one to create so we create it.
                // Note the buffer is fully consumed so we can reset the buffer offests.
                result = DataParseStatus.Done;
                ClearReaderState();
            }

            m_ResponseData.m_ContentLength = contentLength;
            ConnectionReturnResult.Add(ref returnResult, m_CurrentRequest, m_ResponseData.Clone());

#if DEBUG
            GlobalLog.DebugUpdateRequest(m_CurrentRequest, this, GlobalLog.WaitingForReadDoneFlag);
#endif

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ParseStreamData");
            return result; // response stream is taking over the reading
        }

        // Called before restarting Read operations
        private void ClearReaderState() {
            m_BytesRead    = 0;
            m_BytesScanned = 0;
        }

        /*++

            ParseResponseData - Parses the incomming headers, and handles
              creation of new streams that are found while parsing, and passes
              extra data the new streams

            Input:

                returnResult - returns an object containing items that need to be called
                    at the end of the read callback

            Returns:

                bool - true if one should continue reading more data

        --*/
        private DataParseStatus ParseResponseData(ref ConnectionReturnResult returnResult, out bool requestDone, out CoreResponseData continueResponseData)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData()");

            DataParseStatus parseStatus = DataParseStatus.NeedMoreData;
            DataParseStatus parseSubStatus;

            // Indicates whether or not at least one whole request was processed in this loop.
            // (i.e. Whether ParseStreamData() was called.
            requestDone = false;
            continueResponseData = null;

            // loop in case of multiple sets of headers or streams,
            //  that may be generated due to a pipelined response

                // Invariants: at the start of this loop, m_BytesRead
                // is the number of bytes in the buffer, and m_BytesScanned
                // is how many bytes of the buffer we've consumed so far.
                // and the m_ReadState var will be updated at end of
                // each code path, call to this function to reflect,
                // the state, or error condition of the parsing of data
                //
                // We use the following variables in the code below:
                //
                //  m_ReadState - tracks the current state of our Parsing in a
                //      response. z.B.
                //      Start - initial start state and begining of response
                //      StatusLine - the first line sent in response, include status code
                //      Headers - \r\n delimiated Header parsing until we find entity body
                //      Data - Entity Body parsing, if we have all data, we create stream directly
                //
                //  m_ResponseData - An object used to gather Stream, Headers, and other
                //      tidbits so that a request/Response can receive this data when
                //      this code is finished processing
                //
                //  m_ReadBuffer - Of course the buffer of data we are parsing from
                //
                //  m_BytesScanned - The bytes scanned in this buffer so far,
                //      since its always assumed that parse to completion, this
                //      var becomes ended of known data at the end of this function,
                //      based off of 0
                //
                //  m_BytesRead - The total bytes read in buffer, should be const,
                //      till its updated at end of function.
                //

                //
                // Now attempt to parse the data,
                //   we first parse status line,
                //   then read headers,
                //   and finally transfer results to a new stream, and tell request
                //

                switch (m_ReadState) {

                    case ReadState.Start:


                        if (m_CurrentRequest == null)
                        {
                            lock (this)
                            {
                                if (m_WriteList.Count == 0 || ((m_CurrentRequest = m_WriteList[0] as HttpWebRequest) == null))
                                {
                                    m_ParseError.Section = WebParseErrorSection.Generic;
                                    m_ParseError.Code    = WebParseErrorCode.Generic;
                                    parseStatus = DataParseStatus.Invalid;
                                    break;
                                }
                            }
                        }

                        //
                        // Start of new response. Transfer the keep-alive context from the corresponding request to
                        // the connection
                        //
                        m_KeepAlive &= (m_CurrentRequest.KeepAlive || m_CurrentRequest.NtlmKeepAlive);

                        m_MaximumResponseHeadersLength = m_CurrentRequest.MaximumResponseHeadersLength * 1024;
                        m_ResponseData = new CoreResponseData();
                        m_ReadState = ReadState.StatusLine;
                        m_TotalResponseHeadersLength = 0;

                        InitializeParseStatusLine();
                        goto case ReadState.StatusLine;

                    case ReadState.StatusLine:
                        //
                        // Reads HTTP status response line
                        //
                        if (SettingsSectionInternal.Section.UseUnsafeHeaderParsing)
                        {
                            // This one uses an array to store the parsed values in.  Marshal between this legacy way.
                            int[] statusInts = new int[] { 0, m_StatusLineValues.MajorVersion, m_StatusLineValues.MinorVersion, m_StatusLineValues.StatusCode };
                            if (m_StatusLineValues.StatusDescription == null)
                                m_StatusLineValues.StatusDescription = "";

                            parseSubStatus = ParseStatusLine(
                                m_ReadBuffer, // buffer we're working with
                                m_BytesRead,  // total bytes read so far
                                ref m_BytesScanned, // index off of what we've scanned
                                ref statusInts,
                                ref m_StatusLineValues.StatusDescription,
                                ref m_StatusState,
                                ref m_ParseError);

                            m_StatusLineValues.MajorVersion = statusInts[1];
                            m_StatusLineValues.MinorVersion = statusInts[2];
                            m_StatusLineValues.StatusCode = statusInts[3];
                        }
                        else
                        {
                            parseSubStatus = ParseStatusLineStrict(
                                m_ReadBuffer, // buffer we're working with
                                m_BytesRead,  // total bytes read so far
                                ref m_BytesScanned, // index off of what we've scanned
                                ref m_StatusState,
                                m_StatusLineValues,
                                m_MaximumResponseHeadersLength,
                                ref m_TotalResponseHeadersLength,
                                ref m_ParseError);
                        }

                        if (parseSubStatus == DataParseStatus.Done)
                        {
                            if (Logging.On) Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_received_status_line, m_StatusLineValues.MajorVersion+"."+m_StatusLineValues.MinorVersion, m_StatusLineValues.StatusCode, m_StatusLineValues.StatusDescription));
                            SetStatusLineParsed();
                            m_ReadState = ReadState.Headers;
                            m_ResponseData.m_ResponseHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebResponse);
                            goto case ReadState.Headers;
                        }
                        else if (parseSubStatus != DataParseStatus.NeedMoreData)
                        {
                            //
                            // report error - either Invalid or DataTooBig
                            //
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() ParseStatusLine() parseSubStatus:" + parseSubStatus.ToString());
                            parseStatus = parseSubStatus;
                            break;
                        }

                        break; // read more data

                    case ReadState.Headers:
                        //
                        // Parse additional lines of header-name: value pairs
                        //
                        if (m_BytesScanned >= m_BytesRead) {
                            //
                            // we already can tell we need more data
                            //
                            break;
                        }

                        if (SettingsSectionInternal.Section.UseUnsafeHeaderParsing)
                        {
                            parseSubStatus = m_ResponseData.m_ResponseHeaders.ParseHeaders(
                                m_ReadBuffer,
                                m_BytesRead,
                                ref m_BytesScanned,
                                ref m_TotalResponseHeadersLength,
                                m_MaximumResponseHeadersLength,
                                ref m_ParseError);
                        }
                        else
                        {
                            parseSubStatus = m_ResponseData.m_ResponseHeaders.ParseHeadersStrict(
                                m_ReadBuffer,
                                m_BytesRead,
                                ref m_BytesScanned,
                                ref m_TotalResponseHeadersLength,
                                m_MaximumResponseHeadersLength,
                                ref m_ParseError);
                        }

                        if (parseSubStatus == DataParseStatus.Invalid || parseSubStatus == DataParseStatus.DataTooBig)
                        {
                            //
                            // report error
                            //
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() ParseHeaders() parseSubStatus:" + parseSubStatus.ToString());
                            parseStatus = parseSubStatus;
                            break;
                        }
                        else if (parseSubStatus == DataParseStatus.Done)
                        {
                            if (Logging.On) Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_received_headers, m_ResponseData.m_ResponseHeaders.ToString(true)));

                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() DataParseStatus.Done StatusCode:" + (int)m_ResponseData.m_StatusCode + " m_BytesRead:" + m_BytesRead + " m_BytesScanned:" + m_BytesScanned);

                            //get the IIS server version
                            if(m_IISVersion == -1){
                                string server = m_ResponseData.m_ResponseHeaders.Server;
                                if (server != null && server.ToLower(CultureInfo.InvariantCulture).Contains("microsoft-iis")){
                                    int i = server.IndexOf("/");
                                    if(i++>0 && i <server.Length){
                                        m_IISVersion = server[i++] - '0';
                                        while(i < server.Length && Char.IsDigit(server[i])) {
                                            m_IISVersion = m_IISVersion*10 + server[i++] - '0';
                                        }
                                    }
                                }
                                //we got a response,so if we don't know the server by now and it wasn't a 100 continue,
                                //we can't assume we will ever know it.  IIS5 sends its server header w/ the continue

                                if(m_IISVersion == -1 && m_ResponseData.m_StatusCode != HttpStatusCode.Continue){
                                    m_IISVersion = 0;
                                }
                            }

                            if (m_ResponseData.m_StatusCode == HttpStatusCode.Continue || m_ResponseData.m_StatusCode == HttpStatusCode.BadRequest) {

                                GlobalLog.Assert(m_CurrentRequest != null, "Connection#{0}::ParseResponseData()|m_CurrentRequest == null", ValidationHelper.HashString(this));
                                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() HttpWebRequest#" + ValidationHelper.HashString(m_CurrentRequest));

                                if (m_ResponseData.m_StatusCode == HttpStatusCode.BadRequest) {
                                    // If we have a 400 and we were sending a chunked request going through to a proxy with a chunked upload,
                                    // this proxy is a partially compliant so shut off fancy features (pipelining and chunked uploads)
                                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() got a 400 StatusDescription:" + m_ResponseData.m_StatusDescription);
                                    if (ServicePoint.HttpBehaviour == HttpBehaviour.HTTP11
                                        && m_CurrentRequest.HttpWriteMode==HttpWriteMode.Chunked
                                        && m_ResponseData.m_ResponseHeaders.Via != null
                                        && string.Compare(m_ResponseData.m_StatusDescription, "Bad Request ( The HTTP request includes a non-supported header. Contact the Server administrator.  )", StringComparison.OrdinalIgnoreCase)==0) {
                                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() downgrading server to HTTP11PartiallyCompliant.");
                                        ServicePoint.HttpBehaviour = HttpBehaviour.HTTP11PartiallyCompliant;
                                    }
                                }
                                else {
                                    // If we have an HTTP continue, eat these headers and look
                                    //  for the 200 OK
                                    //
                                    // we got a 100 Continue. set this on the HttpWebRequest
                                    //
                                    m_CurrentRequest.Saw100Continue = true;
                                    if (!ServicePoint.Understands100Continue) {
                                        //
                                        // and start expecting it again if this behaviour was turned off
                                        //
                                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() HttpWebRequest#" + ValidationHelper.HashString(m_CurrentRequest) + " ServicePoint#" + ValidationHelper.HashString(ServicePoint) + " sent UNexpected 100 Continue");
                                        ServicePoint.Understands100Continue = true;
                                    }

                                    //
                                    // set Continue Ack on request.
                                    //
                                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() calling SetRequestContinue()");
                                    continueResponseData = m_ResponseData;

                                    //if we got a 100continue we ---- it and start looking for a final response
                                    goto case ReadState.Start;
                                }
                            }

                            m_ReadState = ReadState.Data;
                            goto case ReadState.Data;
                        }

                        // need more data,
                        break;

                    case ReadState.Data:

                        // (check status code for continue handling)
                        // 1. Figure out if its Chunked, content-length, or encoded
                        // 2. Takes extra data, place in stream(s)
                        // 3. Wake up blocked stream requests
                        //

                        // Got through one entire response
                        requestDone = true;

                        // parse and create a stream if needed
                        parseStatus = ParseStreamData(ref returnResult);

                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() result:" + parseStatus);
                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData()" + " WriteDone:" + m_WriteDone + " ReadDone:" + m_ReadDone + " WaitList:" + m_WaitList.Count + " WriteList:" + m_WriteList.Count);
                        break;
                }

            if (m_BytesScanned == m_BytesRead)
                ClearReaderState();

            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData() m_ReadState:" + m_ReadState);
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ParseResponseData()", parseStatus.ToString());
            return parseStatus;
        }

        /// <devdoc>
        ///    <para>
        ///       Cause the Connection to Close and Abort its socket,
        ///         after the next request is completed.  If the Connection
        ///         is already idle, then Aborts the socket immediately.
        ///    </para>
        /// </devdoc>
        internal void CloseOnIdle() {
            // The timer thread is allowed to call this.  (It doesn't call user code and doesn't block.)
            GlobalLog.ThreadContract(ThreadKinds.Unknown, ThreadKinds.SafeSources | ThreadKinds.Timer, "Connection#" + ValidationHelper.HashString(this) + "::CloseOnIdle");

            lock(this){
                m_KeepAlive = false;
                m_RemovedFromConnectionList = true;
                if (!m_Idle)
                {
                    CheckIdle();
                }
                if (m_Idle)
                {
                    AbortSocket(false);
                    GC.SuppressFinalize(this);
                }
            }
        }

        internal bool AbortOrDisassociate(HttpWebRequest request, WebException webException)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::AbortOrDisassociate", "request#" + ValidationHelper.HashString(request));
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::AbortOrDisassociate()");

            ConnectionReturnResult result = null;
            lock(this)
            {
                int idx = m_WriteList.IndexOf(request);
                // If the request is in the submission AND this is the first request we have to abort the connection,
                // Otheriwse we simply disassociate it from the current connection.
                if (idx == -1)
                {
                    WaitListItem foundItem = null;

                    if (m_WaitList.Count > 0)
                    {
                        foundItem = m_WaitList.Find(o => object.ReferenceEquals(o.Request, request));
                    }

                    // If not found then the request must be already dispatched and the response stream is drained
                    // If so then we let request.Abort() to deal with this situation.
                    if (foundItem != null)
                    {
                        NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgQueueTime,
                            foundItem.QueueStartTime);
                        m_WaitList.Remove(foundItem);
                        UnlockIfNeeded(foundItem.Request);
                    }

                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::AbortOrDisassociate()", "Request was wisassociated");
                    return true;
                }
                else if (idx != 0)
                {
                    // Make this connection Keep-Alive=false, remove the request and do not close the connection
                    // When the active request completes, the rest of the pipeline (minus aborted request) will be resubmitted.
                    m_WriteList.RemoveAt(idx);
                    m_KeepAlive = false;
                    GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::AbortOrDisassociate()", "Request was Disassociated from the Write List, idx = " + idx);
                    return true;
                }

#if DEBUG
                try
                {
#endif
                m_KeepAlive = false;
                if (webException != null && m_InnerException == null)
                {
                    m_InnerException = webException;
                    m_Error = webException.Status;
                }
                else
                {
                    m_Error = WebExceptionStatus.RequestCanceled;
                }

                PrepareCloseConnectionSocket(ref result);
                // Hard Close the socket.
                Close(0);
                FreeReadBuffer();	// Do it after close completes to insure buffer not in use
#if DEBUG
                }
                catch (Exception exception)
                {
                    t_LastStressException = exception;
                    if (!NclUtilities.IsFatal(exception)){
                        GlobalLog.Assert("Connection#" + ValidationHelper.HashString(this) + "::AbortOrDisassociate()", exception.Message);
                    }
                }
#endif
            }
            ConnectionReturnResult.SetResponses(result);
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::AbortOrDisassociate()", "Connection Aborted");
            return false;
        }

#if DEBUG
        [ThreadStatic]
        private static Exception t_LastStressException;
#endif

        internal void AbortSocket(bool isAbortState)
        {
            // The timer/finalization thread is allowed to call this.  (It doesn't call user code and doesn't block.)
            GlobalLog.ThreadContract(ThreadKinds.Unknown, ThreadKinds.SafeSources | ThreadKinds.Timer | ThreadKinds.Finalization, "Connection#" + ValidationHelper.HashString(this) + "::AbortSocket");
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::Abort", "isAbortState:" + isAbortState.ToString());

            if (isAbortState) {
                UnlockRequest();
                CheckIdle();
            }
            else {
                // This one is recoverable, set it to keep  Read/Write StartNextRequest happy.
                m_Error = WebExceptionStatus.KeepAliveFailure;
            }

            // Stream close isn't threadsafe.
            lock (this)
            {
                Close(0);
                FreeReadBuffer();	// Do it after close completes to insure buffer not in use
            }

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::Abort", "isAbortState:" + isAbortState.ToString());
        }


        /*++

            PrepareCloseConnectionSocket - reset the connection requests list.

            This method is called when we want to close the conection.
            It must be called with the critical section held.
            The caller must call this.Close if decided to call this method.

            All connection closes (either ours or server initiated) eventually go through here.

            As to what we do: we loop through our write and wait list and pull requests
            off it, and give each request an error failure. Then the caller will
            dispatch the responses.

        --*/

        private void PrepareCloseConnectionSocket(ref ConnectionReturnResult returnResult)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::PrepareCloseConnectionSocket", m_Error.ToString());

            // Effectivelly, closing a connection makes it exempted from the "Idling" logic
            m_IdleSinceUtc = DateTime.MinValue;
            CanBePooled = false;

            if (m_WriteList.Count != 0 || m_WaitList.Count != 0)
            {

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::PrepareCloseConnectionSocket() m_WriteList.Count:" + m_WriteList.Count);
                DebugDumpWriteListEntries();
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::PrepareCloseConnectionSocket() m_WaitList.Count:" + m_WaitList.Count);
                DebugDumpWaitListEntries();

                HttpWebRequest lockedRequest = LockedRequest;

                if (lockedRequest != null)
                {
                    bool callUnlockRequest = false;
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::PrepareCloseConnectionSocket() looking for HttpWebRequest#" + ValidationHelper.HashString(lockedRequest));

                    foreach (HttpWebRequest request in m_WriteList)
                    {
                        if (request == lockedRequest) {
                            callUnlockRequest = true;
                        }
                    }

                    if (!callUnlockRequest) {
                        foreach (WaitListItem item in m_WaitList) {
                            if (item.Request == lockedRequest) {
                                callUnlockRequest = true;
                                break;
                            }
                        }
                    }
                    if (callUnlockRequest) {
                        UnlockRequest();
                    }
                }

                HttpWebRequest[] requestArray = null;

                // WaitList gets Isolated exception status, free to retry multiple times
                if (m_WaitList.Count != 0)
                {
                    requestArray = new HttpWebRequest[m_WaitList.Count];
                    for (int i = 0; i < m_WaitList.Count; i++)
                    {
                        requestArray[i] = m_WaitList[i].Request;
                    }
                    ConnectionReturnResult.AddExceptionRange(ref returnResult, requestArray, ExceptionHelper.IsolatedException);
                }

                //
                // WriteList (except for single request list) gets Recoverable exception status, may be retired if not failed once
                // For a single request list the exception is computed here
                // InnerExeption if any may tell more details in both cases
                //
                if (m_WriteList.Count != 0)
                {
                    Exception theException = m_InnerException;


                    if(theException != null)
                       GlobalLog.Print(theException.ToString());

                    GlobalLog.Print("m_Error = "+ m_Error.ToString());

                    if (!(theException is WebException) && !(theException is SecurityException))
                    {
                        if (m_Error == WebExceptionStatus.ServerProtocolViolation)
                        {
                            string errorString = NetRes.GetWebStatusString(m_Error);

                            string detailedInfo = "";
                            if (m_ParseError.Section != WebParseErrorSection.Generic)
                                detailedInfo += " Section=" + m_ParseError.Section.ToString();
                            if (m_ParseError.Code != WebParseErrorCode.Generic) {
                                detailedInfo += " Detail=" + SR.GetString("net_WebResponseParseError_" + m_ParseError.Code.ToString());
                            }
                            if (detailedInfo.Length != 0)
                                errorString += "." + detailedInfo;

                            theException = new WebException(errorString,
                                                            theException,
                                                            m_Error,
                                                            null,
                                                            WebExceptionInternalStatus.RequestFatal);
                        }
                        else if (m_Error == WebExceptionStatus.SecureChannelFailure)
                        {
                            theException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.SecureChannelFailure),
                                                            WebExceptionStatus.SecureChannelFailure);
                        }

                        else if (m_Error == WebExceptionStatus.Timeout)
                        {
                            theException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.Timeout),
                                                            WebExceptionStatus.Timeout);
                        }
                        else if(m_Error == WebExceptionStatus.RequestCanceled)
                        {
                            theException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled),
                                                            WebExceptionStatus.RequestCanceled,
                                                            WebExceptionInternalStatus.RequestFatal,
                                                            theException);
                        }
                        else if(m_Error == WebExceptionStatus.MessageLengthLimitExceeded ||
                                m_Error == WebExceptionStatus.TrustFailure)
                        {
                            theException = new WebException(NetRes.GetWebStatusString("net_connclosed", m_Error),
                                                            m_Error,
                                                            WebExceptionInternalStatus.RequestFatal,
                                                            theException);
                        }
                        else
                        {
                            if (m_Error == WebExceptionStatus.Success)
                            {
                                throw new InternalException();              // 
                                //m_Error = WebExceptionStatus.UnknownError;
                            }

                            bool retry = false;
                            bool isolatedKeepAliveFailure = false;

                            if (m_WriteList.Count != 1)
                            {
                                // Real scenario: SSL against IIS-5 would fail if pipelinning.
                                // retry = true will cover a general case when >>the server<< aborts a pipeline
                                // Basically all pipelined requests are marked with recoverable error including the very active request.
                                retry = true;
                            }
                            else if (m_Error == WebExceptionStatus.KeepAliveFailure)
                            {
                                HttpWebRequest request = (HttpWebRequest) m_WriteList[0];
                                // Check that the active request did not start the body yet
                                if (!request.BodyStarted)
                                    isolatedKeepAliveFailure = true;
                            }
                            else{
                                retry = (!AtLeastOneResponseReceived && !((HttpWebRequest) m_WriteList[0]).BodyStarted);
                            }
                                theException = new WebException(NetRes.GetWebStatusString("net_connclosed", m_Error),
                                                            m_Error,
                                                            (isolatedKeepAliveFailure? WebExceptionInternalStatus.Isolated:
                                                                retry? WebExceptionInternalStatus.Recoverable:
                                                                WebExceptionInternalStatus.RequestFatal),
                                                            theException);
                        }
                    }

                    WebException pipelineException = new WebException(NetRes.GetWebStatusString("net_connclosed", WebExceptionStatus.PipelineFailure),
                                                                      WebExceptionStatus.PipelineFailure,
                                                                      WebExceptionInternalStatus.Recoverable,
                                                                      theException);

                    requestArray = new HttpWebRequest[m_WriteList.Count];
                    m_WriteList.CopyTo(requestArray, 0);
                    ConnectionReturnResult.AddExceptionRange(ref returnResult, requestArray, pipelineException, theException);
                }

#if TRAVE
                foreach (WaitListItem item in m_WaitList) {
                    GlobalLog.Print("Request removed from WaitList#"+ValidationHelper.HashString(item.Request));
                }

                foreach (HttpWebRequest request in m_WriteList) {
                    GlobalLog.Print("Request removed from m_WriteList#"+ValidationHelper.HashString(request));
                }
#endif

                m_WriteList.Clear();

                foreach (WaitListItem item in m_WaitList)
                {
                    NetworkingPerfCounters.Instance.IncrementAverage(NetworkingPerfCounterName.HttpWebRequestAvgQueueTime,
                        item.QueueStartTime);
                }
                m_WaitList.Clear();
            }

            CheckIdle();

            if (m_Idle)
            {
                GC.SuppressFinalize(this);
            }
            if (!m_RemovedFromConnectionList && ConnectionGroup != null)
            {
                m_RemovedFromConnectionList = true;
                ConnectionGroup.Disassociate(this);
            }

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::PrepareCloseConnectionSocket");
        }


        /*++

            HandleError - Handle a protocol error from the server.

            This method is called when we've detected some sort of fatal protocol
            violation while parsing a response, receiving data from the server,
            or failing to connect to the server. We'll fabricate
            a WebException and then call CloseConnection which closes the
            connection as well as informs the request through a callback.

            Input:
                    webExceptionStatus -
                    connectFailure -
                    readFailure -

            Returns: Nothing

        --*/
        internal void HandleConnectStreamException(bool writeDone, bool readDone, WebExceptionStatus webExceptionStatus, ref ConnectionReturnResult returnResult, Exception e)
        {
            if (m_InnerException == null)
            {
                m_InnerException = e;
                if (!(e is WebException) && NetworkStream is TlsStream)
                {
                    // Unless a WebException is passed the Connection knows better the error code if the transport is TlsStream
                    webExceptionStatus = ((TlsStream) NetworkStream).ExceptionStatus;
                }
                else if (e is ObjectDisposedException)
                {
                    webExceptionStatus = WebExceptionStatus.RequestCanceled;
                }
            }
            HandleError(writeDone, readDone, webExceptionStatus, ref returnResult);
        }
        //
        private void HandleErrorWithReadDone(WebExceptionStatus webExceptionStatus, ref ConnectionReturnResult returnResult)
        {
            HandleError(false, true, webExceptionStatus, ref returnResult);
        }
        //
        private void HandleError(bool writeDone, bool readDone, WebExceptionStatus webExceptionStatus, ref ConnectionReturnResult returnResult)
        {
            lock(this)
            {
                if (writeDone)
                    m_WriteDone = true;
                if (readDone)
                    m_ReadDone = true;

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::HandleError() m_WriteList.Count:" + m_WriteList.Count +
                                " m_WaitList.Count:" + m_WaitList.Count +
                                " new WriteDone:" + m_WriteDone + " new ReadDone:" + m_ReadDone);
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::HandleError() current HttpWebRequest#" + ValidationHelper.HashString(m_CurrentRequest));

                if(webExceptionStatus == WebExceptionStatus.Success)
                    throw new InternalException(); //consider making an assert later.

                m_Error = webExceptionStatus;

                PrepareCloseConnectionSocket(ref returnResult);
                // This will kill the socket
                // Must be done inside the lock.  (Stream Close() isn't threadsafe.)
                Close(0);
                FreeReadBuffer();	// Do it after close completes to insure buffer not in use
            }
        }

        private static void ReadCallbackWrapper(IAsyncResult asyncResult)
        {
            if (asyncResult.CompletedSynchronously)
            {
                return;
            }

            ((Connection) asyncResult.AsyncState).ReadCallback(asyncResult);
        }

        /// <devdoc>
        ///    <para>Performs read callback processing on connection
        ///     handles getting headers parsed and streams created</para>
        /// </devdoc>
        private void ReadCallback(IAsyncResult asyncResult)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::ReadCallback", ValidationHelper.HashString(asyncResult));

            int bytesRead = -1;
            WebExceptionStatus errorStatus = WebExceptionStatus.ReceiveFailure;

            //
            // parameter validation
            //
            GlobalLog.Assert(asyncResult != null, "Connection#{0}::ReadCallback()|asyncResult == null", ValidationHelper.HashString(this));
            GlobalLog.Assert((asyncResult is OverlappedAsyncResult || asyncResult is LazyAsyncResult), "Connection#{0}::ReadCallback()|asyncResult is not OverlappedAsyncResult.", ValidationHelper.HashString(this));

            try {
                bytesRead = EndRead(asyncResult);
                if (bytesRead == 0)
                    bytesRead = -1; // 0 is reserved for re-entry on already buffered data

                errorStatus = WebExceptionStatus.Success;
            }
            catch (Exception exception) {
                // Notify request's SubmitWriteStream that a socket error happened.  This will cause future writes to
                   // throw an IOException.
                   HttpWebRequest curRequest = m_CurrentRequest;
                   if (curRequest != null)
                   {
                       curRequest.ErrorStatusCodeNotify(this, false, true);
                   }


                if (m_InnerException == null)
                    m_InnerException = exception;

                if (exception.GetType() == typeof(ObjectDisposedException))
                    errorStatus = WebExceptionStatus.RequestCanceled;

#if !FEATURE_PAL
                //ASYNCISSUE
                // Consider: In case of a async exception we should do minimal cleanup here trying the appDomain
                // to survive or to force unloading of the appDomain .
                // need to handle SSL errors too
                if (NetworkStream is TlsStream)  {
                    errorStatus = ((TlsStream) NetworkStream).ExceptionStatus;
                }
                else {
                    errorStatus = WebExceptionStatus.ReceiveFailure;
                }
#else
                errorStatus = WebExceptionStatus.ReceiveFailure;
#endif // !FEATURE_PAL
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadCallback() EndRead() errorStatus:" + errorStatus.ToString() + " caught exception:" + exception);
            }

            ReadComplete(bytesRead, errorStatus);
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::ReadCallback");
        }


        /// <devdoc>
        ///    <para>Attempts to poll the socket, to see if data is waiting to be read,
        ///     if there is data there, then a read is started</para>
        /// </devdoc>
        internal void PollAndRead(HttpWebRequest request, bool userRetrievedStream) {
            GlobalLog.ThreadContract(ThreadKinds.Unknown, "Connection#" + ValidationHelper.HashString(this) + "::PollAndRead");

            // Ensure that we don't already have a response for this request, before we attempt to read the socket.
            request.NeedsToReadForResponse = true;
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::PollAndRead() InternalPeekCompleted:" + request.ConnectionReaderAsyncResult.InternalPeekCompleted.ToString() + " Result:" + ValidationHelper.ToString(request.ConnectionReaderAsyncResult.Result));
            if (request.ConnectionReaderAsyncResult.InternalPeekCompleted && request.ConnectionReaderAsyncResult.Result == null && CanBePooled)
            {
                SyncRead(request, userRetrievedStream, true);
            }
        }
        //
        //    Peforms a Sync Read and calls the ReadComplete to process the result
        //    The reads are done iteratively, until the Request has received enough
        //    data to contruct a response, or a 100-Continue is read, allowing the HttpWebRequest
        //    to return a write stream
        //
        //    probeRead = true only for POST request and when the caller needs to wait for 100-continue
        //
        internal void SyncRead(HttpWebRequest request, bool userRetrievedStream, bool probeRead)
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::SyncRead(byte[]) request#" + ValidationHelper.HashString(request) + (probeRead? ", Probe read = TRUE":string.Empty));
            GlobalLog.ThreadContract(ThreadKinds.Sync, "Connection#" + ValidationHelper.HashString(this) + "::SyncRead");

            // prevent recursive calls to this function
            if (t_SyncReadNesting > 0) {
                GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SyncRead() - nesting");
                return;
            }

            bool pollSuccess = probeRead? false: true;

            try {
                t_SyncReadNesting++;

                // grab a counter to tell us whenever the SetRequestContinue is called
                int requestContinueCount = probeRead ? request.RequestContinueCount : 0;

                bool requestDone;
                int bytesRead = -1;
                WebExceptionStatus errorStatus = WebExceptionStatus.ReceiveFailure;


                if (m_BytesScanned < m_BytesRead)
                {
                    // left over from previous read
                    pollSuccess = true;
                    bytesRead = 0; //tell it we want to use buffered data on the first iteration
                    errorStatus = WebExceptionStatus.Success;
                }

                do {
                    requestDone = true;

                    try {
                        if (bytesRead != 0)
                        {
                            errorStatus = WebExceptionStatus.ReceiveFailure;

                            if (!pollSuccess)
                            {
                                pollSuccess = Poll(request.ContinueTimeout * 1000, SelectMode.SelectRead);  // Timeout is in microseconds
                                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SyncRead() PollSuccess : " + pollSuccess);
                            }

                            if (pollSuccess)
                            {
                                //Ensures that we'll timeout eventually on an appdomain unload.
                                //Will be a no-op if the timeout doesn't change from request to request.
                                ReadTimeout = request.Timeout;

                                bytesRead = Read(m_ReadBuffer, m_BytesRead, m_ReadBuffer.Length - m_BytesRead);
                                errorStatus = WebExceptionStatus.Success;
                                if (bytesRead == 0)
                                    bytesRead = -1; // 0 is reserved for re-entry on already buffered data
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception)) throw;

                        if (m_InnerException == null)
                            m_InnerException = exception;

                        if (exception.GetType() == typeof(ObjectDisposedException))
                            errorStatus = WebExceptionStatus.RequestCanceled;

                        // need to handle SSL errors too
#if !FEATURE_PAL
                        else if (NetworkStream is TlsStream)  {
                            errorStatus = ((TlsStream)NetworkStream).ExceptionStatus;
                        }
#endif // !FEATURE_PAL
                        else
                        {
                            SocketException socketException = exception.InnerException as SocketException;
                            if (socketException != null)
                            {
                                 if (socketException.ErrorCode == (int) SocketError.TimedOut)
                                    errorStatus = WebExceptionStatus.Timeout;
                                else
                                    errorStatus = WebExceptionStatus.ReceiveFailure;
                            }
                        }

                        GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SyncRead() Read() threw errorStatus:" + errorStatus.ToString() + " bytesRead:" + bytesRead.ToString());
                    }

                    if (pollSuccess)
                        requestDone = ReadComplete(bytesRead, errorStatus);

                    bytesRead = -1;
                } while (!requestDone && (userRetrievedStream || requestContinueCount == request.RequestContinueCount));
            }
            finally {
                t_SyncReadNesting--;
            }

            if (probeRead)
            {
                // Sync 100-Continue wait only
                request.FinishContinueWait();
                if (pollSuccess)
                {
                    if (!request.Saw100Continue && !userRetrievedStream)
                    {
                        //During polling, we got a response that wasn't a 100 continue.
                        request.NeedsToReadForResponse = false;
                    }
                }
                else
                {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::SyncRead() Poll has timed out, calling SetRequestContinue().");
                    request.SetRequestContinue();
                }
            }
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::SyncRead()");
        }


        //
        //    Performs read callback processing on connection
        //    handles getting headers parsed and streams created
        //
        //    bytesRead == 0  when  we re-enter on buffered data without doing actual read
        //    bytesRead == -1 when  we got a connection close plus when errorStatus == sucess we got a g----ful close.
        //    Otheriwse bytesRead is read byted to add to m_BytesRead i.e. to previously buffered data
        //
        private bool ReadComplete(int bytesRead, WebExceptionStatus errorStatus)
        {
            bool requestDone = true;
            CoreResponseData continueResponseData = null;
            ConnectionReturnResult returnResult = null;
            HttpWebRequest currentRequest = null;

            try
            {
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() m_BytesRead:" + m_BytesRead.ToString() + " m_BytesScanned:" + m_BytesScanned + " (+= new bytesRead:" + bytesRead.ToString() + ")");
                
                if (bytesRead < 0)
                {
                    // Means we might have gotten g----full or hard connection close.

                    // If this is the first thing we read for a request then it
                    // could be an idle connection closed by the server (isolated error)
                    if (m_ReadState == ReadState.Start && m_AtLeastOneResponseReceived)
                    {
                        // Note that KeepAliveFailure will be checked against POST-type requests
                        // and it's fatal if the body was already started.
                        if (errorStatus == WebExceptionStatus.Success || errorStatus == WebExceptionStatus.ReceiveFailure)
                            errorStatus = WebExceptionStatus.KeepAliveFailure;
                    }
                    else if (errorStatus == WebExceptionStatus.Success)
                    {
                        // we got unexpected FIN in the middle of the response, or on a fresh connection, that's fatal
                        errorStatus = WebExceptionStatus.ConnectionClosed;
                    }

                    // Notify request's SubmitWriteStream that a socket error happened.  This will cause future writes to
                    // throw an IOException.
                    HttpWebRequest curRequest = m_CurrentRequest;
                    if (curRequest != null)
                    {
                        curRequest.ErrorStatusCodeNotify(this, false, true);
                    }

                    HandleErrorWithReadDone(errorStatus, ref returnResult);
                    goto done;
                }

                // Otherwise, we've got data.
                GlobalLog.Dump(m_ReadBuffer, m_BytesScanned, m_BytesRead - m_BytesScanned);
                GlobalLog.Dump(m_ReadBuffer, m_BytesRead, bytesRead);


                bytesRead += m_BytesRead;
                if (bytesRead  > m_ReadBuffer.Length)
                    throw new InternalException();  //in case we posted two receives at once
                m_BytesRead = bytesRead;

                // We have the parsing code seperated out in ParseResponseData
                //
                // If we don't have all the headers yet. Resubmit the receive,
                // passing in the bytes read total as our index. When we get
                // back here we'll end up reparsing from the beginning, which is
                // OK. because this shouldn't be a performance case.

                //if we're back here, we need to reset the scanned bytes to 0.

                DataParseStatus parseStatus = ParseResponseData(ref returnResult, out requestDone, out continueResponseData);

                // copy off m_CurrentRequest as we might start processing a next request before exiting this method
                currentRequest = m_CurrentRequest;

                if (parseStatus != DataParseStatus.NeedMoreData)
                    m_CurrentRequest = null;

                if (parseStatus == DataParseStatus.Invalid || parseStatus == DataParseStatus.DataTooBig)
                {
                    // Tell the request's SubmitWriteStream that the connection will be closed.  It should ---- any
                    // future writes so that the appropriate exception will be received in GetResponse().
                    if (currentRequest != null)
                    {
                        currentRequest.ErrorStatusCodeNotify(this, false, false);
                    }

                    //
                    // report error
                    //
                    if (parseStatus == DataParseStatus.Invalid) {
                        HandleErrorWithReadDone(WebExceptionStatus.ServerProtocolViolation, ref returnResult);
                    }
                    else {
                        HandleErrorWithReadDone(WebExceptionStatus.MessageLengthLimitExceeded, ref returnResult);
                    }
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() parseStatus:" + parseStatus + " returnResult:" + returnResult);
                    goto done;
                }

                //Done means the ConnectStream take care of this connection until ConnectStream.CallDone()
                else if (parseStatus == DataParseStatus.Done)
                {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() [The response stream is ready] parseStatus = DataParseStatus.Done");
                    goto done;
                }

                //
                // we may reach the end of our buffer only when parsing headers.
                // this can happen when the header section is bigger than our initial 4k guess
                // which should be a good assumption in 99.9% of the cases. what we do here is:
                // 1) if there's a single BIG header (bigger than the current size) we will need to
                //    grow the buffer before we move data over and read more data.
                // 2) move unparsed data to the beginning of the buffer and read more data in the
                //    remaining part of the data.
                //
                if (parseStatus == DataParseStatus.NeedMoreData)
                {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() OLD buffer. m_ReadBuffer.Length:" + m_ReadBuffer.Length.ToString() + " m_BytesRead:" + m_BytesRead.ToString() + " m_BytesScanned:" + m_BytesScanned.ToString());
                    int unparsedDataSize = m_BytesRead - m_BytesScanned;
                    if (unparsedDataSize != 0)
                    {
                        if (m_BytesScanned == 0 && m_BytesRead == m_ReadBuffer.Length)
                        {
                            //
                            // 1) we need to grow the buffer, move the unparsed data to the beginning of the buffer before reading more data.
                            // since the buffer size is 4k, should we just double
                            //
                            byte[] newReadBuffer = new byte[m_ReadBuffer.Length * 2 /*+ ReadBufferSize*/];
                            Buffer.BlockCopy(m_ReadBuffer, 0, newReadBuffer, 0, m_BytesRead);

                            // if m_ReadBuffer is from the pinnable cache, give it back
                            FreeReadBuffer();
                            m_ReadBuffer = newReadBuffer;
                        }
                        else
                        {
                            //
                            // just move data around in the same buffer.
                            //
                            Buffer.BlockCopy(m_ReadBuffer, m_BytesScanned, m_ReadBuffer, 0, unparsedDataSize);
                        }
                    }
                    //
                    // update indexes and offsets in the new buffer
                    //
                    m_BytesRead = unparsedDataSize;
                    m_BytesScanned = 0;
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() NEW or shifted buffer. m_ReadBuffer.Length:" + m_ReadBuffer.Length.ToString() + " m_BytesRead:" + m_BytesRead.ToString() + " m_BytesScanned:" + m_BytesScanned.ToString());

                    if (currentRequest != null)
                    {
                        //
                        // This case means that we still parsing the headers, so need to post another read in the async case

                        if (currentRequest.Async)
                        {
                            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() Reposting Async Read.  Buffer:" + ValidationHelper.HashString(m_ReadBuffer) + " BytesScanned:" + m_BytesScanned.ToString());

                            if (Thread.CurrentThread.IsThreadPoolThread)
                            {
                                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() Calling PostReceive().");
                                PostReceive();
                            }
                            else
                            {
                                // Offload to the threadpool to protect against the case where one request's thread posts IO that another request
                                // depends on, but the first thread dies in the mean time.
                                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::ReadComplete() ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this)");
                                ThreadPool.UnsafeQueueUserWorkItem(m_PostReceiveDelegate, this);
                            }
                        }
                    }
                }
            }
            //
            // Any exception is processed by HandleError() and ----ed to avoid throwing on a thread pool
            // In the sync case the HandleError() will abort the request so the caller will pick up the result.
            //
            catch (Exception exception) {
                if (NclUtilities.IsFatal(exception)) throw;

                requestDone = true;

                if (m_InnerException == null)
                    m_InnerException = exception;

                // Notify request's SubmitWriteStream that a socket error happened.  This will cause future writes to
                // throw an IOException.
                HttpWebRequest curRequest = m_CurrentRequest;
                if (curRequest != null)
                {
                    curRequest.ErrorStatusCodeNotify(this, false, true);
                }

                HandleErrorWithReadDone(WebExceptionStatus.ReceiveFailure, ref returnResult);
            }

done:
            try {
                // It is only safe to continue if there was a 100 continue OR buffering is supported.
                if (currentRequest != null && currentRequest.HttpWriteMode != HttpWriteMode.None && 
                    (continueResponseData != null
                        // not a 100 continue, but we have buffering so we don't care what it was.
                        || (returnResult != null && returnResult.IsNotEmpty && currentRequest.AllowWriteStreamBuffering)
                    )
                   ) 
                {
                    // if returnResult is not empty it must also contain some result for the currently active request
                    // Since it could be a POST request waiting on the body submit, signal the body here
                    if (currentRequest.FinishContinueWait())
                    {
                        currentRequest.SetRequestContinue(continueResponseData);
                    }
                }
            }
            finally {
                ConnectionReturnResult.SetResponses(returnResult);
            }            

            return requestDone;
        }


        //     This method is called by ConnectStream, only when resubmitting
        //     We have sent the headers already in HttpWebRequest.EndSubmitRequest()
        //     which calls ConnectStream.WriteHeaders() which calls to HttpWebRequest.EndWriteHeaders()
        //     which calls ConnectStream.ResubmitWrite() which calls in here
        internal void Write(ScatterGatherBuffers writeBuffer) {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::Write(ScatterGatherBuffers) networkstream#" + ValidationHelper.HashString(NetworkStream));
            //
            // parameter validation
            //
            GlobalLog.Assert(writeBuffer != null, "Connection#{0}::Write(ScatterGatherBuffers)|writeBuffer == null", ValidationHelper.HashString(this));
            //
            // set up array for MultipleWrite call
            // note that GetBuffers returns null if we never wrote to it.
            //
            BufferOffsetSize[] buffers = writeBuffer.GetBuffers();
            if (buffers!=null) {
                //
                // This will block writing the buffers out.
                //
                MultipleWrite(buffers);
            }
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::Write(ScatterGatherBuffers) this:" + ValidationHelper.HashString(this) + " writeBuffer.Size:" + writeBuffer.Length.ToString());
        }


        /*++

            PostReceiveWrapper - Post a receive from a worker thread.

            This is our delegate, used for posting receives from a worker thread.
            We do this when we can't be sure that we're already on a worker thread,
            and we don't want to post from a client thread because if it goes away
            I/O gets cancelled.

            Input:

            state           - a null object

            Returns:

        --*/
        private static void PostReceiveWrapper(object state) {
            Connection thisConnection = state as Connection;
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(thisConnection) + "::PostReceiveWrapper", "Cnt#" + ValidationHelper.HashString(thisConnection));
            GlobalLog.Assert(thisConnection != null, "Connection#{0}::PostReceiveWrapper()|thisConnection == null", ValidationHelper.HashString(thisConnection));

            thisConnection.PostReceive();

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(thisConnection) + "::PostReceiveWrapper");
        }

        private void PostReceive()
        {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::PostReceive", "");

            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::PostReceive() m_ReadBuffer:" + ValidationHelper.HashString(m_ReadBuffer) + " Length:" + m_ReadBuffer.Length.ToString());

            try
            {
                GlobalLog.Assert(m_BytesScanned == 0, "PostReceive()|A receive should not be posted when m_BytesScanned != 0 (the data should be moved to offset 0).");

                if (m_LastAsyncResult != null && !m_LastAsyncResult.IsCompleted)
                    throw new InternalException();  //This may cause duplicate requests if we let it through in retail


                m_LastAsyncResult = UnsafeBeginRead(m_ReadBuffer, m_BytesRead, m_ReadBuffer.Length - m_BytesRead, m_ReadCallback, this);
                if (m_LastAsyncResult.CompletedSynchronously)
                {
                    // 
                    ReadCallback(m_LastAsyncResult);
                }
            }
            catch (Exception exception) {
                // Notify request's SubmitWriteStream that a socket error happened.  This will cause future writes to
                // throw an IOException.
                HttpWebRequest curRequest = m_CurrentRequest;
                if (curRequest != null)
                {
                    curRequest.ErrorStatusCodeNotify(this, false, true);
                }

                //ASYNCISSUE
                ConnectionReturnResult returnResult = null;
                HandleErrorWithReadDone(WebExceptionStatus.ReceiveFailure, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
                GlobalLog.LeaveException("Connection#" + ValidationHelper.HashString(this) + "::PostReceive", exception);
            }

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::PostReceive");
        }



        private static void TunnelThroughProxyWrapper(IAsyncResult result){
            if(result.CompletedSynchronously){
                return;
            }

            bool success = false;
            WebExceptionStatus ws = WebExceptionStatus.ConnectFailure;
            HttpWebRequest req = (HttpWebRequest)((LazyAsyncResult)result).AsyncObject;
            Connection conn = (Connection)((TunnelStateObject)result.AsyncState).Connection;
            HttpWebRequest originalReq = (HttpWebRequest)((TunnelStateObject)result.AsyncState).OriginalRequest;

            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(conn) + "::TunnelThroughProxyCallback");

            try{
                req.EndGetResponse(result);
                HttpWebResponse connectResponse = (HttpWebResponse)req.GetResponse();
                ConnectStream connectStream = (ConnectStream)connectResponse.GetResponseStream();

                // this stream will be used as the real stream for TlsStream
                conn.NetworkStream = new NetworkStream(connectStream.Connection.NetworkStream, true);
                // This will orphan the original connect stream now owned by tunnelStream
                connectStream.Connection.NetworkStream.ConvertToNotSocketOwner();
                success = true;
            }

            catch (Exception exception) {
                if (conn.m_InnerException == null)
                    conn.m_InnerException = exception;

                if (exception is WebException) {
                    ws = ((WebException)exception).Status;
                }

                GlobalLog.Print("Connection#" + ValidationHelper.HashString(conn) + "::TunnelThroughProxyCallback() exception occurred: " + exception);
            }
            if(!success)
            {
                ConnectionReturnResult returnResult = null;
                conn.HandleError(false, false, ws, ref returnResult);
                ConnectionReturnResult.SetResponses(returnResult);
                return;
            }

            conn.CompleteConnection(true, originalReq);
            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(conn) + "::TunnelThroughProxyCallback");
        }




        // 
        private bool TunnelThroughProxy(Uri proxy, HttpWebRequest originalRequest, bool async) {
            GlobalLog.Enter("Connection#" + ValidationHelper.HashString(this) + "::TunnelThroughProxy", "proxy="+proxy+", async="+async+", originalRequest #"+ValidationHelper.HashString(originalRequest));

            bool result = false;
            HttpWebRequest connectRequest = null;
            HttpWebResponse connectResponse = null;

            try {
                (new WebPermission(NetworkAccess.Connect, proxy)).Assert();
                try {
                    connectRequest = new HttpWebRequest(
                        proxy,
                        originalRequest.Address,
                       // new Uri("https://" + originalRequest.Address.GetParts(UriComponents.HostAndPort, UriFormat.UriEscaped)),
                        originalRequest
                        );
                }
                finally {
                    WebPermission.RevertAssert();
                }

                connectRequest.Credentials = originalRequest.InternalProxy == null ? null : originalRequest.InternalProxy.Credentials;
                connectRequest.InternalProxy = null;
                connectRequest.PreAuthenticate = true;

                if(async){
                    TunnelStateObject o = new TunnelStateObject(originalRequest, this);
                    IAsyncResult asyncResult = connectRequest.BeginGetResponse(m_TunnelCallback, o);
                    if(!asyncResult.CompletedSynchronously){
                        GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::TunnelThroughProxy completed asynchronously", true);
                        return true;
                    }
                    connectResponse = (HttpWebResponse)connectRequest.EndGetResponse(asyncResult);
                }
                else{
                    connectResponse = (HttpWebResponse)connectRequest.GetResponse();
                }

                ConnectStream connectStream = (ConnectStream)connectResponse.GetResponseStream();

                // this stream will be used as the real stream for TlsStream
                NetworkStream = new NetworkStream(connectStream.Connection.NetworkStream, true);
                // This will orphan the original connect stream now owned by tunnelStream
                connectStream.Connection.NetworkStream.ConvertToNotSocketOwner();
                result = true;
            }
            catch (Exception exception) {
                if (m_InnerException == null)
                    m_InnerException = exception;
                GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::TunnelThroughProxy() exception occurred: " + exception);
            }

            GlobalLog.Leave("Connection#" + ValidationHelper.HashString(this) + "::TunnelThroughProxy", result);

            return result;
        }

        //
        // CheckNonIdle - called after update of the WriteList/WaitList,
        //   upon the departure of our Idle state our, BusyCount will
        //   go to non-0, then we need to mark this transition
        //

        private void CheckNonIdle() {
            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckNonIdle()");
            if (m_Idle && this.BusyCount != 0) {
                m_Idle = false;
                ServicePoint.IncrementConnection();
                ConnectionGroup.IncrementConnection();
            }
        }

        //
        // CheckIdle - called after update of the WriteList/WaitList,
        //    specifically called after we remove entries
        //

        private void CheckIdle() {
            // The timer thread is allowed to call this.  (It doesn't call user code and doesn't block.)
            GlobalLog.ThreadContract(ThreadKinds.Unknown, ThreadKinds.SafeSources | ThreadKinds.Finalization | ThreadKinds.Timer, "Connection#" + ValidationHelper.HashString(this) + "::CheckIdle");

            GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckIdle() m_Idle = " + m_Idle + ", BusyCount = " + BusyCount);
            if (!m_Idle && this.BusyCount == 0)  {
                m_Idle = true;
                ServicePoint.DecrementConnection();
                if (ConnectionGroup != null) {
                    GlobalLog.Print("Connection#" + ValidationHelper.HashString(this) + "::CheckIdle() - calling ConnectionGoneIdle()");
                    ConnectionGroup.DecrementConnection();
                    ConnectionGroup.ConnectionGoneIdle();
                }
                // Remember the moment when this connection went idle.
                m_IdleSinceUtc = DateTime.UtcNow;
            }
        }

        //
        // DebugDumpArrayListEntries - debug goop
        //
        [Conditional("TRAVE")]
        private void DebugDumpWriteListEntries() {
            for (int i = 0; i < m_WriteList.Count; i++)
            {
                DebugDumpListEntry(i, m_WriteList[i] as HttpWebRequest, "WriteList");
            }
        }

        [Conditional("TRAVE")]
        private void DebugDumpWaitListEntries() {
            for (int i = 0; i < m_WaitList.Count; i++)
            {
                DebugDumpListEntry(i, m_WaitList[i].Request, "WaitList");
            }
        }

        [Conditional("TRAVE")]
        private void DebugDumpListEntry(int currentPos, HttpWebRequest req, string listType) {
            GlobalLog.Print("WaitList[" + currentPos.ToString() + "] Req: #" +
                ValidationHelper.HashString(req));
        }

        //
        // Validation & debugging
        //
        [System.Diagnostics.Conditional("DEBUG")]
        internal void DebugMembers(int requestHash) {
#if TRAVE
            bool dump = requestHash==0;
            GlobalLog.Print("Cnt#" + this.GetHashCode());
            if (!dump) {
                foreach(HttpWebRequest request in  m_WriteList) {
                    if (request.GetHashCode() == requestHash) {
                        GlobalLog.Print("Found-WriteList");
                        Dump();
                        return;
                    }
                }
                foreach(WaitListItem item in m_WaitList) {
                    if (item.Request.GetHashCode() == requestHash) {
                        GlobalLog.Print("Found-WaitList");
                        Dump();
                        return;
                    }
                }
            }
            else {
                Dump();
                DebugDumpWriteListEntries();
                DebugDumpWaitListEntries();
            }
#endif
        }

#if TRAVE
        [System.Diagnostics.Conditional("TRAVE")]
        internal void Dump() {
            GlobalLog.Print("CanPipeline:" + m_CanPipeline);
            GlobalLog.Print("Pipelining:" + m_Pipelining);
            GlobalLog.Print("KeepAlive:" + m_KeepAlive);
            GlobalLog.Print("m_Error:" + m_Error);
            GlobalLog.Print("m_ReadBuffer:" + m_ReadBuffer);
            GlobalLog.Print("m_BytesRead:" + m_BytesRead);
            GlobalLog.Print("m_BytesScanned:" + m_BytesScanned);
            GlobalLog.Print("m_ResponseData:" + m_ResponseData);
            GlobalLog.Print("m_ReadState:" + m_ReadState);
            GlobalLog.Print("m_StatusState:" + m_StatusState);
            GlobalLog.Print("ConnectionGroup:" + ConnectionGroup);
            GlobalLog.Print("Idle:" + m_Idle);
            GlobalLog.Print("ServicePoint:" + ServicePoint);
            GlobalLog.Print("m_Version:" + ServicePoint.ProtocolVersion);
            GlobalLog.Print("NetworkStream:" + NetworkStream);
#if !FEATURE_PAL
            if ( NetworkStream is TlsStream) {
                TlsStream tlsStream = NetworkStream as TlsStream;
                tlsStream.DebugMembers();
            }
            else
#endif // !FEATURE_PAL
            if (NetworkStream != null) {
                NetworkStream.DebugMembers();
            }
            GlobalLog.Print("m_AbortDelegate:" + m_AbortDelegate);
            GlobalLog.Print("ReadDone:" + m_ReadDone);
            GlobalLog.Print("WriteDone:" + m_WriteDone);
            GlobalLog.Print("Free:" + m_Free);
        }
#endif
    }
}
