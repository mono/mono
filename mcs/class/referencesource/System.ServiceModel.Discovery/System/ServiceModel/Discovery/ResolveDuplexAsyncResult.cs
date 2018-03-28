//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    
    abstract class ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel> : AsyncResult
    {
        readonly IDiscoveryServiceImplementation discoveryServiceImpl;
        readonly IMulticastSuppressionImplementation multicastSuppressionImpl;
        readonly ResolveCriteria resolveCriteria;
        readonly DiscoveryOperationContext context;
        readonly TimeoutHelper timeoutHelper;

        static AsyncCompletion onShouldRedirectResolveCompletedCallback = new AsyncCompletion(OnShouldRedirectResolveCompleted);
        static AsyncCompletion onSendProxyAnnouncementsCompletedCallback = new AsyncCompletion(OnSendProxyAnnouncementsCompleted);

        static AsyncCompletion onOnResolveCompletedCallback = new AsyncCompletion(OnOnResolveCompleted);
        static AsyncCompletion onSendResolveResponseCompletedCallback = new AsyncCompletion(OnSendResolveResponseCompleted);
        TResponseChannel responseChannel;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ResolveDuplexAsyncResult(TResolveMessage resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(callback, state)
        {
            Fx.Assert(resolveMessage != null, "The resolveMessage must be non null.");
            Fx.Assert(discoveryServiceImpl != null, "The discoveryServiceImpl must be non null.");

            this.discoveryServiceImpl = discoveryServiceImpl;
            this.multicastSuppressionImpl = multicastSuppressionImpl;

            if (!this.Validate(resolveMessage))
            {
                this.Complete(true);
                return;
            }
            else
            {
                this.context = new DiscoveryOperationContext(OperationContext.Current);
                this.resolveCriteria = this.GetResolveCriteria(resolveMessage);
                this.timeoutHelper = new TimeoutHelper(this.resolveCriteria.Duration);
                this.timeoutHelper.RemainingTime();
                this.Process();
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

        protected DiscoveryOperationContext Context
        {
            get
            {
                return this.context;
            }
        }

        protected virtual bool Validate(TResolveMessage resolveMessage)
        {
            return (DiscoveryService.EnsureMessageId() &&
                DiscoveryService.EnsureReplyTo() &&
                this.ValidateContent(resolveMessage) &&
                this.EnsureNotDuplicate());
        }

        protected abstract bool ValidateContent(TResolveMessage resolveMessage);

        protected abstract ResolveCriteria GetResolveCriteria(TResolveMessage resolveMessage);

        protected abstract IAsyncResult BeginSendResolveResponse(
            TResponseChannel responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence, 
            EndpointDiscoveryMetadata matchingEndpoint, 
            AsyncCallback callback, 
            object state);
        protected abstract void EndSendResolveResponse(TResponseChannel responseChannel, IAsyncResult result);

        protected abstract IAsyncResult BeginSendProxyAnnouncement(
            TResponseChannel responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata proxyEndpointDiscoveryMetadata,
            AsyncCallback callback,
            object state);
        protected abstract void EndSendProxyAnnouncement(TResponseChannel responseChannel, IAsyncResult result);

        static bool OnShouldRedirectResolveCompleted(IAsyncResult result)
        {
            Collection<EndpointDiscoveryMetadata> redirectionEndpoints = null;

            ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel> thisPtr =
                (ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel>)result.AsyncState;

            if (thisPtr.multicastSuppressionImpl.EndShouldRedirectResolve(result, out redirectionEndpoints))
            {
                return thisPtr.SendProxyAnnouncements(redirectionEndpoints);
            }
            else
            {
                return thisPtr.ProcessResolveRequest();
            }
        }

        static bool OnSendProxyAnnouncementsCompleted(IAsyncResult result)
        {
            ProxyAnnouncementsSendAsyncResult.End(result);
            return true;
        }

        static bool OnOnResolveCompleted(IAsyncResult result)
        {
            ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel> thisPtr =
                (ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel>)result.AsyncState;

            EndpointDiscoveryMetadata matchingEndpoint = thisPtr.discoveryServiceImpl.EndResolve(result);

            return thisPtr.SendResolveResponse(matchingEndpoint);
        }

        static bool OnSendResolveResponseCompleted(IAsyncResult result)
        {
            ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel> thisPtr =
                (ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel>)result.AsyncState;

            thisPtr.EndSendResolveResponse(thisPtr.ResponseChannel, result);
            return true;
        }

        void Process()
        {
            if ((this.multicastSuppressionImpl != null) && (this.context.DiscoveryMode == ServiceDiscoveryMode.Adhoc))
            {
                if (this.SuppressResolveRequest())
                {
                    Complete(true);
                    return;
                }
            }
            else
            {
                if (this.ProcessResolveRequest())
                {
                    Complete(true);
                    return;
                }
            }
        }

        bool SuppressResolveRequest()
        {
            IAsyncResult result = this.multicastSuppressionImpl.BeginShouldRedirectResolve(
                this.resolveCriteria,
                this.PrepareAsyncCompletion(onShouldRedirectResolveCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnShouldRedirectResolveCompleted(result));
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

        bool ProcessResolveRequest()
        {
            IAsyncResult result = this.discoveryServiceImpl.BeginResolve(
                resolveCriteria,
                PrepareAsyncCompletion(onOnResolveCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnOnResolveCompleted(result));
        }

        bool SendResolveResponse(EndpointDiscoveryMetadata matchingEndpoint)
        {
            if (matchingEndpoint == null)
            {
                return true;
            }

            IContextChannel contextChannel = (IContextChannel)this.ResponseChannel;
            IAsyncResult result = null;
            using (new OperationContextScope(contextChannel))
            {
                this.context.AddressDuplexResponseMessage(OperationContext.Current);

                contextChannel.OperationTimeout = this.timeoutHelper.RemainingTime();

                result = this.BeginSendResolveResponse(
                    this.ResponseChannel,
                    this.discoveryServiceImpl.GetNextMessageSequence(),
                    matchingEndpoint,
                    this.PrepareAsyncCompletion(onSendResolveResponseCompletedCallback),
                    this);
            }

            return (result.CompletedSynchronously && OnSendResolveResponseCompleted(result));
        }

        bool EnsureNotDuplicate()
        {
            bool isDuplicate = this.discoveryServiceImpl.IsDuplicate(OperationContext.Current.IncomingMessageHeaders.MessageId);

            if (isDuplicate && TD.DuplicateDiscoveryMessageIsEnabled())
            {
                TD.DuplicateDiscoveryMessage(
                    this.context.EventTraceActivity,
                    ProtocolStrings.TracingStrings.Resolve,
                    OperationContext.Current.IncomingMessageHeaders.MessageId.ToString());
            }

            return !isDuplicate;
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
            ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel> resolveDuplexAsyncResult;
            Collection<EndpointDiscoveryMetadata> redirectionEndpoints;

            public ProxyAnnouncementsSendAsyncResult(
                ResolveDuplexAsyncResult<TResolveMessage, TResponseChannel> resolveDuplexAsyncResult,
                Collection<EndpointDiscoveryMetadata> redirectionEndpoints,
                AsyncCallback callback,
                object state)
                : base(
                redirectionEndpoints.Count,
                resolveDuplexAsyncResult.context.MaxResponseDelay,
                callback,
                state)
            {
                this.resolveDuplexAsyncResult = resolveDuplexAsyncResult;
                this.redirectionEndpoints = redirectionEndpoints;
                this.Start(this.resolveDuplexAsyncResult.timeoutHelper.RemainingTime());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ProxyAnnouncementsSendAsyncResult>(result);
            }

            protected override IAsyncResult OnBeginSend(int index, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.resolveDuplexAsyncResult.BeginSendProxyAnnouncement(
                    this.redirectionEndpoints[index],
                    timeout,
                    callback,
                    state);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                this.resolveDuplexAsyncResult.EndSendProxyAnnouncement(result);
            }
        }
    }
}
