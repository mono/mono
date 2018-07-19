// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Xml;

    internal class ServerUdpOutputChannel : UdpOutputChannel
    {
        public ServerUdpOutputChannel(ChannelManagerBase factory, MessageEncoder encoder, BufferManager bufferManager, UdpSocket[] sendSockets, UdpRetransmissionSettings retransmissionSettings, Uri via, bool isMulticast)
            : base(factory, encoder, bufferManager, sendSockets, retransmissionSettings, via, isMulticast)
        {
        }

        // will either return a valid socket or will set exceptionToBeThrown
        protected UdpSocket GetSendSocket(IPAddress address, Uri destination, out Exception exceptionToBeThrown)
        {
            Fx.Assert(this.IsMulticast == false, "This overload should only be used for unicast.");

            UdpSocket result = null;
            exceptionToBeThrown = null;
            AddressFamily family = address.AddressFamily;

            lock (ThisLock)
            {
                if (this.State == CommunicationState.Opened)
                {
                    for (int i = 0; i < this.SendSockets.Length; i++)
                    {
                        if (family == this.SendSockets[i].AddressFamily)
                        {
                            result = this.SendSockets[i];
                            break;
                        }
                    }

                    if (result == null)
                    {
                        exceptionToBeThrown = new InvalidOperationException(SR.RemoteAddressUnreachableDueToIPVersionMismatch(destination));
                    }
                }
                else
                {
                    exceptionToBeThrown = CreateObjectDisposedException();
                }
            }

            return result;
        }

        // will either return a valid socket or will set exceptionToBeThrown
        protected UdpSocket GetSendSocket(int interfaceIndex, out Exception exceptionToBeThrown)
        {
            Fx.Assert(this.IsMulticast == true, "This overload should only be used for multicast.");

            UdpSocket result = null;
            exceptionToBeThrown = null;

            lock (ThisLock)
            {
                if (this.State == CommunicationState.Opened)
                {
                    for (int i = 0; i < this.SendSockets.Length; i++)
                    {
                        if (interfaceIndex == this.SendSockets[i].InterfaceIndex)
                        {
                            result = this.SendSockets[i];
                            break;
                        }
                    }

                    if (result == null)
                    {
                        exceptionToBeThrown = new InvalidOperationException(SR.UdpSendFailedInterfaceIndexMatchNotFound(interfaceIndex));
                    }
                }
                else
                {
                    exceptionToBeThrown = CreateObjectDisposedException();
                }
            }

            return result;
        }

        // Must return non-null/non-empty array unless exceptionToBeThrown is has been set
        protected override UdpSocket[] GetSendSockets(Message message, out IPEndPoint remoteEndPoint, out Exception exceptionToBeThrown)
        {
            Fx.Assert(message != null, "message can't be null");

            UdpSocket[] socketList = null;
            exceptionToBeThrown = null;

            remoteEndPoint = null;
            Uri destination;
            bool isVia = false;

            if (message.Properties.Via != null)
            {
                destination = message.Properties.Via;
                isVia = true;
            }
            else if (message.Headers.To != null)
            {
                destination = message.Headers.To;
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ToOrViaRequired));
            }

            this.ValidateDestinationUri(destination, isVia);

            if (destination.HostNameType == UriHostNameType.IPv4 || destination.HostNameType == UriHostNameType.IPv6)
            {
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(destination.DnsSafeHost), destination.Port);

                if (this.IsMulticast)
                {
                    UdpSocket socket = this.GetSendSocketUsingInterfaceIndex(message.Properties, out exceptionToBeThrown);

                    if (socket != null)
                    {
                        if (socket.AddressFamily == remoteEndPoint.AddressFamily)
                        {
                            socketList = new UdpSocket[] { socket };
                        }
                        else
                        {
                            exceptionToBeThrown = new InvalidOperationException(SR.RemoteAddressUnreachableDueToIPVersionMismatch(destination.DnsSafeHost));
                        }
                    }
                }
                else
                {
                    UdpSocket socket = this.GetSendSocket(remoteEndPoint.Address, destination, out exceptionToBeThrown);
                    if (socket != null)
                    {
                        socketList = new UdpSocket[] { socket };
                    }
                }
            }
            else
            {
                IPAddress[] remoteAddresses = DnsCache.Resolve(destination).AddressList;

                if (this.IsMulticast)
                {
                    UdpSocket socket = this.GetSendSocketUsingInterfaceIndex(message.Properties, out exceptionToBeThrown);

                    if (socket != null)
                    {
                        socketList = new UdpSocket[] { socket };

                        for (int i = 0; i < remoteAddresses.Length; i++)
                        {
                            if (remoteAddresses[i].AddressFamily == socket.AddressFamily)
                            {
                                remoteEndPoint = new IPEndPoint(remoteAddresses[i], destination.Port);
                                break;
                            }
                        }

                        if (remoteEndPoint == null)
                        {
                            // for multicast, we only listen on either IPv4 or IPv6 (not both).
                            // if we didn't find a matching remote endpoint, then it would indicate that
                            // the remote host didn't resolve to an address we can use...
                            exceptionToBeThrown = new InvalidOperationException(SR.RemoteAddressUnreachableDueToIPVersionMismatch(destination.DnsSafeHost));
                        }
                    }
                }
                else
                {
                    bool useIPv4 = true;
                    bool useIPv6 = true;

                    for (int i = 0; i < remoteAddresses.Length; i++)
                    {
                        IPAddress address = remoteAddresses[i];

                        if (address.AddressFamily == AddressFamily.InterNetwork && useIPv4)
                        {
                            UdpSocket socket = this.GetSendSocket(address, destination, out exceptionToBeThrown);
                            if (socket == null)
                            {
                                if (this.State != CommunicationState.Opened)
                                {
                                    // time to exit, the channel is closing down.
                                    break;
                                }
                                else
                                {
                                    // no matching socket on IPv4, so ignore future IPv4 addresses 
                                    // in the remoteAddresses list
                                    useIPv4 = false;
                                }
                            }
                            else
                            {
                                remoteEndPoint = new IPEndPoint(address, destination.Port);
                                socketList = new UdpSocket[] { socket };
                                break;
                            }
                        }
                        else if (address.AddressFamily == AddressFamily.InterNetworkV6 && useIPv6)
                        {
                            UdpSocket socket = this.GetSendSocket(address, destination, out exceptionToBeThrown);
                            if (socket == null)
                            {
                                if (this.State != CommunicationState.Opened)
                                {
                                    // time to exit, the channel is closing down.
                                    break;
                                }
                                else
                                {
                                    // no matching socket on IPv6, so ignore future IPv6 addresses 
                                    // in the remoteAddresses list
                                    useIPv6 = false;
                                }
                            }
                            else
                            {
                                remoteEndPoint = new IPEndPoint(address, destination.Port);
                                socketList = new UdpSocket[] { socket };
                                break;
                            }
                        }
                    }
                }
            }

            return socketList;
        }

        private UdpSocket GetSendSocketUsingInterfaceIndex(MessageProperties properties, out Exception exceptionToBeThrown)
        {
            NetworkInterfaceMessageProperty property;
            UdpSocket socket = null;
            exceptionToBeThrown = null;

            if (!NetworkInterfaceMessageProperty.TryGet(properties, out property))
            {
                if (this.SendSockets.Length > 1)
                {
                    // this property is required on all messages sent from the channel listener.
                    // the client channel does not use this method to get the send SendSockets or the 
                    // remote endpoint, so it is safe to throw...
                    exceptionToBeThrown = new InvalidOperationException(SR.NetworkInterfaceMessagePropertyMissing(typeof(NetworkInterfaceMessageProperty)));
                }
                else
                {
                    // there is only one socket, so just send it on that one.
                    socket = this.SendSockets[0];
                }
            }
            else
            {
                socket = this.GetSendSocket(property.InterfaceIndex, out exceptionToBeThrown);
            }

            return socket;
        }

        private void ValidateDestinationUri(Uri destination, bool isVia)
        {
            if (!destination.Scheme.Equals(UdpConstants.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                if (isVia)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ViaUriIsNotValid(destination, SR.UriSchemeNotSupported(destination.Scheme))));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ToAddressIsNotValid(destination, SR.UriSchemeNotSupported(destination.Scheme))));
                }
            }

            if (destination.Port < 1 || destination.Port > IPEndPoint.MaxPort)
            {
                if (isVia)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ViaUriIsNotValid(destination, SR.PortNumberInvalid(1, IPEndPoint.MaxPort))));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ToAddressIsNotValid(destination, SR.PortNumberInvalid(1, IPEndPoint.MaxPort))));
                }
            }
        }
    }
}
