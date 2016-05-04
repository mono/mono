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
    using System.Xml;

    static class UdpUtility
    {
        public static Uri AppendRelativePath(Uri basepath, string relativePath)
        {
            // Ensure that baseAddress Path does end with a slash if we have a relative address
            if (!string.IsNullOrEmpty(relativePath))
            {
                if (!basepath.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
                {
                    UriBuilder uriBuilder = new UriBuilder(basepath);
                    uriBuilder.Path = uriBuilder.Path + "/";
                    basepath = uriBuilder.Uri;
                }

                basepath = new Uri(basepath, relativePath);
            }

            return basepath;
        }

        public static MessageEncoderFactory GetEncoder(BindingContext context)
        {
            MessageEncodingBindingElement messageEncoderBindingElement = context.BindingParameters.Remove<MessageEncodingBindingElement>();
            MessageEncoderFactory factory = null;
            if (messageEncoderBindingElement != null)
            {
                factory = messageEncoderBindingElement.CreateMessageEncoderFactory();
            }
            else
            {
                factory = UdpConstants.Defaults.MessageEncoderFactory;
            }

            return factory;
        }

        //there are some errors on the server side that we should just ignore because the server will not need
        //to change its behavior if it sees the exception.  These errors are not ignored on the client 
        //because it may need to adjust settings (e.g. TTL, send smaller messages, double check server address for correctness)
        internal static bool CanIgnoreServerException(Exception ex)
        {
            SocketError error;
            if (UdpUtility.TryGetSocketError(ex, out error))
            {
                switch (error)
                {
                    case SocketError.ConnectionReset: //"ICMP Destination Unreachable" error - client closed the socket
                    case SocketError.NetworkReset: //ICMP: Time Exceeded (TTL expired)
                    case SocketError.MessageSize: //client sent a message larger than the max message size allowed.
                        return true;
                }
            }
            return false;
        }


        public static void CheckSocketSupport(out bool ipV4Supported, out bool ipV6Supported)
        {
            ipV4Supported = Socket.OSSupportsIPv4;
            ipV6Supported = Socket.OSSupportsIPv6;

            ThrowIfNoSocketSupport(ipV4Supported, ipV6Supported);
        }

        public static bool TryGetLoopbackInterfaceIndex(NetworkInterface adapter, bool ipv4, out int interfaceIndex)
        {
            Fx.Assert(adapter != null, "adapter can't be null");
            Fx.Assert(adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback, "adapter type must be loopback adapter");

            interfaceIndex = -1;
            bool result = false;

            if (ipv4 && adapter.Supports(NetworkInterfaceComponent.IPv4))
            {
                interfaceIndex = NetworkInterface.LoopbackInterfaceIndex;
                result = true;
            }
            else if (!ipv4 && adapter.Supports(NetworkInterfaceComponent.IPv6))
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                IPv6InterfaceProperties ipv6Properties = properties.GetIPv6Properties();
                interfaceIndex = ipv6Properties.Index;
                result = true;
            }
            return result;
        }

        public static UdpSocket CreateUnicastListenSocket(IPAddress ipAddress, ref int port, int receiveBufferSize, int timeToLive)
        {
            return CreateListenSocket(ipAddress, ref port, receiveBufferSize, timeToLive, UdpConstants.Defaults.InterfaceIndex, false, false);
        }

        public static UdpSocket CreateListenSocket(IPAddress ipAddress, ref int port, int receiveBufferSize, int timeToLive,
            int interfaceIndex, bool allowMulticastLoopback, bool isLoopbackAdapter)
        {
            bool isIPv6 = (ipAddress.AddressFamily == AddressFamily.InterNetworkV6);
            Socket socket = null;

            bool listenMulticast = IsMulticastAddress(ipAddress);

            IPEndPoint localEndpoint;

            if (listenMulticast)
            {
                IPAddress bindAddress = (isIPv6 ? IPAddress.IPv6Any : IPAddress.Any);
                localEndpoint = new IPEndPoint(bindAddress, port);
            }
            else
            {
                localEndpoint = new IPEndPoint(ipAddress, port);
            }

            socket = new Socket(localEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            SetPreBindSocketOptions(socket, listenMulticast, receiveBufferSize, (short)timeToLive, interfaceIndex, allowMulticastLoopback, isLoopbackAdapter);

            BindSocket(socket, localEndpoint);

            SetPostBindSocketOptions(socket, listenMulticast, ipAddress, interfaceIndex);

            if (port == 0)
            {
                //update the port to be the actual one the socket is bound to...
                port = ((IPEndPoint)socket.LocalEndPoint).Port;
            }


            return new UdpSocket(socket, interfaceIndex);
        }

        //returns the port number used...
        public static int CreateListenSocketsOnUniquePort(IPAddress ipv4Address, IPAddress ipv6Address, int receiveBufferSize, int timeToLive, out UdpSocket ipv4Socket, out UdpSocket ipv6Socket)
        {
            // We need both IPv4 and IPv6 on the same port. We can't atomicly bind for IPv4 and IPv6, 
            // so we try 10 times, which even with a 50% failure rate will statistically succeed 99.9% of the time.
            //
            // We look in the range of 49152-65534 for Vista default behavior parity.
            // http://www.iana.org/assignments/port-numbers
            // 
            // We also grab the 10 random numbers in a row to reduce collisions between multiple people somehow
            // colliding on the same seed.
            const int retries = 10;
            const int lowWatermark = 49152;
            const int highWatermark = 65535;

            ipv4Socket = null;
            ipv6Socket = null;

            int[] portNumbers = new int[retries];

            Random randomNumberGenerator = new Random(AppDomain.CurrentDomain.GetHashCode() | Environment.TickCount);

            for (int i = 0; i < retries; i++)
            {
                portNumbers[i] = randomNumberGenerator.Next(lowWatermark, highWatermark);
            }


            int port = -1;
            for (int i = 0; i < retries; i++)
            {
                port = portNumbers[i];
                try
                {
                    ipv4Socket = UdpUtility.CreateUnicastListenSocket(ipv4Address, ref port, receiveBufferSize, timeToLive);
                    ipv6Socket = UdpUtility.CreateUnicastListenSocket(ipv6Address, ref port, receiveBufferSize, timeToLive);
                    break;
                }
                catch (AddressAlreadyInUseException)
                {
                    if (ipv4Socket != null)
                    {
                        ipv4Socket.Close();
                        ipv4Socket = null;
                    }
                    ipv6Socket = null;
                }
                catch (AddressAccessDeniedException)
                {
                    if (ipv4Socket != null)
                    {
                        ipv4Socket.Close();
                        ipv4Socket = null;
                    }
                    ipv6Socket = null;
                }
            }

            if (ipv4Socket == null)
            {
                throw FxTrace.Exception.AsError(new AddressAlreadyInUseException(SR.UniquePortNotAvailable));
            }

            Fx.Assert(ipv4Socket != null, "An exception should have been thrown if the ipv4Socket socket is null");
            Fx.Assert(ipv6Socket != null, "An exception should have been thrown if the ipv6Socket socket is null");
            Fx.Assert(port > 0, "The port number should have been greater than 0. Actual value was " + port);

            return port;
        }

        public static void ValidateDuplicateDetectionAndRetransmittionSupport(MessageEncoderFactory messageEncoderFactory, bool retransmissionEnabled, bool duplicateDetectionEnabled)
        {
            Fx.Assert(messageEncoderFactory != null, "messageEncoderFactory shouldn't be null");
            
            MessageVersion encoderMessageVersion = messageEncoderFactory.MessageVersion;

            if (encoderMessageVersion.Addressing == AddressingVersion.None)
            {
                if (retransmissionEnabled)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TransportRequiresAddressingOnEncoderForRetransmission(encoderMessageVersion, "RetransmissionSettings", typeof(UdpTransportBindingElement).Name)));
                }

                if (duplicateDetectionEnabled)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TransportRequiresAddressingOnEncoderForDuplicateDetection(encoderMessageVersion, "DuplicateMessageHistoryLength", typeof(UdpTransportBindingElement).Name)));
                }
            }
        }

        public static void ThrowIfNoSocketSupport()
        {
            ThrowIfNoSocketSupport(Socket.OSSupportsIPv4, Socket.OSSupportsIPv6);
        }

        public static void ThrowOnUnsupportedHostNameType(Uri uri)
        {
            bool ipV4, ipV6;
            CheckSocketSupport(out ipV4, out ipV6);
            if (!ipV4 && uri.HostNameType == UriHostNameType.IPv4 || !ipV6 && uri.HostNameType == UriHostNameType.IPv6)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.UriHostNameTypeNotSupportedByOS(uri.Host, uri.HostNameType.ToString())));
            }
        }

        static void ThrowIfNoSocketSupport(bool ipv4Supported, bool ipv6Supported)
        {
            if (!ipv4Supported && !ipv6Supported)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.IPv4OrIPv6Required));
            }
        }

        public static NetworkInterface[] GetMulticastInterfaces(string multicastInterfaceIdentifier)
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            
            Fx.Assert(adapters != null, "NetworkInterface.GetAllNetworkInterfaces() should never return null");

            NetworkInterface[] results = null;

            if (string.IsNullOrEmpty(multicastInterfaceIdentifier)) //find all supported NICs
            {
                List<NetworkInterface> supportedAdapters = new List<NetworkInterface>();

                for (int i = 0; i < adapters.Length; i++)
                {
                    NetworkInterface adapter = adapters[i];

                    if (IsSuitableForMulticast(adapter))
                    {
                        supportedAdapters.Add(adapter);
                    }
                }

                //OK to return an empty array in this case, the calling code will throw an exception
                //with better context information that what we have here...
                results = supportedAdapters.ToArray();
            }
            else  //Only looking for one interface...
            {
                for (int i = 0; i < adapters.Length; i++)
                {
                    NetworkInterface adapter = adapters[i];

                    if (string.Equals(adapter.Id, multicastInterfaceIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsSuitableForMulticast(adapter))
                        {
                            OperationalStatus status = adapter.OperationalStatus;
                            if (status != OperationalStatus.Up)
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UdpAdapterSpecifiedNotConnected(multicastInterfaceIdentifier, status)));
                            }

                            results = new NetworkInterface[] { adapter };
                            break;
                        }
                        else
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UdpAdapterSpecifiedNotSuitableForMulticast(multicastInterfaceIdentifier)));
                        }
                    }
                }

                if (results == null || results.Length == 0)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UdpInterfaceIndexMatchNotFound(multicastInterfaceIdentifier)));
                }
            }

            Fx.Assert(results != null, "A null list of network adapters should never be returned.  It should either be an empty list or an exception should have been thrown.");
            return results;
        }

        public static bool IsMulticastAddress(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                // 224.0.0.0 through 239.255.255.255
                int firstAddressByte = (int)address.GetAddressBytes()[0];
                return (firstAddressByte >= 224 && firstAddressByte <= 239); 
            }
            else
            {
                return address.IsIPv6Multicast;
            }
        }

        public static bool IsSuitableForMulticast(NetworkInterface networkInterface)
        {
            bool result = false;

            if (networkInterface.SupportsMulticast &&
                !networkInterface.IsReceiveOnly &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                networkInterface.NetworkInterfaceType != NetworkInterfaceType.Unknown)
            {
                result = true;
            }

            return result;
        }

        public static bool IsSupportedHostNameType(UriHostNameType hostNameType)
        {
            return hostNameType == UriHostNameType.Dns ||
                hostNameType == UriHostNameType.IPv4 ||
                hostNameType == UriHostNameType.IPv6;
        }

        public static void ValidateBufferBounds(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw FxTrace.Exception.ArgumentNull("buffer");
            }

            ValidateBufferBounds(buffer.Length, offset, size);
        }

        public static void ValidateBufferBounds(int bufferSize, int offset, int size)
        {
            if (offset < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("offset", offset, SR.ValueMustBeNonNegative(offset));
            }

            if (offset > bufferSize)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("offset", offset, SR.OffsetExceedsBufferSize(bufferSize));
            }

            if (size <= 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("size", size, SR.ValueMustBePositive);
            }

            int remainingBufferSpace = bufferSize - offset;
            if (size > remainingBufferSpace)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("size", size, SR.SizeExceedsRemainingBufferSpace(remainingBufferSpace));
            }
        }

        public static Exception WrapAsyncException(Exception ex)
        {
            if (ex is TimeoutException)
            {
                return new TimeoutException(SR.AsynchronousExceptionOccurred, ex);
            }
            else if (ex is AddressAlreadyInUseException)
            {
                return new AddressAlreadyInUseException(SR.AsynchronousExceptionOccurred, ex);
            }
            else if (ex is AddressAccessDeniedException)
            {
                return new AddressAccessDeniedException(SR.AsynchronousExceptionOccurred, ex);
            }
            else if (ex is EndpointNotFoundException)
            {
                return new EndpointNotFoundException(SR.AsynchronousExceptionOccurred, ex);
            }
            else if (ex is XmlException)
            {
                return new XmlException(SR.AsynchronousExceptionOccurred, ex);
            }
            else if (ex is CommunicationException)
            {
                return new CommunicationException(SR.AsynchronousExceptionOccurred, ex);
            }
            else
            {
                return ex;
            }
        }

        static void BindSocket(Socket socket, IPEndPoint localEndpoint)
        {
            try
            {
                socket.Bind(localEndpoint);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    throw FxTrace.Exception.AsError(new AddressAlreadyInUseException(SR.SocketAddressInUse(localEndpoint.ToString()), ex));
                }
                else if (ex.SocketErrorCode == SocketError.AccessDenied)
                {
                    throw FxTrace.Exception.AsError(new AddressAccessDeniedException(SR.SocketAddressAccessDenied(localEndpoint.ToString()), ex));
                }
                else
                {
                    throw;
                }
            }
        }

        static void SetPreBindSocketOptions(Socket socket, bool listenMulticast, int receiveBufferSize, short timeToLive, int interfaceIndex, 
            bool allowMulticastLoopback, bool isLoopbackAdapter)
        {
            bool isIPv4 = socket.AddressFamily == AddressFamily.InterNetwork;
            SocketOptionLevel ipOptionLevel = (isIPv4 ? ipOptionLevel = SocketOptionLevel.IP : ipOptionLevel = SocketOptionLevel.IPv6);

            //sets only the unicast TTL
            socket.Ttl = timeToLive;

            //set send related multicast options even if not listening multicast,
            //we might be sending multicast (e.g. client side or due to manual addressing on server).
            socket.SetSocketOption(ipOptionLevel, SocketOptionName.MulticastTimeToLive, timeToLive);

            if (interfaceIndex != UdpConstants.Defaults.InterfaceIndex)
            {
                int index = (isIPv4 ? IPAddress.HostToNetworkOrder(interfaceIndex) : interfaceIndex);

                //sets the outbound interface index
                socket.SetSocketOption(ipOptionLevel, SocketOptionName.MulticastInterface, index);
            }

            if (listenMulticast)
            {
                //don't set this socket option if the socket is bound to the 
                //loopback adapter because it will throw an argument exception.
                if (!isLoopbackAdapter)
                {
                    socket.SetSocketOption(ipOptionLevel, SocketOptionName.MulticastLoopback, allowMulticastLoopback);
                }

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            }
            else
            {
                socket.ExclusiveAddressUse = true;
            }

            if (receiveBufferSize >= 0)
            {
                socket.ReceiveBufferSize = receiveBufferSize;
            }
        }

        static void SetPostBindSocketOptions(Socket socket, bool listenMulticast, IPAddress ipAddress, int interfaceIndex)
        {
            bool isIPv6 = socket.AddressFamily == AddressFamily.InterNetworkV6;
                        
            if (listenMulticast)
            {
                //Win2k3 requires that the joining of the multicast group be after the socket is bound (not true on Vista).
                if (isIPv6)
                {
                    IPv6MulticastOption multicastGroup = (interfaceIndex == UdpConstants.Defaults.InterfaceIndex ?
                        new IPv6MulticastOption(ipAddress)
                        : new IPv6MulticastOption(ipAddress, interfaceIndex));

                    socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
                        multicastGroup);
                }
                else
                {
                    MulticastOption multicastGroup = (interfaceIndex == UdpConstants.Defaults.InterfaceIndex ?
                        new MulticastOption(ipAddress)
                        : new MulticastOption(ipAddress, interfaceIndex));

                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                        multicastGroup);
                }
            }
        }

        public static bool TryGetSocketError(Exception ex, out SocketError error)
        {
            error = SocketError.SocketError;

            while (ex != null)
            {
                SocketException socketException = ex as SocketException;
                if (socketException != null)
                {
                    error = socketException.SocketErrorCode;
                    return true;
                }

                ex = ex.InnerException;
            }

            return false;
        }

        public static Message DecodeMessage(DuplicateMessageDetector duplicateDetector, MessageEncoder encoder, BufferManager bufferManager, ArraySegment<byte> data, IPEndPoint remoteEndPoint, int interfaceIndex, bool ignoreSerializationException, out string messageHash)
        {
            Fx.Assert(data != null, "data can't be null");
            Fx.Assert(remoteEndPoint != null, "remoteEndPoint can't be null");
            Fx.Assert(encoder != null, "encoder can't be null");
            Fx.Assert(bufferManager != null, "bufferManager can't be null");
            
            Message message = null;

            messageHash = null;

            if (duplicateDetector == null  || !duplicateDetector.IsDuplicate(data, out messageHash))
            {
                try
                {
                    message = encoder.ReadMessage(data, bufferManager);
                }
                catch (XmlException error)
                {
                    // Don't throw serialization exceptions when the channel supports Multicast
                    if (!ignoreSerializationException)
                    {
                        throw;
                    }

                    FxTrace.Exception.AsWarning(error);
                }

                if (message != null)
                {
                    message.Properties.Add(RemoteEndpointMessageProperty.Name,
                        new RemoteEndpointMessageProperty(remoteEndPoint.Address.ToString(), remoteEndPoint.Port));

                    NetworkInterfaceMessageProperty networkInterfaceMessageProperty = new NetworkInterfaceMessageProperty(interfaceIndex);
                    networkInterfaceMessageProperty.AddTo(message);
                }
            }

            return message;
        }

        public static int ComputeMessageBufferSize(int maxReceivedMessageSize)
        {
            return Math.Min(UdpConstants.MaxMessageSizeOverIPv4, maxReceivedMessageSize);
        }
    }
}
