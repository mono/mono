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
    using System.ServiceModel.Description;
    using System.Threading;

    internal abstract class UdpChannelListener<ChannelInterfaceType, TChannel, QueueItemType>
        : ChannelListenerBase<ChannelInterfaceType>, IUdpReceiveHandler
        where ChannelInterfaceType : class, IChannel
        where TChannel : UdpChannelBase<QueueItemType>, ChannelInterfaceType
        where QueueItemType : class, IDisposable
    {
        BufferManager bufferManager;
        TChannel channelInstance;
        InputQueue<TChannel> channelQueue;
        DuplicateMessageDetector duplicateDetector;
        bool isMulticast;
        List<UdpSocket> listenSockets;
        Uri listenUri;
        MessageEncoderFactory messageEncoderFactory;
        EventHandler onChannelClosed;
        UdpTransportBindingElement udpTransportBindingElement;
        UdpSocketReceiveManager socketReceiveManager;
        int cleanedUp;

        internal UdpChannelListener(UdpTransportBindingElement udpTransportBindingElement, BindingContext context)
            : base(context.Binding)
        {
            Fx.Assert(udpTransportBindingElement != null, "udpTransportBindingElement can't be null");
            Fx.Assert(context != null, "BindingContext parameter can't be null");

            this.udpTransportBindingElement = udpTransportBindingElement;
            this.cleanedUp = 0;

            this.duplicateDetector = null;

            if (udpTransportBindingElement.DuplicateMessageHistoryLength > 0)
            {
                this.duplicateDetector = new DuplicateMessageDetector(udpTransportBindingElement.DuplicateMessageHistoryLength);
            }

            this.onChannelClosed = new EventHandler(OnChannelClosed);

            // We should only throw this exception if the user specified realistic MaxReceivedMessageSize less than or equal to max message size over UDP.
            // If the user specified something bigger like Long.MaxValue, we shouldn't stop them.
            if (this.udpTransportBindingElement.MaxReceivedMessageSize <= UdpConstants.MaxMessageSizeOverIPv4 &&
                this.udpTransportBindingElement.SocketReceiveBufferSize < this.udpTransportBindingElement.MaxReceivedMessageSize)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("SocketReceiveBufferSize", this.udpTransportBindingElement.SocketReceiveBufferSize,
                    SR.Property1LessThanOrEqualToProperty2("MaxReceivedMessageSize", this.udpTransportBindingElement.MaxReceivedMessageSize,
                    "SocketReceiveBufferSize", this.udpTransportBindingElement.SocketReceiveBufferSize));
            }


            int maxBufferSize = (int)Math.Min(udpTransportBindingElement.MaxReceivedMessageSize, UdpConstants.MaxMessageSizeOverIPv4);
            this.bufferManager = BufferManager.CreateBufferManager(udpTransportBindingElement.MaxBufferPoolSize, maxBufferSize);

            this.messageEncoderFactory = UdpUtility.GetEncoder(context);

            UdpUtility.ValidateDuplicateDetectionAndRetransmittionSupport(this.messageEncoderFactory, this.udpTransportBindingElement.RetransmissionSettings.Enabled, this.udpTransportBindingElement.DuplicateMessageHistoryLength > 0);
            
            InitUri(context);

            //Note: because we are binding the sockets in InitSockets, we can start receiving data immediately.
            //If there is a delay between the Building of the listener and the call to Open, stale data could build up
            //inside the Winsock buffer.  We have decided that making sure the port is updated correctly in the listen uri 
            //(e.g. in the ListenUriMode.Unique case) before leaving the build step is more important than the 
            //potential for stale data.
            InitSockets(context.ListenUriMode == ListenUriMode.Unique);

            Fx.Assert(!this.listenUri.IsDefaultPort, "Listen Uri's port should never be the default port: " + this.listenUri);
        }

        public MessageEncoderFactory MessageEncoderFactory
        {
            get
            {
                return this.messageEncoderFactory;
            }
        }

        public override Uri Uri
        {
            get
            {
                return this.listenUri;
            }
        }

        protected override TimeSpan DefaultReceiveTimeout
        {
            get
            {
                return UdpConstants.Defaults.ReceiveTimeout;
            }
        }

        protected override TimeSpan DefaultSendTimeout
        {
            get
            {
                return UdpConstants.Defaults.SendTimeout;
            }
        }

        internal BufferManager BufferManager
        {
            get { return this.bufferManager; }
        }

        internal UdpTransportBindingElement UdpTransportBindingElement
        {
            get { return this.udpTransportBindingElement; }
        }

        internal List<UdpSocket> ListenSockets
        {
            get { return this.listenSockets; }
        }

        internal bool IsMulticast
        {
            get { return this.isMulticast; }
        }

        int IUdpReceiveHandler.MaxReceivedMessageSize
        {
            get { return (int)this.udpTransportBindingElement.MaxReceivedMessageSize; }
        }

        string Scheme
        {
            get
            {
                return UdpConstants.Scheme;
            }
        }

        public override T GetProperty<T>()
        {
            T messageEncoderProperty = this.MessageEncoderFactory.Encoder.GetProperty<T>();
            if (messageEncoderProperty != null)
            {
                return messageEncoderProperty;
            }

            if (typeof(T) == typeof(MessageVersion))
            {
                return (T)(object)this.MessageEncoderFactory.Encoder.MessageVersion;
            }
            return base.GetProperty<T>();
        }

        void IUdpReceiveHandler.HandleAsyncException(Exception ex)
        {
            HandleReceiveException(ex);
        }

        //returns false if the message was dropped because the max pending message count was hit.
        bool IUdpReceiveHandler.HandleDataReceived(ArraySegment<byte> data, EndPoint remoteEndpoint, int interfaceIndex, Action onMessageDequeuedCallback)
        {
            BufferManager localBufferManager = this.bufferManager;
            bool returnBuffer = true;
            string messageHash = null;
            Message message = null;
            bool continueReceiving = true;

            try
            {
                IPEndPoint remoteIPEndPoint = (IPEndPoint)remoteEndpoint;
                if (localBufferManager != null)
                {
                    message = UdpUtility.DecodeMessage(this.duplicateDetector, this.messageEncoderFactory.Encoder,
                        localBufferManager, data, remoteIPEndPoint, interfaceIndex, true, out messageHash);

                    if (message != null)
                    {
                        // We pass in the length of the message buffer instead of the length of the message to keep track of the amount of memory that's been allocated
                        continueReceiving = Dispatch(message, data.Array.Length, onMessageDequeuedCallback);
                        returnBuffer = !continueReceiving;
                    }
                }
                else
                {
                    Fx.Assert(this.State != CommunicationState.Opened, "buffer manager should only be null when closing down and the channel instance has taken control of the receive manager.");

                    IUdpReceiveHandler receiveHandler = (IUdpReceiveHandler)this.channelInstance;

                    if (receiveHandler != null)
                    {
                        returnBuffer = false; //let the channel instance take care of the buffer
                        continueReceiving = receiveHandler.HandleDataReceived(data, remoteEndpoint, interfaceIndex, onMessageDequeuedCallback);
                    }
                    else
                    {
                        //both channel and listener are shutting down, so drop the message and stop the receive loop
                        continueReceiving = false;
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    returnBuffer = false;
                    throw;
                }

                HandleReceiveException(e);
            }
            finally
            {
                if (returnBuffer)
                {
                    if (message != null)
                    {
                        if (this.duplicateDetector != null)
                        {
                            Fx.Assert(messageHash != null, "message hash should always be available if duplicate detector is enabled");
                            this.duplicateDetector.RemoveEntry(messageHash);
                        }

                        message.Close(); // implicitly returns the buffer
                    }
                    else
                    {
                        // CSDMain 238600. Both channel and listener are shutting down. There's a race condition happening here 
                        // and the bufferManager is not available at this moment. The data buffer ignored here might introduce
                        // an issue with buffer manager, but given that we are in the shutting down case here, it should not be a 
                        // big problem.
                        if (localBufferManager != null)
                        {
                            localBufferManager.ReturnBuffer(data.Array);
                        }
                    }
                }
            }

            return continueReceiving;
        }

        protected override Type GetCommunicationObjectType()
        {
            return this.GetType();
        }


        protected override void OnAbort()
        {
            Cleanup();
        }

        protected override ChannelInterfaceType OnAcceptChannel(TimeSpan timeout)
        {
            ThrowPending();

            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return this.channelQueue.Dequeue(timeout);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowPending();
            TimeoutHelper.ThrowIfNegativeArgument(timeout);


            return this.channelQueue.BeginDequeue(timeout, callback, state);
        }


        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnClose(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowPending();

            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return this.channelQueue.BeginWaitForItem(timeout, callback, state);
        }

        protected override void OnClosing()
        {
            if (this.channelInstance != null)
            {
                lock (ThisLock)
                {
                    if (this.channelInstance != null)
                    {
                        if (this.channelInstance.TransferReceiveManagerOwnership(this.socketReceiveManager,
                            this.duplicateDetector))
                        {
                            //don't clean these objects up, they now belong to the channel instance
                            this.socketReceiveManager = null;
                            this.duplicateDetector = null;
                            this.bufferManager = null;
                        }
                    }

                    this.channelInstance = null;
                }
            }

            base.OnClosing();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            Cleanup();
        }

        protected override ChannelInterfaceType OnEndAcceptChannel(IAsyncResult result)
        {
            TChannel channel;
            if (this.channelQueue.EndDequeue(result, out channel))
            {
                return channel;
            }
            else
            {
                throw FxTrace.Exception.AsError(new TimeoutException());
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.channelQueue.EndWaitForItem(result);
        }


        protected override void OnOpen(TimeSpan timeout)
        {

        }

        protected override void OnOpened()
        {
            this.channelQueue = new InputQueue<TChannel>();

            Fx.Assert(this.socketReceiveManager == null, "receive manager shouldn't be initialized yet");

            this.socketReceiveManager = new UdpSocketReceiveManager(this.listenSockets.ToArray(),
                UdpConstants.PendingReceiveCountPerProcessor * Environment.ProcessorCount,
                this.bufferManager,
                this);

            //do the state change to CommunicationState.Opened before starting the receive loop.
            //this avoids a ---- between transitioning state and processing messages that are
            //already in the socket receive buffer.
            base.OnOpened();

            this.socketReceiveManager.Open();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            ThrowPending();

            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return this.channelQueue.WaitForItem(timeout);
        }

        void Cleanup()
        {
            if (Interlocked.Increment(ref this.cleanedUp) == 1)
            {
                lock (this.ThisLock)
                {
                    if (this.socketReceiveManager != null)
                    {
                        this.socketReceiveManager.Close();
                        this.socketReceiveManager = null;
                    }

                    // close the sockets to keep ref count consistent(socket will not be acutally closed unless ref count is 0).
                    foreach (UdpSocket udpSocket in this.listenSockets)
                    {
                        udpSocket.Close();
                    }

                    if (this.listenSockets != null)
                    {
                        this.listenSockets.Clear();
                        this.listenSockets = null;
                    }
                }

                if (this.bufferManager != null)
                {
                    this.bufferManager.Clear();
                }

                if (this.channelQueue != null)
                {
                    this.channelQueue.Close();
                }

                if (this.duplicateDetector != null)
                {
                    this.duplicateDetector.Dispose();
                }
            }
        }

        //must be called under a lock
        bool CreateOrRetrieveChannel(out TChannel channel)
        {
            bool channelCreated = false;

            channel = this.channelInstance;

            if (channel == null)
            {
                channelCreated = true;

                UdpSocket[] sendSockets = this.listenSockets.ToArray();

                channel = this.CreateChannel();

                this.channelInstance = channel;

                channel.Closed += this.onChannelClosed;
            }

            return channelCreated;
        }

        public abstract TChannel CreateChannel();

        bool Dispatch(Message message, int messageBufferSize, Action onMessageDequeuedCallback)
        {
            TChannel channel;
            bool channelCreated;

            lock (this.ThisLock)
            {
                if (this.State != CommunicationState.Opened)
                {
                    Fx.Assert(this.State > CommunicationState.Opened, "DispatchMessage called when object is not fully opened.  This would indicate that the receive loop started before transitioning to CommunicationState.Opened, which should not happen.");

                    //Shutting down - the message will get closed by the caller (IUdpReceiveHandler.OnMessageReceivedCallback)
                    return false;
                }

                channelCreated = CreateOrRetrieveChannel(out channel);
            }

            if (channelCreated)
            {
                this.channelQueue.EnqueueAndDispatch(channel, null, false);
            }

            return channel.EnqueueMessage(message, messageBufferSize, onMessageDequeuedCallback);
        }

        //Tries to enqueue this async exception onto the channel instance if possible, 
        //puts it onto the local exception queue otherwise.
        void HandleReceiveException(Exception ex)
        {
            TChannel channel = this.channelInstance;

            if (channel != null)
            {
                channel.HandleReceiveException(ex);
            }
            else
            {
                if (UdpUtility.CanIgnoreServerException(ex))
                {
                    FxTrace.Exception.AsWarning(ex);
                }
                else
                {
                    this.channelQueue.EnqueueAndDispatch(UdpUtility.WrapAsyncException(ex), null, false);
                }
            }
        }

        void InitExplicitUri(Uri listenUriBaseAddress, string relativeAddress)
        {
            if (listenUriBaseAddress.IsDefaultPort || listenUriBaseAddress.Port == 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("context.ListenUriBaseAddress", listenUriBaseAddress, SR.ExplicitListenUriModeRequiresPort);
            }

            this.listenUri = UdpUtility.AppendRelativePath(listenUriBaseAddress, relativeAddress);

        }

        void InitSockets(bool updateListenPort)
        {
            bool ipV4;
            bool ipV6;

            UdpUtility.CheckSocketSupport(out ipV4, out ipV6);

            Fx.Assert(this.listenSockets == null, "listen sockets should only be initialized once");

            this.listenSockets = new List<UdpSocket>();

            int port = (this.listenUri.IsDefaultPort ? 0 : this.listenUri.Port);

            if (this.listenUri.HostNameType == UriHostNameType.IPv6 ||
                this.listenUri.HostNameType == UriHostNameType.IPv4)
            {
                UdpUtility.ThrowOnUnsupportedHostNameType(this.listenUri);
                
                IPAddress address = IPAddress.Parse(this.listenUri.DnsSafeHost);

                if (UdpUtility.IsMulticastAddress(address))
                {

                    this.isMulticast = true;

                    NetworkInterface[] adapters = UdpUtility.GetMulticastInterfaces(udpTransportBindingElement.MulticastInterfaceId);

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
                                if (UdpUtility.TryGetLoopbackInterfaceIndex(adapters[i], address.AddressFamily == AddressFamily.InterNetwork, out interfaceIndex))
                                {
                                    listenSockets.Add(UdpUtility.CreateListenSocket(address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive,
                                        interfaceIndex, allowMulticastLoopback, isLoopbackAdapter));
                                }

                            }
                            else if (this.listenUri.HostNameType == UriHostNameType.IPv6)
                            {
                                if (adapters[i].Supports(NetworkInterfaceComponent.IPv6))
                                {
                                    IPv6InterfaceProperties v6Properties = properties.GetIPv6Properties();

                                    if (v6Properties != null)
                                    {
                                        listenSockets.Add(UdpUtility.CreateListenSocket(address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize,
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
                                        listenSockets.Add(UdpUtility.CreateListenSocket(address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize,
                                            this.udpTransportBindingElement.TimeToLive, v4Properties.Index, allowMulticastLoopback, isLoopbackAdapter));
                                    }
                                }
                            }
                        }
                    }

                    if (listenSockets.Count == 0)
                    {
                        throw FxTrace.Exception.AsError(new ArgumentException(SR.UdpFailedToFindMulticastAdapter(this.listenUri)));
                    }
                }
                else
                {
                    //unicast - only sends on the default adapter...
                    this.listenSockets.Add(UdpUtility.CreateUnicastListenSocket(address, ref port,
                        this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive));
                }
            }
            else
            {
                IPAddress v4Address = IPAddress.Any;
                IPAddress v6Address = IPAddress.IPv6Any;

                if (ipV4 && ipV6)
                {
                    if (port == 0)
                    {
                        //port 0 is only allowed when ListenUriMode == ListenUriMode.Unique
                        UdpSocket ipv4Socket, ipv6Socket;
                        port = UdpUtility.CreateListenSocketsOnUniquePort(v4Address, v6Address, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive, out ipv4Socket, out ipv6Socket);

                        this.listenSockets.Add(ipv4Socket);
                        this.listenSockets.Add(ipv6Socket);
                    }
                    else
                    {
                        this.listenSockets.Add(UdpUtility.CreateUnicastListenSocket(v4Address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive));
                        this.listenSockets.Add(UdpUtility.CreateUnicastListenSocket(v6Address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive));
                    }
                }
                else if (ipV4)
                {
                    this.listenSockets.Add(UdpUtility.CreateUnicastListenSocket(v4Address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive));
                }
                else if (ipV6)
                {
                    this.listenSockets.Add(UdpUtility.CreateUnicastListenSocket(v6Address, ref port, this.udpTransportBindingElement.SocketReceiveBufferSize, this.udpTransportBindingElement.TimeToLive));
                }
            }

            if (updateListenPort && port != this.listenUri.Port)
            {
                UriBuilder uriBuilder = new UriBuilder(this.listenUri);
                uriBuilder.Port = port;
                this.listenUri = uriBuilder.Uri;
            }

            // Open all the sockets to keep ref counts consistent
            foreach (UdpSocket udpSocket in this.ListenSockets)
            {
                udpSocket.Open();
            }
        }

        void InitUniqueUri(Uri listenUriBaseAddress, string relativeAddress)
        {
            Fx.Assert(listenUriBaseAddress != null, "listenUriBaseAddress parameter should have been verified before now");

            listenUriBaseAddress = UdpUtility.AppendRelativePath(listenUriBaseAddress, relativeAddress);

            this.listenUri = UdpUtility.AppendRelativePath(listenUriBaseAddress, Guid.NewGuid().ToString());
        }

        void InitUri(BindingContext context)
        {
            Uri listenUriBase = context.ListenUriBaseAddress;

            if (context.ListenUriMode == ListenUriMode.Unique && listenUriBase == null)
            {
                UriBuilder uriBuilder = new UriBuilder(this.Scheme, DnsCache.MachineName);
                uriBuilder.Path = Guid.NewGuid().ToString();
                listenUriBase = uriBuilder.Uri;
                context.ListenUriBaseAddress = listenUriBase;
            }
            else
            {
                if (listenUriBase == null)
                {
                    throw FxTrace.Exception.ArgumentNull("context.ListenUriBaseAddress");
                }

                if (!listenUriBase.IsAbsoluteUri)
                {
                    throw FxTrace.Exception.Argument("context.ListenUriBaseAddress", SR.RelativeUriNotAllowed(listenUriBase));
                }

                if (context.ListenUriMode == ListenUriMode.Unique && !listenUriBase.IsDefaultPort)
                {
                    throw FxTrace.Exception.Argument("context.ListenUriBaseAddress", SR.DefaultPortRequiredForListenUriModeUnique(listenUriBase));
                }

                if (listenUriBase.Scheme.Equals(this.Scheme, StringComparison.OrdinalIgnoreCase) == false)
                {
                    throw FxTrace.Exception.Argument("context.ListenUriBaseAddress", SR.UriSchemeNotSupported(listenUriBase.Scheme));
                }

                if (!UdpUtility.IsSupportedHostNameType(listenUriBase.HostNameType))
                {
                    throw FxTrace.Exception.Argument("context.ListenUriBaseAddress", SR.UnsupportedUriHostNameType(listenUriBase.Host, listenUriBase.HostNameType));
                }
            }

            switch (context.ListenUriMode)
            {
                case ListenUriMode.Explicit:
                    InitExplicitUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
                    break;
                case ListenUriMode.Unique:
                    InitUniqueUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
                    break;
                default:
                    Fx.AssertAndThrow("Unhandled ListenUriMode encountered: " + context.ListenUriMode);
                    break;
            }
        }

        void OnChannelClosed(object sender, EventArgs args)
        {
            TChannel closingChannel = (TChannel)sender;
            closingChannel.Closed -= this.onChannelClosed;

            lock (ThisLock)
            {
                //set to null within a lock because other code
                //assumes that the instance will not suddenly become null 
                //if it already holds the lock.
                this.channelInstance = null;
            }
        }
    }

    internal class UdpDuplexChannelListener : UdpChannelListener<IDuplexChannel, UdpDuplexChannel, Message>
    {
        public UdpDuplexChannelListener(UdpTransportBindingElement udpTransportBindingElement, BindingContext context)
            : base(udpTransportBindingElement, context)
        {
        }

        public override UdpDuplexChannel CreateChannel()
        {
            return new ServerUdpDuplexChannel(this, this.ListenSockets.ToArray(), new EndpointAddress(this.Uri), this.Uri, this.IsMulticast);
        }
    }

    internal class UdpReplyChannelListener : UdpChannelListener<IReplyChannel, UdpReplyChannel, RequestContext>
    {
        public UdpReplyChannelListener(UdpTransportBindingElement udpTransportBindingElement, BindingContext context)
            : base(udpTransportBindingElement, context)
        {
        }

        public override UdpReplyChannel CreateChannel()
        {
            return new UdpReplyChannel(this, this.ListenSockets.ToArray(), new EndpointAddress(this.Uri), this.Uri, this.IsMulticast);
        }
    }


    internal sealed class ServerUdpDuplexChannel : UdpDuplexChannel
    {
        //the listener's buffer manager is used, but the channel won't clear it unless 
        //UdpChannelListener.OnClosing successfully transfers ownership to the channel instance.
        public ServerUdpDuplexChannel(UdpDuplexChannelListener listener, UdpSocket[] sockets, EndpointAddress localAddress, Uri via, bool isMulticast)
            : base(listener, listener.MessageEncoderFactory.Encoder, listener.BufferManager,
            sockets, listener.UdpTransportBindingElement.RetransmissionSettings, listener.UdpTransportBindingElement.MaxPendingMessagesTotalSize,
            localAddress, via, listener.IsMulticast, (int)listener.UdpTransportBindingElement.MaxReceivedMessageSize)
        {
            UdpOutputChannel udpOutputChannel = new ServerUdpOutputChannel(
                listener,
                listener.MessageEncoderFactory.Encoder,
                listener.BufferManager,
                sockets,
                listener.UdpTransportBindingElement.RetransmissionSettings,
                via,
                isMulticast);

            this.SetOutputChannel(udpOutputChannel);
        }

        protected override bool IgnoreSerializationException
        {
            get
            {
                return true;
            }
        }

        internal override void HandleReceiveException(Exception ex)
        {
            if (UdpUtility.CanIgnoreServerException(ex))
            {
                FxTrace.Exception.AsWarning(ex);
            }
            else
            {
                //base implementation will wrap the exception and enqueue it.
                base.HandleReceiveException(ex);
            }
        }
    }

}
