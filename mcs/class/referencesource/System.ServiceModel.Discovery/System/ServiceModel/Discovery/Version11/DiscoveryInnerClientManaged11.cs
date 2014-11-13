//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Runtime;
    using System.ServiceModel.Description;

    class DiscoveryInnerClientManaged11 : ClientBase<IDiscoveryContractManaged11>, IDiscoveryInnerClient
    {
        IDiscoveryInnerClientResponse responseReceiver;

        internal DiscoveryInnerClientManaged11(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver)
            : base(discoveryEndpoint)
        {
            Fx.Assert(responseReceiver != null, "The responseReceiver parameter cannot be null");
            this.responseReceiver = responseReceiver;
        }

        public new ClientCredentials ClientCredentials
        {
            get
            {
                return base.ClientCredentials;
            }
        }

        public new ChannelFactory ChannelFactory
        {
            get
            {
                return base.ChannelFactory;
            }
        }

        public new IClientChannel InnerChannel
        {
            get
            {
                return base.InnerChannel;
            }
        }

        public new ServiceEndpoint Endpoint
        {
            get
            {
                return base.Endpoint;
            }
        }

        public ICommunicationObject InnerCommunicationObject
        {
            get
            {
                return this as ICommunicationObject;
            }
        }

        public bool IsRequestResponse
        {
            get
            {
                return true;
            }
        }

        public IAsyncResult BeginProbeOperation(FindCriteria findCriteria, AsyncCallback callback, object state)
        {
            ProbeMessage11 request = new ProbeMessage11();
            request.Probe = FindCriteria11.FromFindCriteria(findCriteria);
            return base.Channel.BeginProbeOperation(request, callback, state);
        }

        public IAsyncResult BeginResolveOperation(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            ResolveMessage11 request = new ResolveMessage11();
            request.Resolve = ResolveCriteria11.FromResolveCriteria(resolveCriteria);

            return base.Channel.BeginResolveOperation(request, callback, state);
        }

        public void EndProbeOperation(IAsyncResult result)
        {
            ProbeMatchesMessage11 response = base.Channel.EndProbeOperation(result);
            AsyncOperationContext context = (AsyncOperationContext)result.AsyncState;
            if ((response != null) && (response.ProbeMatches != null))
            {                
                this.responseReceiver.ProbeMatchOperation(
                    context.OperationId, 
                    DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(response.MessageSequence), 
                    DiscoveryUtility.ToEndpointDiscoveryMetadataCollection(response.ProbeMatches),
                    true);
            }
            else
            {
                this.responseReceiver.PostFindCompletedAndRemove(context.OperationId, false, null);
            }
        }

        public void EndResolveOperation(IAsyncResult result)
        {
            ResolveMatchesMessage11 response = base.Channel.EndResolveOperation(result);
            AsyncOperationContext context = (AsyncOperationContext)result.AsyncState;
            if ((response != null) && (response.ResolveMatches != null) && (response.ResolveMatches.ResolveMatch != null))
            {               
                this.responseReceiver.ResolveMatchOperation(
                    context.OperationId,
                    DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(response.MessageSequence), 
                    response.ResolveMatches.ResolveMatch.ToEndpointDiscoveryMetadata());
            }
            else
            {
                this.responseReceiver.PostResolveCompletedAndRemove(context.OperationId, false, null);
            }
        }

        public void ProbeOperation(FindCriteria findCriteria)
        {
            ProbeMessage11 request = new ProbeMessage11();
            request.Probe = FindCriteria11.FromFindCriteria(findCriteria);

            ProbeMatchesMessage11 response = base.Channel.ProbeOperation(request);
            if ((response != null) && (response.ProbeMatches != null))
            {
                this.responseReceiver.ProbeMatchOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo, 
                    DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(response.MessageSequence), 
                    DiscoveryUtility.ToEndpointDiscoveryMetadataCollection(response.ProbeMatches),
                    true);
            }
            else
            {
                this.responseReceiver.PostFindCompletedAndRemove(OperationContext.Current.IncomingMessageHeaders.RelatesTo, false, null);
            }
        }

        public void ResolveOperation(ResolveCriteria resolveCriteria)
        {
            ResolveMessage11 request = new ResolveMessage11();
            request.Resolve = ResolveCriteria11.FromResolveCriteria(resolveCriteria);

            ResolveMatchesMessage11 response = base.Channel.ResolveOperation(request);
            if ((response != null) && (response.ResolveMatches != null) && (response.ResolveMatches.ResolveMatch != null))
            {
                this.responseReceiver.ResolveMatchOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo,
                    DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(response.MessageSequence), 
                    response.ResolveMatches.ResolveMatch.ToEndpointDiscoveryMetadata());
            }
            else
            {
                this.responseReceiver.PostResolveCompletedAndRemove(OperationContext.Current.IncomingMessageHeaders.RelatesTo, false, null);
            }
        }
    }
}
