//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Description;

    class DiscoveryInnerClientApril2005<TChannel> : IDiscoveryInnerClient, IDiscoveryResponseContractApril2005
        where TChannel : class, IDiscoveryContractApril2005
    {
        IDiscoveryInnerClientResponse responseReceiver;
        DuplexClientApril2005 duplexInnerClient;

        public DiscoveryInnerClientApril2005(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver)
        {
            Fx.Assert(discoveryEndpoint != null, "The discoveryEndpoint parameter cannot be null");
            Fx.Assert(responseReceiver != null, "The responseReceiver parameter cannot be null");

            this.responseReceiver = responseReceiver;
            if (discoveryEndpoint.Behaviors.Find<DiscoveryCallbackBehavior>() == null)
            {
                discoveryEndpoint.Behaviors.Insert(0, new DiscoveryCallbackBehavior());
            }

            this.duplexInnerClient = new DuplexClientApril2005(this, discoveryEndpoint);
        }

        public ClientCredentials ClientCredentials
        {
            get
            {
                return this.duplexInnerClient.ClientCredentials;
            }
        }

        public ChannelFactory ChannelFactory
        {
            get
            {
                return this.duplexInnerClient.ChannelFactory;
            }
        }

        public IClientChannel InnerChannel
        {
            get
            {
                return this.duplexInnerClient.InnerChannel;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                return this.duplexInnerClient.Endpoint;
            }
        }

        public ICommunicationObject InnerCommunicationObject
        {
            get
            {
                return this.duplexInnerClient as ICommunicationObject;
            }
        }

        public bool IsRequestResponse
        {
            get
            {
                return false;
            }
        }

        public IAsyncResult BeginProbeOperation(FindCriteria findCriteria, AsyncCallback callback, object state)
        {
            ProbeMessageApril2005 request = new ProbeMessageApril2005();
            request.Probe = FindCriteriaApril2005.FromFindCriteria(findCriteria);
            return this.duplexInnerClient.BeginProbeOperation(request, callback, state);
        }

        public IAsyncResult BeginResolveOperation(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            ResolveMessageApril2005 request = new ResolveMessageApril2005();
            request.Resolve = ResolveCriteriaApril2005.FromResolveCriteria(resolveCriteria);
            return this.duplexInnerClient.BeginResolveOperation(request, callback, state);
        }

        public void EndProbeOperation(IAsyncResult result)
        {
            this.duplexInnerClient.EndProbeOperation(result);
        }

        public void EndResolveOperation(IAsyncResult result)
        {
            this.duplexInnerClient.EndResolveOperation(result);
        }

        public void ProbeOperation(FindCriteria findCriteria)
        {
            ProbeMessageApril2005 request = new ProbeMessageApril2005();
            request.Probe = FindCriteriaApril2005.FromFindCriteria(findCriteria);
            this.duplexInnerClient.ProbeOperation(request);
        }

        public void ResolveOperation(ResolveCriteria resolveCriteria)
        {
            ResolveMessageApril2005 request = new ResolveMessageApril2005();
            request.Resolve = ResolveCriteriaApril2005.FromResolveCriteria(resolveCriteria);
            this.duplexInnerClient.ResolveOperation(request);
        }

        public IAsyncResult BeginProbeMatchOperation(ProbeMatchesMessageApril2005 response, AsyncCallback callback, object state)
        {
            Fx.Assert(response != null, "The response message cannot be null.");
            if (response.ProbeMatches != null)
            {                
                this.responseReceiver.ProbeMatchOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo,
                    DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(response.MessageSequence),
                    DiscoveryUtility.ToEndpointDiscoveryMetadataCollection(response.ProbeMatches),
                    false);
            }
            return new CompletedAsyncResult(callback, state);
        }

        public void EndProbeMatchOperation(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public IAsyncResult BeginResolveMatchOperation(ResolveMatchesMessageApril2005 response, AsyncCallback callback, object state)
        {
            Fx.Assert(response != null, "The response message cannot be null.");
            if ((response.ResolveMatches != null) && (response.ResolveMatches.ResolveMatch != null))
            {
                this.responseReceiver.ResolveMatchOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo,
                    DiscoveryUtility.ToDiscoveryMessageSequenceOrNull(response.MessageSequence), 
                    response.ResolveMatches.ResolveMatch.ToEndpointDiscoveryMetadata());
            }
            return new CompletedAsyncResult(callback, state);
        }

        public void EndResolveMatchOperation(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public IAsyncResult BeginHelloOperation(HelloMessageApril2005 message, AsyncCallback callback, object state)
        {
            Fx.Assert(message != null, "The message cannot be null.");
            if ((message.MessageSequence != null) && (message.Hello != null))
            {
                this.responseReceiver.HelloOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo, 
                    message.MessageSequence.ToDiscoveryMessageSequence(), 
                    message.Hello.ToEndpointDiscoveryMetadata());
            }
            else
            {
                if (TD.DiscoveryMessageWithNullMessageSequenceIsEnabled() && message.MessageSequence == null)
                {
                    TD.DiscoveryMessageWithNullMessageSequence(
                        ProtocolStrings.TracingStrings.Hello,
                        OperationContext.Current.IncomingMessageHeaders.MessageId.ToString());
                }
            }
            return new CompletedAsyncResult(callback, state);
        }

        public void EndHelloOperation(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        class DuplexClientApril2005 : DuplexClientBase<TChannel>
        {
            public DuplexClientApril2005(object callbackInstance, DiscoveryEndpoint discoveryEndpoint)
                : base(callbackInstance, discoveryEndpoint)
            {
            }

            public void ProbeOperation(ProbeMessageApril2005 request)
            {
                base.Channel.ProbeOperation(request);
            }

            public void ResolveOperation(ResolveMessageApril2005 request)
            {
                base.Channel.ResolveOperation(request);
            }

            public IAsyncResult BeginProbeOperation(ProbeMessageApril2005 request, AsyncCallback callback, object state)
            {
                return base.Channel.BeginProbeOperation(request, callback, state);
            }

            public IAsyncResult BeginResolveOperation(ResolveMessageApril2005 request, AsyncCallback callback, object state)
            {
                return base.Channel.BeginResolveOperation(request, callback, state);
            }

            public void EndProbeOperation(IAsyncResult result)
            {
                base.Channel.EndProbeOperation(result);
            }

            public void EndResolveOperation(IAsyncResult result)
            {
                base.Channel.EndResolveOperation(result);
            }
        }
    }
}
