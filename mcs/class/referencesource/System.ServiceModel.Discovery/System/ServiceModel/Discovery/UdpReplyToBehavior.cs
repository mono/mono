//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    // This behavior allows the UdpDiscoveryEndpoint to conform to the WS-Discovery specification 
    // and provide mitigation for the DDOS. 
    //
    // The Probe and Resolve request are sent multicast and are not secure. An attacker can launch 
    // a third party distributed DOS attack by setting the address of the third party in the ReplyTo
    // header of the Probe and Resolve requests. To mitigate this threat this behavior drops the 
    // message that have ReplyTo set to a value that is not annonymous by setting appropriate 
    // message filter.
    //
    // As per the WS-Discovery specification the ReplyTo header is optional, if not specified it is 
    // considered anonymous. The reply for Probe and Resolve requests whose ReplyTo header is set 
    // to anonymous value, must be sent to the transport address of the remote endpoint. 
    // This behavior obtains this transport address information from the message property and sets 
    // it in the ReplyTo header before passing the message to the higher level. The higher level 
    // discovery code simply uses the ReplyTo header to address the response.
    //
    class UdpReplyToBehavior : IEndpointBehavior, IDispatchMessageInspector, IClientMessageInspector
    {
        static EndpointAddress annonymousAddress;
        string scheme;

        public UdpReplyToBehavior(string scheme)
        {
            this.scheme = scheme;
        }

        static EndpointAddress AnnonymousAddress
        {
            get
            {
                if (annonymousAddress == null)
                {
                    annonymousAddress = new EndpointAddress(EndpointAddress.AnonymousUri);
                }

                return annonymousAddress;
            }
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDispatcher");
            }

            endpointDispatcher.AddressFilter = new UdpDiscoveryMessageFilter(endpointDispatcher.AddressFilter);
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
            if (endpointDispatcher.DispatchRuntime.CallbackClientRuntime != null)
            {
                endpointDispatcher.DispatchRuntime.CallbackClientRuntime.MessageInspectors.Add(this);
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // obtain the remote transport address information and include it in the ReplyTo
            object messageProperty = null;

            UdpAddressingState addressingState = null;

            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out messageProperty))
            {
                RemoteEndpointMessageProperty remoteEndpointProperty = messageProperty as RemoteEndpointMessageProperty;
                if (remoteEndpointProperty != null)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = this.scheme;
                    uriBuilder.Host = remoteEndpointProperty.Address;
                    uriBuilder.Port = remoteEndpointProperty.Port;

                    addressingState = new UdpAddressingState();
                    addressingState.RemoteEndpointAddress = uriBuilder.Uri;

                    OperationContext.Current.IncomingMessageHeaders.ReplyTo = AnnonymousAddress;                    
                }
            }

            NetworkInterfaceMessageProperty udpMessageProperty;
            if (NetworkInterfaceMessageProperty.TryGet(OperationContext.Current.IncomingMessageProperties, out udpMessageProperty))
            {               
                if (addressingState == null)
                {
                    addressingState = new UdpAddressingState();
                }

                addressingState.NetworkInterfaceMessageProperty = udpMessageProperty;
            }

            if (addressingState != null)
            {
                DiscoveryMessageProperty discoveryMessageProperty = new DiscoveryMessageProperty(addressingState);
                OperationContext.Current.IncomingMessageProperties[DiscoveryMessageProperty.Name] = discoveryMessageProperty;
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            object messageProperty;
            if (OperationContext.Current.OutgoingMessageProperties.TryGetValue(
                DiscoveryMessageProperty.Name, out messageProperty))
            {
                DiscoveryMessageProperty discoveryMessageProperty = messageProperty as DiscoveryMessageProperty;
                if (discoveryMessageProperty != null)
                {
                    UdpAddressingState state = discoveryMessageProperty.CorrelationState as UdpAddressingState;
                    if (state != null)
                    {
                        if (state.RemoteEndpointAddress != null)
                        {
                            AnnonymousAddress.ApplyTo(request);
                            request.Properties.Via = state.RemoteEndpointAddress;
                        }

                        if (state.NetworkInterfaceMessageProperty != null)
                        {
                            state.NetworkInterfaceMessageProperty.AddTo(request);
                        }
                    }
                }
            }

            return null;
        }

        class UdpAddressingState
        {
            Uri remoteEndpontAddress;
            NetworkInterfaceMessageProperty networkInterfaceMessageProperty;

            public Uri RemoteEndpointAddress
            {
                get
                {
                    return remoteEndpontAddress;
                }
                set
                {
                    remoteEndpontAddress = value;
                }
            }


            public NetworkInterfaceMessageProperty NetworkInterfaceMessageProperty
            {
                get
                {
                    return networkInterfaceMessageProperty;
                }
                set
                {
                    networkInterfaceMessageProperty = value;
                }
            }
        }
    }
}
