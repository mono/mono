//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Xml;
    using System.ServiceModel.Diagnostics.Application;

    class DuplexChannelBinder : IChannelBinder
    {
        IDuplexChannel channel;
        IRequestReplyCorrelator correlator;
        TimeSpan defaultCloseTimeout;
        TimeSpan defaultSendTimeout;
        IdentityVerifier identityVerifier;
        bool isSession;
        Uri listenUri;
        int pending;
        bool syncPumpEnabled;
        List<IDuplexRequest> requests;
        List<ICorrelatorKey> timedOutRequests;
        ChannelHandler channelHandler;
        volatile bool requestAborted;

        internal DuplexChannelBinder(IDuplexChannel channel, IRequestReplyCorrelator correlator)
        {
            Fx.Assert(channel != null, "caller must verify");
            Fx.Assert(correlator != null, "caller must verify");
            this.channel = channel;
            this.correlator = correlator;
            this.channel.Faulted += new EventHandler(OnFaulted);
        }

        internal DuplexChannelBinder(IDuplexChannel channel, IRequestReplyCorrelator correlator, Uri listenUri)
            : this(channel, correlator)
        {
            this.listenUri = listenUri;
        }

        internal DuplexChannelBinder(IDuplexSessionChannel channel, IRequestReplyCorrelator correlator, Uri listenUri)
            : this((IDuplexChannel)channel, correlator, listenUri)
        {
            this.isSession = true;
        }

        internal DuplexChannelBinder(IDuplexSessionChannel channel, IRequestReplyCorrelator correlator, bool useActiveAutoClose)
            : this(useActiveAutoClose ? new AutoCloseDuplexSessionChannel(channel) : channel, correlator, null)
        {
        }

        public IChannel Channel
        {
            get { return this.channel; }
        }

        public TimeSpan DefaultCloseTimeout
        {
            get { return this.defaultCloseTimeout; }
            set { this.defaultCloseTimeout = value; }
        }

        internal ChannelHandler ChannelHandler
        {
            get
            {
                if (!(this.channelHandler != null))
                {
                    Fx.Assert("DuplexChannelBinder.ChannelHandler: (channelHandler != null)");
                }
                return this.channelHandler;
            }
            set
            {
                if (!(this.channelHandler == null))
                {
                    Fx.Assert("DuplexChannelBinder.ChannelHandler: (channelHandler == null)");
                }
                this.channelHandler = value;
            }
        }

        public TimeSpan DefaultSendTimeout
        {
            get { return this.defaultSendTimeout; }
            set { this.defaultSendTimeout = value; }
        }

        public bool HasSession
        {
            get { return this.isSession; }
        }

        internal IdentityVerifier IdentityVerifier
        {
            get
            {
                if (this.identityVerifier == null)
                {
                    this.identityVerifier = IdentityVerifier.CreateDefault();
                }

                return this.identityVerifier;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.identityVerifier = value;
            }
        }

        public Uri ListenUri
        {
            get { return this.listenUri; }
        }

        public EndpointAddress LocalAddress
        {
            get { return this.channel.LocalAddress; }
        }

        bool Pumping
        {
            get
            {
                if (this.syncPumpEnabled)
                    return true;

                if (this.ChannelHandler != null && this.ChannelHandler.HasRegisterBeenCalled)
                    return true;

                return false;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.channel.RemoteAddress; }
        }

        List<IDuplexRequest> Requests
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.requests == null)
                        this.requests = new List<IDuplexRequest>();
                    return this.requests;
                }
            }
        }

        List<ICorrelatorKey> TimedOutRequests
        {
            get
            {
                lock (this.ThisLock)
                {
                    if (this.timedOutRequests == null)
                    {
                        this.timedOutRequests = new List<ICorrelatorKey>();
                    }
                    return this.timedOutRequests;
                }
            }
        }

        object ThisLock
        {
            get { return this; }
        }

        void OnFaulted(object sender, EventArgs e)
        {
            //Some unhandled exception happened on the channel. 
            //So close all pending requests so the callbacks (in case of async)
            //on the requests are called.
            this.AbortRequests();
        }

        public void Abort()
        {
            this.channel.Abort();
            this.AbortRequests();
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.channel.Close(timeout);
            this.AbortRequests();
        }

        void AbortRequests()
        {
            IDuplexRequest[] array = null;
            lock (this.ThisLock)
            {
                if (this.requests != null)
                {
                    array = this.requests.ToArray();

                    foreach (IDuplexRequest request in array)
                    {
                        request.Abort();
                    }
                }
                this.requests = null;
                this.requestAborted = true;
            }

            // Remove requests from the correlator since the channel might be either faulting or aborting,
            // We are not going to get a reply for these requests. If they are not removed from the correlator, this will cause a leak.
            // This operation does not have to be under the lock
            if (array != null && array.Length > 0)
            {
                RequestReplyCorrelator requestReplyCorrelator = this.correlator as RequestReplyCorrelator;
                if (requestReplyCorrelator != null)
                {
                    foreach (IDuplexRequest request in array)
                    {
                        ICorrelatorKey keyedRequest = request as ICorrelatorKey;
                        if (keyedRequest != null)
                        {
                            requestReplyCorrelator.RemoveRequest(keyedRequest);
                        }
                    }
                }
            }

            //if there are any timed out requests, delete it from the correlator table
            this.DeleteTimedoutRequestsFromCorrelator();
        }

        TimeoutException GetReceiveTimeoutException(TimeSpan timeout)
        {
            EndpointAddress address = this.channel.RemoteAddress ?? this.channel.LocalAddress;
            if (address != null)
            {
                return new TimeoutException(SR.GetString(SR.SFxRequestTimedOut2, address, timeout));
            }
            else
            {
                return new TimeoutException(SR.GetString(SR.SFxRequestTimedOut1, timeout));
            }
        }

        internal bool HandleRequestAsReply(Message message)
        {
            UniqueId relatesTo = null;
            try
            {
                relatesTo = message.Headers.RelatesTo;
            }
            catch (MessageHeaderException)
            {
                // ignore it
            }
            if (relatesTo == null)
            {
                return false;
            }
            else
            {
                return HandleRequestAsReplyCore(message);
            }
        }

        bool HandleRequestAsReplyCore(Message message)
        {
            IDuplexRequest request = correlator.Find<IDuplexRequest>(message, true);
            if (request != null)
            {
                request.GotReply(message);
                return true;
            }
            return false;
        }

        public void EnsurePumping()
        {
            lock (this.ThisLock)
            {
                if (!this.syncPumpEnabled)
                {
                    if (!this.ChannelHandler.HasRegisterBeenCalled)
                    {
                        ChannelHandler.Register(this.ChannelHandler);
                    }
                }
            }
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.channel.State == CommunicationState.Faulted)
                return new ChannelFaultedAsyncResult(callback, state);

            return this.channel.BeginTryReceive(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            ChannelFaultedAsyncResult channelFaultedResult = result as ChannelFaultedAsyncResult;
            if (channelFaultedResult != null)
            {
                this.AbortRequests();
                requestContext = null;
                return true;
            }

            Message message;
            if (this.channel.EndTryReceive(result, out message))
            {
                if (message != null)
                {
                    requestContext = new DuplexRequestContext(this.channel, message, this);
                }
                else
                {
                    this.AbortRequests();
                    requestContext = null;
                }
                return true;
            }
            else
            {
                requestContext = null;
                return false;
            }
        }

        public RequestContext CreateRequestContext(Message message)
        {
            return new DuplexRequestContext(this.channel, message, this);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.channel.EndSend(result);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.channel.Send(message, timeout);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool success = false;
            AsyncDuplexRequest duplexRequest = null;

            try
            {
                RequestReplyCorrelator.PrepareRequest(message);
                duplexRequest = new AsyncDuplexRequest(message, this, timeout, callback, state);

                lock (this.ThisLock)
                {
                    this.RequestStarting(message, duplexRequest);
                }

                IAsyncResult result = this.channel.BeginSend(message, timeout, Fx.ThunkCallback(new AsyncCallback(this.SendCallback)), duplexRequest);

                if (result.CompletedSynchronously)
                    duplexRequest.FinishedSend(result, true);

                EnsurePumping();

                success = true;
                return duplexRequest;
            }
            finally
            {
                lock (this.ThisLock)
                {
                    if (success)
                    {
                        duplexRequest.EnableCompletion();
                    }
                    else
                    {
                        this.RequestCompleting(duplexRequest);
                    }
                }
            }
        }

        public Message EndRequest(IAsyncResult result)
        {
            AsyncDuplexRequest duplexRequest = result as AsyncDuplexRequest;

            if (duplexRequest == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.InvalidAsyncResult)));

            return duplexRequest.End();
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            if (this.channel.State == CommunicationState.Faulted)
            {
                this.AbortRequests();
                requestContext = null;
                return true;
            }

            Message message;
            if (this.channel.TryReceive(timeout, out message))
            {
                if (message != null)
                {
                    requestContext = new DuplexRequestContext(this.channel, message, this);
                }
                else
                {
                    this.AbortRequests();
                    requestContext = null;
                }
                return true;
            }
            else
            {
                requestContext = null;
                return false;
            }
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            SyncDuplexRequest duplexRequest = null;
            bool optimized = false;

            RequestReplyCorrelator.PrepareRequest(message);

            lock (this.ThisLock)
            {
                if (!Pumping)
                {
                    optimized = true;
                    syncPumpEnabled = true;
                }

                if (!optimized)
                    duplexRequest = new SyncDuplexRequest(this);

                this.RequestStarting(message, duplexRequest);
            }

            if (optimized)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                UniqueId messageId = message.Headers.MessageId;

                try
                {
                    this.channel.Send(message, timeoutHelper.RemainingTime());
                    if (DiagnosticUtility.ShouldUseActivity &&
                        ServiceModelActivity.Current != null &&
                        ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction)
                    {
                        ServiceModelActivity.Current.Suspend();
                    }

                    for (;;)
                    {
                        TimeSpan remaining = timeoutHelper.RemainingTime();
                        Message reply;

                        if (!this.channel.TryReceive(remaining, out reply))
                        {
                            throw TraceUtility.ThrowHelperError(this.GetReceiveTimeoutException(timeout), message);
                        }

                        if (reply == null)
                        {
                            this.AbortRequests();
                            return null;
                        }

                        if (reply.Headers.RelatesTo == messageId)
                        {
                            this.ThrowIfInvalidReplyIdentity(reply);
                            return reply;
                        }
                        else if (!this.HandleRequestAsReply(reply))
                        {
                            // SFx drops a message here
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                EndpointDispatcher dispatcher = null;
                                if (this.ChannelHandler != null && this.ChannelHandler.Channel != null)
                                {
                                    dispatcher = this.ChannelHandler.Channel.EndpointDispatcher;
                                }
                                TraceUtility.TraceDroppedMessage(reply, dispatcher);
                            }
                            reply.Close();
                        }
                    }
                }
                finally
                {
                    lock (this.ThisLock)
                    {
                        this.RequestCompleting(null);
                        syncPumpEnabled = false;
                        if (this.pending > 0)
                            EnsurePumping();
                    }
                }
            }
            else
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                this.channel.Send(message, timeoutHelper.RemainingTime());
                EnsurePumping();
                return duplexRequest.WaitForReply(timeoutHelper.RemainingTime());
            }
        }

        // ASSUMPTION: (Microsoft) caller holds lock (this.mutex)
        void RequestStarting(Message message, IDuplexRequest request)
        {
            if (request != null)
            {
                this.Requests.Add(request);
                if (!this.requestAborted)
                {
                    this.correlator.Add<IDuplexRequest>(message, request);
                }
            }
            this.pending++;

        }

        // ASSUMPTION: (Microsoft) caller holds lock (this.mutex)
        void RequestCompleting(IDuplexRequest request)
        {
            this.pending--;
            if (this.pending == 0)
            {
                this.requests = null;
            }
            else if ((request != null) && (this.requests != null))
            {
                this.requests.Remove(request);
            }
        }

        // ASSUMPTION: caller holds ThisLock
        void AddToTimedOutRequestList(ICorrelatorKey request)
        {
            Fx.Assert(request != null, "request cannot be null");
            this.TimedOutRequests.Add(request);
        }

        // ASSUMPTION: caller holds  ThisLock
        void RemoveFromTimedOutRequestList(ICorrelatorKey request)
        {
            Fx.Assert(request != null, "request cannot be null");
            if (this.timedOutRequests != null)
            {
                this.timedOutRequests.Remove(request);
            }
        }

        void DeleteTimedoutRequestsFromCorrelator()
        {
            ICorrelatorKey[] array = null;
            if (this.timedOutRequests != null && this.timedOutRequests.Count > 0)
            {
                lock (this.ThisLock)
                {
                    if (this.timedOutRequests != null && this.timedOutRequests.Count > 0)
                    {
                        array = this.timedOutRequests.ToArray();
                        this.timedOutRequests = null;
                    }
                }
            }

            // Remove requests from the correlator since the channel might be either faulting, aborting or closing 
            // We are not going to get a reply for these timed out requests. If they are not removed from the correlator, this will cause a leak.
            // This operation does not have to be under the lock
            if (array != null && array.Length > 0)
            {
                RequestReplyCorrelator requestReplyCorrelator = this.correlator as RequestReplyCorrelator;
                if (requestReplyCorrelator != null)
                {
                    foreach (ICorrelatorKey request in array)
                    {
                        requestReplyCorrelator.RemoveRequest(request);
                    }
                }
            }

        }

        void SendCallback(IAsyncResult result)
        {
            AsyncDuplexRequest duplexRequest = result.AsyncState as AsyncDuplexRequest;
            if (!((duplexRequest != null)))
            {
                Fx.Assert("DuplexChannelBinder.RequestCallback: (duplexRequest != null)");
            }

            if (!result.CompletedSynchronously)
                duplexRequest.FinishedSend(result, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void EnsureIncomingIdentity(SecurityMessageProperty property, EndpointAddress address, Message reply)
        {
            this.IdentityVerifier.EnsureIncomingIdentity(address, property.ServiceSecurityContext.AuthorizationContext);
        }

        void ThrowIfInvalidReplyIdentity(Message reply)
        {
            if (!this.isSession)
            {
                SecurityMessageProperty property = reply.Properties.Security;
                EndpointAddress address = this.channel.RemoteAddress;

                if ((property != null) && (address != null))
                {
                    EnsureIncomingIdentity(property, address, reply);
                }
            }
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.channel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.channel.EndWaitForMessage(result);
        }

        class DuplexRequestContext : RequestContextBase
        {
            DuplexChannelBinder binder;
            IDuplexChannel channel;

            internal DuplexRequestContext(IDuplexChannel channel, Message request, DuplexChannelBinder binder)
                : base(request, binder.DefaultCloseTimeout, binder.DefaultSendTimeout)
            {
                this.channel = channel;
                this.binder = binder;
            }

            protected override void OnAbort()
            {
            }

            protected override void OnClose(TimeSpan timeout)
            {
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                if (message != null)
                {
                    this.channel.Send(message, timeout);
                }
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ReplyAsyncResult(this, message, timeout, callback, state);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                ReplyAsyncResult.End(result);
            }

            class ReplyAsyncResult : AsyncResult
            {
                static AsyncCallback onSend;
                DuplexRequestContext context;

                public ReplyAsyncResult(DuplexRequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    if (message != null)
                    {
                        if (onSend == null)
                        {

                            onSend = Fx.ThunkCallback(new AsyncCallback(OnSend));
                        }
                        this.context = context;
                        IAsyncResult result = context.channel.BeginSend(message, timeout, onSend, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        context.channel.EndSend(result);
                    }

                    base.Complete(true);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<ReplyAsyncResult>(result);
                }

                static void OnSend(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    Exception completionException = null;
                    ReplyAsyncResult thisPtr = (ReplyAsyncResult)result.AsyncState;
                    try
                    {
                        thisPtr.context.channel.EndSend(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completionException = exception;
                    }

                    thisPtr.Complete(false, completionException);
                }
            }
        }

        interface IDuplexRequest
        {
            void Abort();
            void GotReply(Message reply);
        }

        class SyncDuplexRequest : IDuplexRequest, ICorrelatorKey
        {
            Message reply;
            DuplexChannelBinder parent;
            ManualResetEvent wait = new ManualResetEvent(false);
            int waitCount = 0;
            RequestReplyCorrelator.Key requestCorrelatorKey;

            internal SyncDuplexRequest(DuplexChannelBinder parent)
            {
                this.parent = parent;
            }

            RequestReplyCorrelator.Key ICorrelatorKey.RequestCorrelatorKey
            {
                get
                {
                    return this.requestCorrelatorKey;
                }
                set
                {
                    Fx.Assert(this.requestCorrelatorKey == null, "RequestCorrelatorKey is already set for this request");
                    this.requestCorrelatorKey = value;
                }
            }

            public void Abort()
            {
                this.wait.Set();
            }

            internal Message WaitForReply(TimeSpan timeout)
            {
                try
                {
                    if (!TimeoutHelper.WaitOne(this.wait, timeout))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.parent.GetReceiveTimeoutException(timeout));
                    }
                }
                finally
                {
                    this.CloseWaitHandle();
                }

                this.parent.ThrowIfInvalidReplyIdentity(this.reply);
                return this.reply;
            }

            public void GotReply(Message reply)
            {
                lock (this.parent.ThisLock)
                {
                    this.parent.RequestCompleting(this);
                }
                this.reply = reply;
                this.wait.Set();
                this.CloseWaitHandle();
            }

            void CloseWaitHandle()
            {
                if (Interlocked.Increment(ref this.waitCount) == 2)
                {
                    this.wait.Close();
                }
            }
        }

        class AsyncDuplexRequest : AsyncResult, IDuplexRequest, ICorrelatorKey
        {
            static Action<object> timerCallback = new Action<object>(AsyncDuplexRequest.TimerCallback);

            bool aborted;
            bool enableComplete;
            bool gotReply;
            Exception sendException;
            IAsyncResult sendResult;
            DuplexChannelBinder parent;
            Message reply;
            bool timedOut;
            TimeSpan timeout;
            IOThreadTimer timer;
            ServiceModelActivity activity;
            RequestReplyCorrelator.Key requestCorrelatorKey;

            internal AsyncDuplexRequest(Message message, DuplexChannelBinder parent, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.timeout = timeout;

                if (timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(AsyncDuplexRequest.timerCallback, this, true);
                    this.timer.Set(timeout);
                }
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    this.activity = TraceUtility.ExtractActivity(message);
                }
            }

            bool IsDone
            {
                get
                {
                    if (!this.enableComplete)
                    {
                        return false;
                    }

                    return (((this.sendResult != null) && this.gotReply) ||
                            (this.sendException != null) ||
                            this.timedOut ||
                            this.aborted);
                }
            }

            RequestReplyCorrelator.Key ICorrelatorKey.RequestCorrelatorKey
            {
                get
                {
                    return this.requestCorrelatorKey;
                }
                set
                {
                    Fx.Assert(this.requestCorrelatorKey == null, "RequestCorrelatorKey is already set for this request");
                    this.requestCorrelatorKey = value;
                }
            }

            public void Abort()
            {
                bool done;

                lock (this.parent.ThisLock)
                {
                    bool wasDone = this.IsDone;
                    this.aborted = true;
                    done = !wasDone && this.IsDone;
                }

                if (done)
                    this.Done(false);
            }

            void Done(bool completedSynchronously)
            {
                // Make sure that we are acting on the Reply activity.
                ServiceModelActivity replyActivity = DiagnosticUtility.ShouldUseActivity ?
                    TraceUtility.ExtractActivity(this.reply) : null;
                using (ServiceModelActivity.BoundOperation(replyActivity))
                {
                    if (this.timer != null)
                    {
                        this.timer.Cancel();
                        this.timer = null;
                    }

                    lock (this.parent.ThisLock)
                    {
                        if (this.timedOut)
                        {
                            // this needs to be saved in a list since we need to remove these from the correlator table when the channel aborts or closes
                            this.parent.AddToTimedOutRequestList(this);
                        }
                        this.parent.RequestCompleting(this);
                    }

                    if (this.sendException != null)
                        this.Complete(completedSynchronously, this.sendException);
                    else if (this.timedOut)
                        this.Complete(completedSynchronously, this.parent.GetReceiveTimeoutException(this.timeout));
                    else
                        this.Complete(completedSynchronously);
                }
            }

            public void EnableCompletion()
            {
                bool done;

                lock (this.parent.ThisLock)
                {
                    bool wasDone = this.IsDone;
                    this.enableComplete = true;
                    done = !wasDone && this.IsDone;
                }

                if (done)
                    this.Done(true);
            }

            public void FinishedSend(IAsyncResult sendResult, bool completedSynchronously)
            {
                Exception sendException = null;

                try
                {
                    this.parent.channel.EndSend(sendResult);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    sendException = e;
                }

                bool done;

                lock (this.parent.ThisLock)
                {
                    bool wasDone = this.IsDone;
                    this.sendResult = sendResult;
                    this.sendException = sendException;
                    done = !wasDone && this.IsDone;
                }

                if (done)
                    this.Done(completedSynchronously);
            }

            internal Message End()
            {
                AsyncResult.End<AsyncDuplexRequest>(this);
                this.parent.ThrowIfInvalidReplyIdentity(this.reply);
                return this.reply;
            }

            public void GotReply(Message reply)
            {
                bool done;

                ServiceModelActivity replyActivity = DiagnosticUtility.ShouldUseActivity ?
                    TraceUtility.ExtractActivity(reply) : null;

                using (ServiceModelActivity.BoundOperation(replyActivity))
                {
                    lock (this.parent.ThisLock)
                    {
                        bool wasDone = this.IsDone;
                        this.reply = reply;
                        this.gotReply = true;
                        done = !wasDone && this.IsDone;
                        // we got reply on the channel after the request timed out, let's delete it from the pending timed out requests
                        // we don't neeed to hold on to it since it is now  removed from the correlator table
                        if (wasDone && this.timedOut)
                        {
                            this.parent.RemoveFromTimedOutRequestList(this);
                        }
                    }
                    if (replyActivity != null && DiagnosticUtility.ShouldUseActivity)
                    {
                        TraceUtility.SetActivity(reply, this.activity);
                        if (DiagnosticUtility.ShouldUseActivity && this.activity != null)
                        {
                            if (null != FxTrace.Trace)
                            {
                                FxTrace.Trace.TraceTransfer(this.activity.Id);
                            }
                        }
                    }
                }
                if (DiagnosticUtility.ShouldUseActivity && replyActivity != null)
                {
                    replyActivity.Stop();
                }

                if (done)
                    this.Done(false);
            }

            void TimedOut()
            {
                bool done;

                lock (this.parent.ThisLock)
                {
                    bool wasDone = this.IsDone;
                    this.timedOut = true;
                    done = !wasDone && this.IsDone;
                }

                if (done)
                    this.Done(false);
            }

            static void TimerCallback(object state)
            {
                ((AsyncDuplexRequest)state).TimedOut();
            }
        }

        class ChannelFaultedAsyncResult : CompletedAsyncResult
        {
            public ChannelFaultedAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }
        }

        // used to read-ahead by a single message and auto-close the session when we read null
        class AutoCloseDuplexSessionChannel : IDuplexSessionChannel
        {
            static AsyncCallback receiveAsyncCallback;
            static Action<object> receiveThreadSchedulerCallback;
            static AsyncCallback closeInnerChannelCallback;
            IDuplexSessionChannel innerChannel;
            InputQueue<Message> pendingMessages;
            Action messageDequeuedCallback;
            CloseState closeState;

            public AutoCloseDuplexSessionChannel(IDuplexSessionChannel innerChannel)
            {
                this.innerChannel = innerChannel;
                this.pendingMessages = new InputQueue<Message>();
                this.messageDequeuedCallback = new Action(StartBackgroundReceive); // kick off a new receive when a message is picked up
                this.closeState = new CloseState();
            }

            object ThisLock
            {
                get
                {
                    return this;
                }
            }

            public EndpointAddress LocalAddress
            {
                get { return this.innerChannel.LocalAddress; }
            }

            public EndpointAddress RemoteAddress
            {
                get { return this.innerChannel.RemoteAddress; }
            }

            public Uri Via
            {
                get { return this.innerChannel.Via; }
            }

            public IDuplexSession Session
            {
                get { return this.innerChannel.Session; }
            }

            public CommunicationState State
            {
                get { return this.innerChannel.State; }
            }

            public event EventHandler Closing
            {
                add { this.innerChannel.Closing += value; }
                remove { this.innerChannel.Closing -= value; }
            }

            public event EventHandler Closed
            {
                add { this.innerChannel.Closed += value; }
                remove { this.innerChannel.Closed -= value; }
            }

            public event EventHandler Faulted
            {
                add { this.innerChannel.Faulted += value; }
                remove { this.innerChannel.Faulted -= value; }
            }

            public event EventHandler Opened
            {
                add { this.innerChannel.Opened += value; }
                remove { this.innerChannel.Opened -= value; }
            }

            public event EventHandler Opening
            {
                add { this.innerChannel.Opening += value; }
                remove { this.innerChannel.Opening -= value; }
            }

            TimeSpan DefaultCloseTimeout
            {
                get
                {
                    IDefaultCommunicationTimeouts defaultTimeouts = this.innerChannel as IDefaultCommunicationTimeouts;

                    if (defaultTimeouts != null)
                    {
                        return defaultTimeouts.CloseTimeout;
                    }
                    else
                    {
                        return ServiceDefaults.CloseTimeout;
                    }
                }
            }

            TimeSpan DefaultReceiveTimeout
            {
                get
                {
                    IDefaultCommunicationTimeouts defaultTimeouts = this.innerChannel as IDefaultCommunicationTimeouts;

                    if (defaultTimeouts != null)
                    {
                        return defaultTimeouts.ReceiveTimeout;
                    }
                    else
                    {
                        return ServiceDefaults.ReceiveTimeout;
                    }
                }
            }

            // kick off an async receive so that we notice when the server is trying to shutdown
            void StartBackgroundReceive()
            {
                if (receiveAsyncCallback == null)
                {
                    receiveAsyncCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveAsyncCallback));
                }

                IAsyncResult result = null;
                Exception exceptionFromBeginReceive = null;
                try
                {
                    result = this.innerChannel.BeginReceive(TimeSpan.MaxValue, receiveAsyncCallback, this);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    exceptionFromBeginReceive = e;
                }

                if (exceptionFromBeginReceive != null)
                {
                    this.pendingMessages.EnqueueAndDispatch(exceptionFromBeginReceive, messageDequeuedCallback, false);
                }
                else if (result.CompletedSynchronously)
                {
                    if (receiveThreadSchedulerCallback == null)
                    {
                        receiveThreadSchedulerCallback = new Action<object>(ReceiveThreadSchedulerCallback);
                    }
                    IOThreadScheduler.ScheduleCallbackLowPriNoFlow(receiveThreadSchedulerCallback, result);
                }
            }

            static void ReceiveThreadSchedulerCallback(object state)
            {
                IAsyncResult result = (IAsyncResult)state;
                AutoCloseDuplexSessionChannel thisPtr = (AutoCloseDuplexSessionChannel)result.AsyncState;
                thisPtr.OnReceive(result);
            }

            static void ReceiveAsyncCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                AutoCloseDuplexSessionChannel thisPtr = (AutoCloseDuplexSessionChannel)result.AsyncState;
                thisPtr.OnReceive(result);
            }

            void OnReceive(IAsyncResult result)
            {
                Message message = null;
                Exception receiveException = null;
                try
                {
                    message = this.innerChannel.EndReceive(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    receiveException = e;
                }

                if (receiveException != null)
                {
                    this.pendingMessages.EnqueueAndDispatch(receiveException, this.messageDequeuedCallback, true);
                }
                else
                {
                    if (message == null)
                    {
                        // we've hit end of session, time for auto-close to kick in
                        this.pendingMessages.Shutdown();
                        this.CloseInnerChannel();
                    }
                    else
                    {
                        this.pendingMessages.EnqueueAndDispatch(message, this.messageDequeuedCallback, true);
                    }
                }
            }

            void CloseInnerChannel()
            {
                lock (ThisLock)
                {
                    if (!this.closeState.TryBackgroundClose() || this.State != CommunicationState.Opened)
                    {
                        return;
                    }
                }

                IAsyncResult result = null;
                Exception backgroundCloseException = null;
                try
                {
                    if (closeInnerChannelCallback == null)
                    {
                        closeInnerChannelCallback = Fx.ThunkCallback(new AsyncCallback(CloseInnerChannelCallback));
                    }
                    result = this.innerChannel.BeginClose(closeInnerChannelCallback, this);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.innerChannel.Abort();

                    backgroundCloseException = e;
                }

                if (backgroundCloseException != null)
                {
                    // stash away exception to throw out of user's Close()
                    this.closeState.CaptureBackgroundException(backgroundCloseException);
                }
                else if (result.CompletedSynchronously)
                {
                    OnCloseInnerChannel(result);
                }
            }

            static void CloseInnerChannelCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ((AutoCloseDuplexSessionChannel)result.AsyncState).OnCloseInnerChannel(result);
            }

            void OnCloseInnerChannel(IAsyncResult result)
            {
                Exception backgroundCloseException = null;
                try
                {
                    this.innerChannel.EndClose(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.innerChannel.Abort();
                    backgroundCloseException = e;
                }

                if (backgroundCloseException != null)
                {
                    // stash away exception to throw out of user's Close()
                    this.closeState.CaptureBackgroundException(backgroundCloseException);
                }
                else
                {
                    this.closeState.FinishBackgroundClose();
                }
            }

            public Message Receive()
            {
                return Receive(DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return this.pendingMessages.Dequeue(timeout);
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return BeginReceive(DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.pendingMessages.BeginDequeue(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                return this.pendingMessages.Dequeue(timeout, out message);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.pendingMessages.BeginDequeue(timeout, callback, state);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return this.pendingMessages.EndDequeue(result, out message);
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.pendingMessages.WaitForItem(timeout);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.pendingMessages.BeginWaitForItem(timeout, callback, state);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.pendingMessages.EndWaitForItem(result);
            }

            public T GetProperty<T>() where T : class
            {
                return this.innerChannel.GetProperty<T>();
            }

            public void Abort()
            {
                this.innerChannel.Abort();
                Cleanup();
            }

            public void Close()
            {
                Close(DefaultCloseTimeout);
            }

            public void Close(TimeSpan timeout)
            {
                bool performChannelClose;
                lock (ThisLock)
                {
                    performChannelClose = this.closeState.TryUserClose();
                }
                if (performChannelClose)
                {
                    this.innerChannel.Close(timeout);
                }
                else
                {
                    this.closeState.WaitForBackgroundClose(timeout);
                }
                Cleanup();
            }

            public IAsyncResult BeginClose(AsyncCallback callback, object state)
            {
                return BeginClose(DefaultCloseTimeout, callback, state);
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                bool performChannelClose;
                lock (ThisLock)
                {
                    performChannelClose = this.closeState.TryUserClose();
                }
                if (performChannelClose)
                {
                    return this.innerChannel.BeginClose(timeout, callback, state);
                }
                else
                {
                    return this.closeState.BeginWaitForBackgroundClose(timeout, callback, state);
                }
            }

            public void EndClose(IAsyncResult result)
            {
                // don't need to lock here since BeginClose is the sync-point
                if (this.closeState.TryUserClose())
                {
                    this.innerChannel.EndClose(result);
                }
                else
                {
                    this.closeState.EndWaitForBackgroundClose(result);
                }
                Cleanup();
            }

            // called from both Abort and Close paths
            void Cleanup()
            {
                this.pendingMessages.Dispose();
            }

            public void Open()
            {
                this.innerChannel.Open();
                this.StartBackgroundReceive();
            }

            public void Open(TimeSpan timeout)
            {
                this.innerChannel.Open(timeout);
                this.StartBackgroundReceive();
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(callback, state);
            }

            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginOpen(timeout, callback, state);
            }

            public void EndOpen(IAsyncResult result)
            {
                this.innerChannel.EndOpen(result);
                this.StartBackgroundReceive();
            }

            public void Send(Message message)
            {
                this.Send(message);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                this.Send(message, timeout);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(message, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            class CloseState
            {
                bool userClose;
                InputQueue<object> backgroundCloseData;

                public CloseState()
                {
                }

                public bool TryBackgroundClose()
                {
                    Fx.Assert(this.backgroundCloseData == null, "can't try twice");
                    if (!this.userClose)
                    {
                        this.backgroundCloseData = new InputQueue<object>();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void FinishBackgroundClose()
                {
                    Fx.Assert(this.backgroundCloseData != null, "Only callable from background close");
                    this.backgroundCloseData.Close();
                }

                public bool TryUserClose()
                {
                    if (this.backgroundCloseData == null)
                    {
                        this.userClose = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void WaitForBackgroundClose(TimeSpan timeout)
                {
                    Fx.Assert(this.backgroundCloseData != null, "Need to check background close first");
                    object dummy = this.backgroundCloseData.Dequeue(timeout);
                    Fx.Assert(dummy == null, "we should get an exception or null");
                }

                public IAsyncResult BeginWaitForBackgroundClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    Fx.Assert(this.backgroundCloseData != null, "Need to check background close first");
                    return this.backgroundCloseData.BeginDequeue(timeout, callback, state);
                }

                public void EndWaitForBackgroundClose(IAsyncResult result)
                {
                    Fx.Assert(this.backgroundCloseData != null, "Need to check background close first");
                    object dummy = this.backgroundCloseData.EndDequeue(result);
                    Fx.Assert(dummy == null, "we should get an exception or null");
                }

                public void CaptureBackgroundException(Exception exception)
                {
                    this.backgroundCloseData.EnqueueAndDispatch(exception, null, true);
                }

            }
        }
    }
}
