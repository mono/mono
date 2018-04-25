//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    
    abstract class ProbeRequestResponseAsyncResult<TProbeMessage, TResponseMessage> : AsyncResult
    {
        readonly IDiscoveryServiceImplementation discoveryServiceImpl;
        readonly FindRequestResponseContext findRequest;
        readonly DiscoveryOperationContext context;

        static AsyncCompletion onOnFindCompletedCallback = new AsyncCompletion(OnOnFindCompleted);
        bool isFindCompleted;

        [Fx.Tag.SynchronizationObject]
        object findCompletedLock;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ProbeRequestResponseAsyncResult(
            TProbeMessage probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            AsyncCallback callback,
            object state)
            : base(callback, state)
        {
            Fx.Assert(probeMessage != null, "The probeMessage must be non null.");
            Fx.Assert(discoveryServiceImpl != null, "The discoveryServiceImpl must be non null.");

            this.discoveryServiceImpl = discoveryServiceImpl;
            this.findCompletedLock = new object();

            if (!this.Validate(probeMessage))
            {
                this.Complete(true);
                return;
            }
            else
            {
                this.context = new DiscoveryOperationContext(OperationContext.Current);
                this.findRequest = new FindRequestResponseContext(this.GetFindCriteria(probeMessage), this);
                if (this.ProcessFindRequest())
                {
                    this.Complete(true);
                    return;
                }
            }
        }

        protected virtual bool Validate(TProbeMessage probeMessage)
        {
            return (DiscoveryService.EnsureMessageId() &&
                this.ValidateContent(probeMessage) &&
                this.EnsureNotDuplicate());
        }

        protected abstract bool ValidateContent(TProbeMessage probeMessage);

        protected abstract FindCriteria GetFindCriteria(TProbeMessage probeMessage);

        protected abstract TResponseMessage GetProbeResponse(
            DiscoveryMessageSequence discoveryMessageSequence,
            Collection<EndpointDiscoveryMetadata> matchingEndpoints);

        protected TResponseMessage End()
        {
            this.context.AddressRequestResponseMessage(OperationContext.Current);

            return this.GetProbeResponse(
                this.discoveryServiceImpl.GetNextMessageSequence(),
                this.findRequest.MatchingEndpoints);
        }

        static bool OnOnFindCompleted(IAsyncResult result)
        {            
            ProbeRequestResponseAsyncResult<TProbeMessage, TResponseMessage> thisPtr =
                (ProbeRequestResponseAsyncResult<TProbeMessage, TResponseMessage>)result.AsyncState;

            lock (thisPtr.findCompletedLock)
            {
                thisPtr.isFindCompleted = true;
            }

            thisPtr.discoveryServiceImpl.EndFind(result);
            return true;
        }

        bool ProcessFindRequest()
        {
            IAsyncResult result = this.discoveryServiceImpl.BeginFind(
                this.findRequest,
                this.PrepareAsyncCompletion(onOnFindCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnOnFindCompleted(result));
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

        class FindRequestResponseContext : FindRequestContext
        {
            Collection<EndpointDiscoveryMetadata> matchingEndpoints;
            readonly ProbeRequestResponseAsyncResult<TProbeMessage, TResponseMessage> probeRequestResponseAsyncResult;

            public FindRequestResponseContext(
                FindCriteria criteria, 
                ProbeRequestResponseAsyncResult<TProbeMessage, TResponseMessage> probeRequestResponseAsyncResult)
                : base(criteria)
            {
                this.matchingEndpoints = new Collection<EndpointDiscoveryMetadata>();
                this.probeRequestResponseAsyncResult = probeRequestResponseAsyncResult;
            }

            public Collection<EndpointDiscoveryMetadata> MatchingEndpoints
            {
                get 
                { 
                    return this.matchingEndpoints; 
                }
            }

            protected override void OnAddMatchingEndpoint(EndpointDiscoveryMetadata matchingEndpoint)
            {
                lock (this.probeRequestResponseAsyncResult.findCompletedLock)
                {
                    if (this.probeRequestResponseAsyncResult.isFindCompleted)
                    {
                        throw FxTrace.Exception.AsError(
                            new InvalidOperationException(SR.DiscoveryCannotAddMatchingEndpoint));
                    }
                    else
                    {
                        this.matchingEndpoints.Add(matchingEndpoint);
                    }
                }
            }
        }
    }
}
