//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime;

    class UdpChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
    {
        MessageEncoderFactory messageEncoderFactory;
        UdpTransportBindingElement udpTransportBindingElement;
       
        internal UdpChannelFactory(UdpTransportBindingElement transportBindingElement, BindingContext context)
            : base(context.Binding)
        {
            Fx.Assert(transportBindingElement != null, "transportBindingElement can't be null");
            Fx.Assert(context != null, "binding context can't be null");
            Fx.Assert(typeof(TChannel) == typeof(IOutputChannel) || typeof(TChannel) == typeof(IDuplexChannel), "this channel factory only supports IOutputChannel and IDuplexChannel");

            this.udpTransportBindingElement = transportBindingElement;

            // We should only throw this exception if the user specified realistic MaxReceivedMessageSize less than or equal to max message size over UDP.
            // If the user specified something bigger like Long.MaxValue, we shouldn't stop them.
            if (this.udpTransportBindingElement.MaxReceivedMessageSize <= UdpConstants.MaxMessageSizeOverIPv4  && 
                this.udpTransportBindingElement.SocketReceiveBufferSize < this.udpTransportBindingElement.MaxReceivedMessageSize)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("SocketReceiveBufferSize", this.udpTransportBindingElement.SocketReceiveBufferSize,
                    SR.Property1LessThanOrEqualToProperty2("MaxReceivedMessageSize", this.udpTransportBindingElement.MaxReceivedMessageSize,
                    "SocketReceiveBufferSize", this.udpTransportBindingElement.SocketReceiveBufferSize));
            }

            this.messageEncoderFactory = UdpUtility.GetEncoder(context);

            bool retransmissionEnabled = this.udpTransportBindingElement.RetransmissionSettings.Enabled;
            //duplicated detection doesn't apply to IOutputChannel, so don't throw if we are only sending
            bool duplicateDetectionEnabled = this.udpTransportBindingElement.DuplicateMessageHistoryLength > 0 ? typeof(TChannel) != typeof(IOutputChannel) : false;
            UdpUtility.ValidateDuplicateDetectionAndRetransmittionSupport(this.messageEncoderFactory, retransmissionEnabled, duplicateDetectionEnabled);

            int maxBufferSize = (int)Math.Min(transportBindingElement.MaxReceivedMessageSize, UdpConstants.MaxMessageSizeOverIPv4);
            this.BufferManager = BufferManager.CreateBufferManager(transportBindingElement.MaxBufferPoolSize, maxBufferSize);

        }

        BufferManager BufferManager
        {
            get;
            set;
        }

        public override T GetProperty<T>()
        {
            T messageEncoderProperty = this.messageEncoderFactory.Encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)this.messageEncoderFactory.Encoder.MessageVersion;
            }

            return base.GetProperty<T>();
        }


        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override TChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            Fx.Assert(to != null, "To address should have been validated as non-null by ChannelFactoryBase");
            Fx.Assert(via != null, "Via address should have been validated as non-null by ChannelFactoryBase");

            if (!via.IsAbsoluteUri)
            {
                throw FxTrace.Exception.Argument("via", SR.RelativeUriNotAllowed(via));
            }

            if (!via.Scheme.Equals(UdpConstants.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw FxTrace.Exception.Argument("via", SR.UriSchemeNotSupported(via.Scheme));
            }

            if (!UdpUtility.IsSupportedHostNameType(via.HostNameType))
            {
                throw FxTrace.Exception.Argument("via", SR.UnsupportedUriHostNameType(via.Host, via.HostNameType));
            }

            if (via.IsDefaultPort || via.Port == 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("via", via, SR.PortNumberRequiredOnVia(via));
            }

            UdpSocket[] sockets = null;
            IPEndPoint remoteEndPoint = null;
            TChannel channel;

            lock (this.ThisLock)
            {
                bool isMulticast;
                sockets = GetSockets(via, out remoteEndPoint, out isMulticast);

                EndpointAddress localAddress = new EndpointAddress(EndpointAddress.AnonymousUri);

                if (typeof(TChannel) == typeof(IDuplexChannel))
                {
                    UdpChannelFactory<IDuplexChannel> duplexChannelFactory = (UdpChannelFactory<IDuplexChannel>)(object)this;
                    channel = (TChannel)(object)new ClientUdpDuplexChannel(duplexChannelFactory, sockets, remoteEndPoint, localAddress, to, via, isMulticast);
                }
                else
                {
                    UdpChannelFactory<IOutputChannel> outputChannelFactory = (UdpChannelFactory<IOutputChannel>)(object)this;
                    channel = (TChannel)(object)new ClientUdpOutputChannel(
                        outputChannelFactory,
                        remoteEndPoint,
                        outputChannelFactory.messageEncoderFactory.Encoder, 
                        this.BufferManager, 
                        sockets, 
                        outputChannelFactory.udpTransportBindingElement.RetransmissionSettings,
                        to,
                        via,
                        isMulticast);
                }
            }

            return channel;
        }
        
        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {

        }

        //will only return > 1 socket when both of the following are true:
        // 1) multicast 
        // 2) sending on all interfaces
        UdpSocket[] GetSockets(Uri via, out IPEndPoint remoteEndPoint, out bool isMulticast)
        {
            UdpSocket[] results = null;

            remoteEndPoint = null;
            IPAddress[] remoteAddressList;
            isMulticast = false;

            UdpUtility.ThrowIfNoSocketSupport();
           
            if (via.HostNameType == UriHostNameType.IPv6 || via.HostNameType == UriHostNameType.IPv4)
            {
                UdpUtility.ThrowOnUnsupportedHostNameType(via);

                IPAddress address = IPAddress.Parse(via.DnsSafeHost);
                isMulticast = UdpUtility.IsMulticastAddress(address);

                remoteAddressList = new IPAddress[] { address };
            }
            else
            {
                remoteAddressList = DnsCache.Resolve(via).AddressList;
            }

            if (remoteAddressList.Length < 1)
            {
                // System.Net.Dns shouldn't ever allow this to happen, but...
                Fx.Assert("DnsCache returned a HostEntry with zero length address list");
                throw FxTrace.Exception.AsError(new EndpointNotFoundException(SR.DnsResolveFailed(via.DnsSafeHost)));
            }

            remoteEndPoint = new IPEndPoint(remoteAddressList[0], via.Port);

            IPAddress localAddress;
            if (via.IsLoopback)
            {
                localAddress = (remoteEndPoint.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Loopback : IPAddress.IPv6Loopback);
            }
            else
            {
                localAddress = (remoteEndPoint.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any);
            }

            int port = 0;

            if (isMulticast)
            {
                List<UdpSocket> socketList = new List<UdpSocket>();
                NetworkInterface[] adapters = UdpUtility.GetMulticastInterfaces(this.udpTransportBindingElement.MulticastInterfaceId);

                //if listening on a specific adapter, don't disable multicast loopback on that adapter.
                bool allowMulticastLoopback = !string.IsNullOrEmpty(this.udpTransportBindingElement.MulticastInterfaceId);

                for (int i = 0; i < adapters.Length; i++)
                {
                    if (adapters[i].OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties properties = adapters[i].GetIPProperties();
                        bool isLoopbackAdapter = adapters[i].NetworkInterfaceType == NetworkInterfaceType.Loopback;

                        if (isLoopbackAdapter)
                        {
                            int interfaceIndex;
                            if (UdpUtility.TryGetLoopbackInterfaceIndex(adapters[i], localAddress.AddressFamily == AddressFamily.InterNetwork, out interfaceIndex))
                            {
                                socketList.Add(UdpUtility.CreateListenSocket(localAddress, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive,
                                    interfaceIndex, allowMulticastLoopback, isLoopbackAdapter));
                            }

                        }
                        else if (localAddress.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            if (adapters[i].Supports(NetworkInterfaceComponent.IPv6))
                            {
                                IPv6InterfaceProperties v6Properties = properties.GetIPv6Properties();

                                if (v6Properties != null)
                                {
                                    socketList.Add(UdpUtility.CreateListenSocket(localAddress, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize,
                                        this.udpTransportBindingElement.TimeToLive, v6Properties.Index, allowMulticastLoopback, isLoopbackAdapter));
                                }
                            }
                        }
                        else
                        {
                            if (adapters[i].Supports(NetworkInterfaceComponent.IPv4))
                            {
                                IPv4InterfaceProperties v4Properties = properties.GetIPv4Properties();
                                if (v4Properties != null)
                                {
                                    socketList.Add(UdpUtility.CreateListenSocket(localAddress, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize,
                                        this.udpTransportBindingElement.TimeToLive, v4Properties.Index, allowMulticastLoopback, isLoopbackAdapter));
                                }
                            }
                        }
                    }

                    //CreateListenSocket sets the port, but since we aren't listening
                    //on multicast, each socket can't share the same port.  
                    port = 0;
                }
                
                if (socketList.Count == 0)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.UdpFailedToFindMulticastAdapter(via)));
                }

                results = socketList.ToArray();
            }
            else
            {
                UdpSocket socket = UdpUtility.CreateUnicastListenSocket(localAddress, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize,
                    this.udpTransportBindingElement.TimeToLive);

                results = new UdpSocket[] { socket };
            }


            Fx.Assert(results != null, "GetSockets(...) return results should never be null. An exception should have been thrown but wasn't.");
            return results;

        }

        sealed class ClientUdpDuplexChannel : UdpDuplexChannel
        {
            EndpointAddress to;
            ChannelParameterCollection channelParameters;

            internal ClientUdpDuplexChannel(UdpChannelFactory<IDuplexChannel> factory, UdpSocket[] sockets, IPEndPoint remoteEndPoint, EndpointAddress localAddress, EndpointAddress to, Uri via, bool isMulticast)
                : base(factory,
                factory.messageEncoderFactory.Encoder,
                factory.BufferManager,
                sockets,
                factory.udpTransportBindingElement.RetransmissionSettings,
                factory.udpTransportBindingElement.MaxPendingMessagesTotalSize,
                localAddress,
                via,
                isMulticast,
                (int)factory.udpTransportBindingElement.MaxReceivedMessageSize)
            {
                Fx.Assert(to != null, "to address can't be null for this constructor...");
                Fx.Assert(remoteEndPoint != null, "remoteEndPoint can't be null");

                this.RemoteEndPoint = remoteEndPoint;
                this.to = to;

                if (factory.udpTransportBindingElement.DuplicateMessageHistoryLength > 0)
                {
                    this.DuplicateDetector = new DuplicateMessageDetector(factory.udpTransportBindingElement.DuplicateMessageHistoryLength);
                }
                else
                {
                    this.DuplicateDetector = null;
                }

                UdpOutputChannel udpOutputChannel = new ClientUdpOutputChannel(factory, remoteEndPoint, factory.messageEncoderFactory.Encoder, factory.BufferManager, sockets, factory.udpTransportBindingElement.RetransmissionSettings, to, via, isMulticast);
                this.SetOutputChannel(udpOutputChannel);
            }

            protected override bool IgnoreSerializationException
            {
                get
                {
                    return this.IsMulticast;
                }
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    return this.to;
                }
            }

            public IPEndPoint RemoteEndPoint
            {
                get;
                private set;
            }

            public override T GetProperty<T>()
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    if (this.State == CommunicationState.Created)
                    {
                        lock (ThisLock)
                        {
                            if (this.channelParameters == null)
                            {
                                this.channelParameters = new ChannelParameterCollection();
                            }
                        }
                    }
                    return (T)(object)this.channelParameters;
                }
                else
                {
                    return base.GetProperty<T>();
                }
            }

            protected override void OnOpened()
            {
                this.ReceiveManager = new UdpSocketReceiveManager(this.Sockets,
                    UdpConstants.PendingReceiveCountPerProcessor * Environment.ProcessorCount,
                    base.BufferManager,
                    this);

                //do the state change to CommunicationState.Opened before starting the receive loop.
                //this avoids a ---- between transitioning state and processing messages that are
                //already in the socket receive buffer.
                base.OnOpened();

                this.ReceiveManager.Open();
            }

            protected override void AddHeadersTo(Message message)
            {
                Fx.Assert(message != null, "Message can't be null");

                if (message.Version.Addressing != AddressingVersion.None)
                {
                    this.to.ApplyTo(message);
                }

                message.Properties.Via = this.Via;

                base.AddHeadersTo(message);
            }
        }
    }
}
