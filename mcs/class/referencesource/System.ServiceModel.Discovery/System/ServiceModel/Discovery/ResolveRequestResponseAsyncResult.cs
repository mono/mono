//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    
    abstract class ResolveRequestResponseAsyncResult<TResolveMessage, TResponseMessage> : AsyncResult
    {
        readonly ResolveCriteria resolveCriteria;
        readonly IDiscoveryServiceImplementation discoveryServiceImpl;
        readonly DiscoveryOperationContext context;
        static AsyncCompletion onOnResolveCompletedCallback = new AsyncCompletion(OnOnResolveCompleted);

        EndpointDiscoveryMetadata matchingEndpoint;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ResolveRequestResponseAsyncResult(
            TResolveMessage resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            AsyncCallback callback,
            object state)
            : base(callback, state)
        {
            Fx.Assert(resolveMessage != null, "The resolveMessage must be non null.");
            Fx.Assert(discoveryServiceImpl != null, "The discoveryServiceImpl must be non null.");

            this.discoveryServiceImpl = discoveryServiceImpl;

            if (!this.Validate(resolveMessage))
            {
                this.Complete(true);
                return;
            }
            else
            {
                this.context = new DiscoveryOperationContext(OperationContext.Current);
                this.resolveCriteria = this.GetResolveCriteria(resolveMessage);
                if (this.ProcessResolveRequest())
                {
                    this.Complete(true);
                    return;
                }
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
                this.ValidateContent(resolveMessage) &&
                this.EnsureNotDuplicate());
        }

        protected abstract bool ValidateContent(TResolveMessage resolveMessage);

        protected abstract ResolveCriteria GetResolveCriteria(TResolveMessage resolveMessage);

        protected abstract TResponseMessage GetResolveResponse(
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoints);

        protected TResponseMessage End()
        {
            this.context.AddressRequestResponseMessage(OperationContext.Current);

            return this.GetResolveResponse(
                this.discoveryServiceImpl.GetNextMessageSequence(),
                this.matchingEndpoint);
        }

        static bool OnOnResolveCompleted(IAsyncResult result)
        {
            ResolveRequestResponseAsyncResult<TResolveMessage, TResponseMessage> thisPtr =
                (ResolveRequestResponseAsyncResult<TResolveMessage, TResponseMessage>)result.AsyncState;

            thisPtr.matchingEndpoint = thisPtr.discoveryServiceImpl.EndResolve(result);
            return true;
        }

        bool ProcessResolveRequest()
        {
            IAsyncResult result = this.discoveryServiceImpl.BeginResolve(
                this.resolveCriteria,
                this.PrepareAsyncCompletion(onOnResolveCompletedCallback),
                this);

            return (result.CompletedSynchronously && OnOnResolveCompleted(result));
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
    }
}
