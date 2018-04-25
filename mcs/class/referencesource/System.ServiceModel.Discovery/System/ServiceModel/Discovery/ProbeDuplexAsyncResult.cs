//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    
    abstract class ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> : AsyncResult
    {
        readonly IDiscoveryServiceImplementation discoveryServiceImpl;
        readonly IMulticastSuppressionImplementation multicastSuppressionImpl;
        readonly DuplexFindContext findRequest;
        readonly DiscoveryOperationContext context;
        readonly TimeoutHelper timeoutHelper;        

        static AsyncCompletion onShouldRedirectFindCompletedCallback = new AsyncCompletion(OnShouldRedirectFindCompleted);
        static AsyncCompletion onSendProxyAnnouncementsCompletedCallback = new AsyncCompletion(OnSendProxyAnnouncementsCompleted);
        static AsyncCallback onFindCompletedCallback = Fx.ThunkCallback(new AsyncCallback(OnFindCompleted));
        static AsyncCompletion onSendFindResponsesCompletedCallback = new AsyncCompletion(OnSendFindResponsesCompleted);

        bool isFindCompleted;
        
        [Fx.Tag.SynchronizationObject]
        object findCompletedLock;

        TResponseChannel responseChannel;
        Exception findException;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ProbeDuplexAsyncResult(TProbeMessage probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(callback, state)
        {
            Fx.Assert(probeMessage != null, "The probeMessage must be non null.");
            Fx.Assert(discoveryServiceImpl != null, "The discoveryServiceImpl must be non null.");

            this.discoveryServiceImpl = discoveryServiceImpl;
            this.multicastSuppressionImpl = multicastSuppressionImpl;
            this.findCompletedLock = new object();

            if (!this.Validate(probeMessage))
            {
                this.Complete(true);
                return;
            }
            else
            {
                this.context = new DiscoveryOperationContext(OperationContext.Current);
                this.findRequest = new DuplexFindContext(this.GetFindCriteria(probeMessage), this);
                this.timeoutHelper = new TimeoutHelper(this.findRequest.Criteria.Duration);
                this.timeoutHelper.RemainingTime();                
                this.Process();
            }
        }

        protected DiscoveryOperationContext Context
        {
            get
            {
                return this.context;
            }
        }

        TResponseChannel ResponseChannel
        {
            get
            {
                if (this.responseChannel == null)
                {
                    this.responseChannel = this.context.GetCallbackChannel<TResponseChannel>();
                }

                return this.responseChannel;
            }
        }

        protected virtual bool Validate(TProbeMessage probeMessage)
        {
            return (DiscoveryService.EnsureMessageId() &&
                DiscoveryService.EnsureReplyTo() &&
                this.ValidateContent(probeMessage) &&
                this.EnsureNotDuplicate());
        }

        protected abstract bool ValidateContent(TProbeMessage probeMessage);

        protected abstract FindCriteria GetFindCriteria(TProbeMessage probeMessage);

        protected abstract IAsyncResult BeginSendFindResponse(
            TResponseChannel responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoint,
            AsyncCallback callback,
            object state);
        protected abstract void EndSendFindResponse(TResponseChannel responseChannel, IAsyncResult result);

        protected abstract IAsyncResult BeginSendProxyAnnouncement(
            TResponseChannel responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata proxyEndpointDiscoveryMetadata,
            AsyncCallback callback,
            object state);
        protected abstract void EndSendProxyAnnouncement(TResponseChannel responseChannel, IAsyncResult result);

        static bool OnShouldRedirectFindCompleted(IAsyncResult result)
        {
            Collection<EndpointDiscoveryMetadata> redirectionEndpoints = null;

            ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> thisPtr =
                (ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel>)result.AsyncState;

            if (thisPtr.multicastSuppressionImpl.EndShouldRedirectFind(result, out redirectionEndpoints))
            {
                return thisPtr.SendProxyAnnouncements(redirectionEndpoints);
            }
            else
            {
                return thisPtr.ProcessFindRequest();
            }
        }

        static bool OnSendProxyAnnouncementsCompleted(IAsyncResult result)
        {
            ProxyAnnouncementsSendAsyncResult.End(result);
            return true;
        }

        static void OnFindCompleted(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            else
            {
                ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> thisPtr =
                    (ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel>)result.AsyncState;
                thisPtr.FinishFind(result);
            }
        }

        static bool OnSendFindResponsesCompleted(IAsyncResult result)
        {
            FindResponsesSendAsyncResult.End(result);

            ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> thisPtr =
                (ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel>)result.AsyncState;
            if (thisPtr.findException != null)
            {
                throw FxTrace.Exception.AsError(thisPtr.findException);
            }

            return true;
        }

        void FinishFind(IAsyncResult result)
        {
            try
            {
                lock (this.findCompletedLock)
                {
                    this.isFindCompleted = true;
                }
                this.discoveryServiceImpl.EndFind(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                this.findException = e;
            }
            finally
            {
                this.findRequest.MatchingEndpoints.Shutdown();
            }
        }

        void Process()
        {
            if ((this.multicastSuppressionImpl != null) && (this.context.DiscoveryMode == ServiceDiscoveryMode.Adhoc))
            {
                if (this.SuppressFindRequest())
                {
                    this.Complete(true);
                    return;
                }
            }
            else
            {
                if (this.ProcessFindRequest())
                {
                    this.Complete(true);
                    return;
                }
            }
        }

        bool SuppressFindRequest()
        {
            IAsyncResult result = this.multicastSuppressionImpl.BeginShouldRedirectFind(
                this.findRequest.Criteria,
                this.PrepareAsyncCompletion(onShouldRedirectFindCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnShouldRedirectFindCompleted(result));
        }

        bool SendProxyAnnouncements(Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
        {
            if ((redirectionEndpoints == null) || (redirectionEndpoints.Count == 0))
            {
                return true;
            }

            IAsyncResult result = new ProxyAnnouncementsSendAsyncResult(
                this,
                redirectionEndpoints,
                this.PrepareAsyncCompletion(onSendProxyAnnouncementsCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnSendProxyAnnouncementsCompleted(result));
        }

        bool ProcessFindRequest()
        {
            IAsyncResult result = this.discoveryServiceImpl.BeginFind(
                findRequest,
                onFindCompletedCallback,
                this);

            if (result.CompletedSynchronously)
            {
                this.FinishFind(result);
            }

            return this.SendFindResponses();
        }

        bool SendFindResponses()
        {
            IAsyncResult result = new FindResponsesSendAsyncResult(
                this,
                this.PrepareAsyncCompletion(onSendFindResponsesCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnSendFindResponsesCompleted(result));
        }

        bool EnsureNotDuplicate()
        {
            bool isDuplicate = this.discoveryServiceImpl.IsDuplicate(OperationContext.Current.IncomingMessageHeaders.MessageId);

            if (isDuplicate && TD.DuplicateDiscoveryMessageIsEnabled())
            {
                TD.DuplicateDiscoveryMessage(
                    this.context.EventTraceActivity,
                    ProtocolStrings.TracingStrings.Probe,
                    OperationContext.Current.IncomingMessageHeaders.MessageId.ToString());
            }

            return !isDuplicate;
        }

        IAsyncResult BeginSendFindResponse(
            EndpointDiscoveryMetadata matchingEndpoint, 
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            IAsyncResult result;
            IContextChannel contextChannel = (IContextChannel)this.ResponseChannel;
            using (new OperationContextScope(contextChannel))
            {
                this.context.AddressDuplexResponseMessage(OperationContext.Current);

                contextChannel.OperationTimeout = timeout;

                result = this.BeginSendFindResponse(
                    this.ResponseChannel,
                    this.discoveryServiceImpl.GetNextMessageSequence(),
                    matchingEndpoint,
                    callback,
                    state);
            }

            return result;
        }

        void EndSendFindResponse(IAsyncResult result)
        {
            this.EndSendFindResponse(this.ResponseChannel, result);
        }

        IAsyncResult BeginSendProxyAnnouncement(
            EndpointDiscoveryMetadata proxyEndpoint, 
            TimeSpan timeout, 
            AsyncCallback callback, 
            object state)
        {
            IAsyncResult result;
            IContextChannel contextChannel = (IContextChannel)this.ResponseChannel;
            using (new OperationContextScope(contextChannel))
            {
                this.context.AddressDuplexResponseMessage(OperationContext.Current);

                contextChannel.OperationTimeout = timeout;

                result = this.BeginSendProxyAnnouncement(
                    this.ResponseChannel,
                    this.discoveryServiceImpl.GetNextMessageSequence(),
                    proxyEndpoint,
                    callback,
                    state);
            }

            return result;
        }

        void EndSendProxyAnnouncement(IAsyncResult result)
        {
            this.EndSendProxyAnnouncement(this.ResponseChannel, result);
        }

        class ProxyAnnouncementsSendAsyncResult : RandomDelaySendsAsyncResult
        {
            ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> probeDuplexAsyncResult;
            Collection<EndpointDiscoveryMetadata> redirectionEndpoints;

            public ProxyAnnouncementsSendAsyncResult(
                ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> probeDuplexAsyncResult,
                Collection<EndpointDiscoveryMetadata> redirectionEndpoints,
                AsyncCallback callback,
                object state)
                : base(
                redirectionEndpoints.Count,
                probeDuplexAsyncResult.context.MaxResponseDelay,
                callback,
                state)
            {
                this.probeDuplexAsyncResult = probeDuplexAsyncResult;
                this.redirectionEndpoints = redirectionEndpoints;
                this.Start(this.probeDuplexAsyncResult.timeoutHelper.RemainingTime());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ProxyAnnouncementsSendAsyncResult>(result);
            }

            protected override IAsyncResult OnBeginSend(int index, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.probeDuplexAsyncResult.BeginSendProxyAnnouncement(
                    this.redirectionEndpoints[index],
                    timeout,
                    callback,
                    state);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                this.probeDuplexAsyncResult.EndSendProxyAnnouncement(result);
            }
        }

        class FindResponsesSendAsyncResult : RandomDelayQueuedSendsAsyncResult<EndpointDiscoveryMetadata>
        {
            readonly ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> probeDuplexAsyncResult;

            public FindResponsesSendAsyncResult(
                ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> probeDuplexAsyncResult,
                AsyncCallback callback,
                object state)
                : base(
                probeDuplexAsyncResult.context.MaxResponseDelay,
                probeDuplexAsyncResult.findRequest.MatchingEndpoints,                
                callback,
                state)
            {
                this.probeDuplexAsyncResult = probeDuplexAsyncResult;
                this.Start(this.probeDuplexAsyncResult.timeoutHelper.RemainingTime());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FindResponsesSendAsyncResult>(result);
            }

            protected override IAsyncResult OnBeginSendItem(
                EndpointDiscoveryMetadata item, 
                TimeSpan timeout,
                AsyncCallback callback, 
                object state)
            {
                return this.probeDuplexAsyncResult.BeginSendFindResponse(
                    item,
                    timeout,
                    callback,
                    state);
            }

            protected override void OnEndSendItem(IAsyncResult result)
            {
                this.probeDuplexAsyncResult.EndSendFindResponse(result);
            }
        }

        class DuplexFindContext : FindRequestContext
        {
            readonly InputQueue<EndpointDiscoveryMetadata> matchingEndpoints;
            readonly ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> probeDuplexAsyncResult;

            public DuplexFindContext(FindCriteria criteria, ProbeDuplexAsyncResult<TProbeMessage, TResponseChannel> probeDuplexAsyncResult)
                : base(criteria)
            {
                this.matchingEndpoints = new InputQueue<EndpointDiscoveryMetadata>();
                this.probeDuplexAsyncResult = probeDuplexAsyncResult;
            }

            public InputQueue<EndpointDiscoveryMetadata> MatchingEndpoints
            {
                get
                {
                    return this.matchingEndpoints;
                }
            }

            protected override void OnAddMatchingEndpoint(EndpointDiscoveryMetadata matchingEndpoint)
            {                
                lock (this.probeDuplexAsyncResult.findCompletedLock)
                {
                    if (this.probeDuplexAsyncResult.isFindCompleted)
                    {
                        throw FxTrace.Exception.AsError(
                            new InvalidOperationException(SR.DiscoveryCannotAddMatchingEndpoint));
                    }
                    else
                    {
                        this.matchingEndpoints.EnqueueAndDispatch(matchingEndpoint, null, false);
                    }
                }                
            }
        }
    }
}
