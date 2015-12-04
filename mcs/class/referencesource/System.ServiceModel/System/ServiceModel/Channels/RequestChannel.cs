//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    abstract class RequestChannel : ChannelBase, IRequestChannel
    {
        bool manualAddressing;
        List<IRequestBase> outstandingRequests = new List<IRequestBase>();
        EndpointAddress to;
        Uri via;
        ManualResetEvent closedEvent;
        bool closed;

        protected RequestChannel(ChannelManagerBase channelFactory, EndpointAddress to, Uri via, bool manualAddressing)
            : base(channelFactory)
        {
            if (!manualAddressing)
            {
                if (to == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("to");
                }
            }

            this.manualAddressing = manualAddressing;
            this.to = to;
            this.via = via;
        }

        protected bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.to;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }

        protected void AbortPendingRequests()
        {
            IRequestBase[] requestsToAbort = CopyPendingRequests(false);

            if (requestsToAbort != null)
            {
                foreach (IRequestBase request in requestsToAbort)
                {
                    request.Abort(this);
                }
            }
        }

        protected IAsyncResult BeginWaitForPendingRequests(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IRequestBase[] pendingRequests = SetupWaitForPendingRequests();
            return new WaitForPendingRequestsAsyncResult(timeout, this, pendingRequests, callback, state);
        }

        protected void EndWaitForPendingRequests(IAsyncResult result)
        {
            WaitForPendingRequestsAsyncResult.End(result);
        }

        void FinishClose()
        {
            lock (outstandingRequests)
            {
                if (!closed)
                {
                    closed = true;
                    if (closedEvent != null)
                    {
                        this.closedEvent.Close();
                    }
                }
            }
        }

        IRequestBase[] SetupWaitForPendingRequests()
        {
            return this.CopyPendingRequests(true);
        }

        protected void WaitForPendingRequests(TimeSpan timeout)
        {
            IRequestBase[] pendingRequests = SetupWaitForPendingRequests();
            if (pendingRequests != null)
            {
                if (!closedEvent.WaitOne(timeout, false))
                {
                    foreach (IRequestBase request in pendingRequests)
                    {
                        request.Abort(this);
                    }
                }
            }
            FinishClose();
        }

        IRequestBase[] CopyPendingRequests(bool createEventIfNecessary)
        {
            IRequestBase[] requests = null;

            lock (outstandingRequests)
            {
                if (outstandingRequests.Count > 0)
                {
                    requests = new IRequestBase[outstandingRequests.Count];
                    outstandingRequests.CopyTo(requests);
                    outstandingRequests.Clear();

                    if (createEventIfNecessary && closedEvent == null)
                    {
                        closedEvent = new ManualResetEvent(false);
                    }
                }
            }

            return requests;
        }

        protected void FaultPendingRequests()
        {
            IRequestBase[] requestsToFault = CopyPendingRequests(false);

            if (requestsToFault != null)
            {
                foreach (IRequestBase request in requestsToFault)
                {
                    request.Fault(this);
                }
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IRequestChannel))
            {
                return (T)(object)this;
            }

            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return default(T);
        }

        protected override void OnAbort()
        {
            AbortPendingRequests();
        }

        void ReleaseRequest(IRequestBase request)
        {
            if (request != null)
            {
                // Synchronization of OnReleaseRequest is the 
                // responsibility of the concrete implementation of request.
                request.OnReleaseRequest();
            }

            lock (outstandingRequests)
            {
                // Remove supports the connection having been removed, so don't need extra Contains() check,
                // even though this may have been removed by Abort()
                outstandingRequests.Remove(request);
                if (outstandingRequests.Count == 0)
                {
                    if (!closed && closedEvent != null)
                    {
                        closedEvent.Set();
                    }
                }
            }
        }

        void TrackRequest(IRequestBase request)
        {
            lock (outstandingRequests)
            {
                ThrowIfDisposedOrNotOpen(); // make sure that we haven't already snapshot our collection
                outstandingRequests.Add(request);
            }
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");

            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            ThrowIfDisposedOrNotOpen();

            AddHeadersTo(message);
            IAsyncRequest asyncRequest = CreateAsyncRequest(message, callback, state);
            TrackRequest(asyncRequest);

            bool throwing = true;
            try
            {
                asyncRequest.BeginSendRequest(message, timeout);
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    ReleaseRequest(asyncRequest);
                }
            }

            return asyncRequest;
        }

        protected abstract IRequest CreateRequest(Message message);
        protected abstract IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state);

        public Message EndRequest(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            IAsyncRequest asyncRequest = result as IAsyncRequest;

            if (asyncRequest == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.InvalidAsyncResult));
            }

            try
            {
                Message reply = asyncRequest.End();

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.RequestChannelReplyReceived,
                        SR.GetString(SR.TraceCodeRequestChannelReplyReceived), reply);
                }

                return reply;
            }
            finally
            {
                ReleaseRequest(asyncRequest);
            }
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (timeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRange0)));

            ThrowIfDisposedOrNotOpen();

            AddHeadersTo(message);
            IRequest request = CreateRequest(message);
            TrackRequest(request);
            try
            {
                Message reply;
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                TimeSpan savedTimeout = timeoutHelper.RemainingTime();
                try
                {
                    request.SendRequest(message, savedTimeout);
                }
                catch (TimeoutException timeoutException)
                {
                    throw TraceUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.RequestChannelSendTimedOut, savedTimeout),
                        timeoutException), message);
                }

                savedTimeout = timeoutHelper.RemainingTime();

                try
                {
                    reply = request.WaitForReply(savedTimeout);
                }
                catch (TimeoutException timeoutException)
                {
                    throw TraceUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.RequestChannelWaitForReplyTimedOut, savedTimeout),
                        timeoutException), message);
                }


                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.RequestChannelReplyReceived,
                        SR.GetString(SR.TraceCodeRequestChannelReplyReceived), reply);
                }

                return reply;
            }
            finally
            {
                ReleaseRequest(request);
            }
        }

        protected virtual void AddHeadersTo(Message message)
        {
            if (!manualAddressing && to != null)
            {
                to.ApplyTo(message);
            }
        }

        class WaitForPendingRequestsAsyncResult : AsyncResult
        {
            static WaitOrTimerCallback completeWaitCallBack = new WaitOrTimerCallback(OnCompleteWaitCallBack);
            IRequestBase[] pendingRequests;
            RequestChannel requestChannel;
            TimeSpan timeout;
            RegisteredWaitHandle waitHandle;

            public WaitForPendingRequestsAsyncResult(TimeSpan timeout, RequestChannel requestChannel, IRequestBase[] pendingRequests, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.requestChannel = requestChannel;
                this.pendingRequests = pendingRequests;
                this.timeout = timeout;

                if (this.timeout == TimeSpan.Zero || this.pendingRequests == null)
                {
                    AbortRequests();
                    CleanupEvents();
                    Complete(true);
                }
                else
                {
                    this.waitHandle = ThreadPool.RegisterWaitForSingleObject(this.requestChannel.closedEvent, completeWaitCallBack, this, TimeoutHelper.ToMilliseconds(timeout), true);
                }
            }

            void AbortRequests()
            {
                if (pendingRequests != null)
                {
                    foreach (IRequestBase request in pendingRequests)
                    {
                        request.Abort(this.requestChannel);
                    }
                }
            }

            void CleanupEvents()
            {
                if (requestChannel.closedEvent != null)
                {
                    if (waitHandle != null)
                    {
                        waitHandle.Unregister(requestChannel.closedEvent);
                    }
                    requestChannel.FinishClose();
                }
            }

            static void OnCompleteWaitCallBack(object state, bool timedOut)
            {
                WaitForPendingRequestsAsyncResult thisPtr = (WaitForPendingRequestsAsyncResult)state;
                Exception completionException = null;
                try
                {
                    if (timedOut)
                    {
                        thisPtr.AbortRequests();
                    }
                    thisPtr.CleanupEvents();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completionException = e;
                }

                thisPtr.Complete(false, completionException);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WaitForPendingRequestsAsyncResult>(result);
            }
        }
    }

    interface IRequestBase
    {
        void Abort(RequestChannel requestChannel);
        void Fault(RequestChannel requestChannel);
        void OnReleaseRequest();
    }

    interface IRequest : IRequestBase
    {
        void SendRequest(Message message, TimeSpan timeout);
        Message WaitForReply(TimeSpan timeout);
    }

    interface IAsyncRequest : IAsyncResult, IRequestBase
    {
        void BeginSendRequest(Message message, TimeSpan timeout);
        Message End();
    }
}
