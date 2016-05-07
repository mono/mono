//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Description;

    class DiscoveryInnerClientAdhoc11 : IDiscoveryInnerClient, IDiscoveryResponseContract11
    {
        IDiscoveryInnerClientResponse responseReceiver;
        DuplexClient11 duplexInnerClient;

        public DiscoveryInnerClientAdhoc11(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver)
        {
            Fx.Assert(discoveryEndpoint != null, "The discoveryEndpoint parameter cannot be null");
            Fx.Assert(responseReceiver != null, "The responseReceiver parameter cannot be null");

            this.responseReceiver = responseReceiver;
            if (discoveryEndpoint.Behaviors.Find<DiscoveryCallbackBehavior>() == null)
            {
                discoveryEndpoint.Behaviors.Insert(0, new DiscoveryCallbackBehavior());
            }

            this.duplexInnerClient = new DuplexClient11(this, discoveryEndpoint);
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
            ProbeMessage11 request = new ProbeMessage11();
            request.Probe = FindCriteria11.FromFindCriteria(findCriteria);
            return this.duplexInnerClient.BeginProbeOperation(request, callback, state);
        }

        public IAsyncResult BeginResolveOperation(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            ResolveMessage11 request = new ResolveMessage11();
            request.Resolve = ResolveCriteria11.FromResolveCriteria(resolveCriteria);
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
            ProbeMessage11 request = new ProbeMessage11();
            request.Probe = FindCriteria11.FromFindCriteria(findCriteria);
            this.duplexInnerClient.ProbeOperation(request);
        }

        public void ResolveOperation(ResolveCriteria resolveCriteria)
        {
            ResolveMessage11 request = new ResolveMessage11();
            request.Resolve = ResolveCriteria11.FromResolveCriteria(resolveCriteria);
            this.duplexInnerClient.ResolveOperation(request);
        }

        public IAsyncResult BeginProbeMatchOperation(ProbeMatchesMessage11 response, AsyncCallback callback, object state)
        {
            Fx.Assert(response != null, "The response message cannot be null.");
            if ((response.MessageSequence != null) && (response.ProbeMatches != null))
            {
                this.responseReceiver.ProbeMatchOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo,
                    response.MessageSequence.ToDiscoveryMessageSequence(),
                    DiscoveryUtility.ToEndpointDiscoveryMetadataCollection(response.ProbeMatches),
                    false);
            }
            else
            {
                if (TD.DiscoveryMessageWithNullMessageSequenceIsEnabled() && response.MessageSequence == null)
                {
                    TD.DiscoveryMessageWithNullMessageSequence(
                        ProtocolStrings.TracingStrings.ProbeMatches,
                        OperationContext.Current.IncomingMessageHeaders.MessageId.ToString());
                }
            }

            return new CompletedAsyncResult(callback, state);
        }

        public void EndProbeMatchOperation(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public IAsyncResult BeginResolveMatchOperation(ResolveMatchesMessage11 response, AsyncCallback callback, object state)
        {
            Fx.Assert(response != null, "The response message cannot be null.");
            if ((response.MessageSequence != null) && (response.ResolveMatches != null) && (response.ResolveMatches.ResolveMatch != null))
            {
                this.responseReceiver.ResolveMatchOperation(
                    OperationContext.Current.IncomingMessageHeaders.RelatesTo, 
                    response.MessageSequence.ToDiscoveryMessageSequence(), 
                    response.ResolveMatches.ResolveMatch.ToEndpointDiscoveryMetadata());
            }
            else
            {
                if (TD.DiscoveryMessageWithNullMessageSequenceIsEnabled() && response.MessageSequence == null)
                {
                    TD.DiscoveryMessageWithNullMessageSequence(
                        ProtocolStrings.TracingStrings.ResolveMatches,
                        OperationContext.Current.IncomingMessageHeaders.MessageId.ToString());
                }
            }

            return new CompletedAsyncResult(callback, state);
        }

        public void EndResolveMatchOperation(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        public IAsyncResult BeginHelloOperation(HelloMessage11 message, AsyncCallback callback, object state)
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

        class DuplexClient11 : DuplexClientBase<IDiscoveryContractAdhoc11>
        {
            public DuplexClient11(object callbackInstance, DiscoveryEndpoint discoveryEndpoint)
                : base(callbackInstance, discoveryEndpoint)
            {
            }

            public void ProbeOperation(ProbeMessage11 request)
            {
                base.Channel.ProbeOperation(request);
            }

            public void ResolveOperation(ResolveMessage11 request)
            {
                base.Channel.ResolveOperation(request);
            }

            public IAsyncResult BeginProbeOperation(ProbeMessage11 request, AsyncCallback callback, object state)
            {
                return base.Channel.BeginProbeOperation(request, callback, state);
            }

            public IAsyncResult BeginResolveOperation(ResolveMessage11 request, AsyncCallback callback, object state)
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
