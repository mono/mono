//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Runtime;
    using System.ServiceModel.Description;

    class DiscoveryInnerClientManagedCD1 : ClientBase<IDiscoveryContractManagedCD1>, IDiscoveryInnerClient
    {
        IDiscoveryInnerClientResponse responseReceiver;

        internal DiscoveryInnerClientManagedCD1(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver)
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
            ProbeMessageCD1 request = new ProbeMessageCD1();
            request.Probe = FindCriteriaCD1.FromFindCriteria(findCriteria);
            return base.Channel.BeginProbeOperation(request, callback, state);
        }

        public IAsyncResult BeginResolveOperation(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            ResolveMessageCD1 request = new ResolveMessageCD1();
            request.Resolve = ResolveCriteriaCD1.FromResolveCriteria(resolveCriteria);

            return base.Channel.BeginResolveOperation(request, callback, state);
        }

        public void EndProbeOperation(IAsyncResult result)
        {
            ProbeMatchesMessageCD1 response = base.Channel.EndProbeOperation(result);
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
            ResolveMatchesMessageCD1 response = base.Channel.EndResolveOperation(result);
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
            ProbeMessageCD1 request = new ProbeMessageCD1();
            request.Probe = FindCriteriaCD1.FromFindCriteria(findCriteria);

            ProbeMatchesMessageCD1 response = base.Channel.ProbeOperation(request);
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
            ResolveMessageCD1 request = new ResolveMessageCD1();
            request.Resolve = ResolveCriteriaCD1.FromResolveCriteria(resolveCriteria);

            ResolveMatchesMessageCD1 response = base.Channel.ResolveOperation(request);
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
