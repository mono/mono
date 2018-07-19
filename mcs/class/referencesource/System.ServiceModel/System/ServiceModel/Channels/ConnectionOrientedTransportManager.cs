//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Activation;
    using System.ServiceModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;

    abstract class ConnectionOrientedTransportManager<TChannelListener> : TransportManager
        where TChannelListener : ConnectionOrientedTransportChannelListener
    {
        UriPrefixTable<TChannelListener> addressTable;
        int connectionBufferSize;
        TimeSpan channelInitializationTimeout;
        int maxPendingConnections;
        TimeSpan maxOutputDelay;
        int maxPendingAccepts;
        TimeSpan idleTimeout;
        int maxPooledConnections;
        Action messageReceivedCallback;

        protected ConnectionOrientedTransportManager()
        {
            this.addressTable = new UriPrefixTable<TChannelListener>();
        }

        UriPrefixTable<TChannelListener> AddressTable
        {
            get { return addressTable; }
        }

        protected TimeSpan ChannelInitializationTimeout
        {
            get
            {
                return channelInitializationTimeout;
            }
        }

        internal void ApplyListenerSettings(IConnectionOrientedListenerSettings listenerSettings)
        {
            this.connectionBufferSize = listenerSettings.ConnectionBufferSize;
            this.channelInitializationTimeout = listenerSettings.ChannelInitializationTimeout;
            this.maxPendingConnections = listenerSettings.MaxPendingConnections;
            this.maxOutputDelay = listenerSettings.MaxOutputDelay;
            this.maxPendingAccepts = listenerSettings.MaxPendingAccepts;
            this.idleTimeout = listenerSettings.IdleTimeout;
            this.maxPooledConnections = listenerSettings.MaxPooledConnections;
        }

        internal int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
        }

        internal int MaxPendingConnections
        {
            get
            {
                return this.maxPendingConnections;
            }
        }

        internal TimeSpan MaxOutputDelay
        {
            get
            {
                return maxOutputDelay;
            }
        }

        internal int MaxPendingAccepts
        {
            get
            {
                return this.maxPendingAccepts;
            }
        }

        internal TimeSpan IdleTimeout
        {
            get { return this.idleTimeout; }
        }

        internal int MaxPooledConnections
        {
            get { return this.maxPooledConnections; }
        }

        internal bool IsCompatible(ConnectionOrientedTransportChannelListener channelListener)
        {
            if (channelListener.InheritBaseAddressSettings)
                return true;

            return (
                (this.ChannelInitializationTimeout == channelListener.ChannelInitializationTimeout) &&
                (this.ConnectionBufferSize == channelListener.ConnectionBufferSize) &&
                (this.MaxPendingConnections == channelListener.MaxPendingConnections) &&
                (this.MaxOutputDelay == channelListener.MaxOutputDelay) &&
                (this.MaxPendingAccepts == channelListener.MaxPendingAccepts) &&
                (this.idleTimeout == channelListener.IdleTimeout) &&
                (this.maxPooledConnections == channelListener.MaxPooledConnections)
                );
        }

        TChannelListener GetChannelListener(Uri via)
        {
            TChannelListener channelListener = null;
            if (AddressTable.TryLookupUri(via, HostNameComparisonMode.StrongWildcard, out channelListener))
            {
                return channelListener;
            }

            if (AddressTable.TryLookupUri(via, HostNameComparisonMode.Exact, out channelListener))
            {
                return channelListener;
            }

            AddressTable.TryLookupUri(via, HostNameComparisonMode.WeakWildcard, out channelListener);
            return channelListener;
        }

        internal void OnDemuxerError(Exception exception)
        {
            lock (ThisLock)
            {
                this.Fault(this.AddressTable, exception);
            }
        }

        internal ISingletonChannelListener OnGetSingletonMessageHandler(ServerSingletonPreambleConnectionReader serverSingletonPreambleReader)
        {
            Uri via = serverSingletonPreambleReader.Via;
            TChannelListener channelListener = GetChannelListener(via);

            if (channelListener != null)
            {
                if (channelListener is IChannelListener<IReplyChannel>)
                {
                    channelListener.RaiseMessageReceived();
                    return (ISingletonChannelListener)channelListener;
                }
                else
                {
                    serverSingletonPreambleReader.SendFault(FramingEncodingString.UnsupportedModeFault);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.FramingModeNotSupported, FramingMode.Singleton)));
                }
            }
            else
            {
                serverSingletonPreambleReader.SendFault(FramingEncodingString.EndpointNotFoundFault);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, via)));
            }
        }

        internal void OnHandleServerSessionPreamble(ServerSessionPreambleConnectionReader serverSessionPreambleReader,
            ConnectionDemuxer connectionDemuxer)
        {
            Uri via = serverSessionPreambleReader.Via;
            TChannelListener channelListener = GetChannelListener(via);

            if (channelListener != null)
            {
                ISessionPreambleHandler sessionPreambleHandler = channelListener as ISessionPreambleHandler;

                if (sessionPreambleHandler != null && channelListener is IChannelListener<IDuplexSessionChannel>)
                {
                    sessionPreambleHandler.HandleServerSessionPreamble(serverSessionPreambleReader, connectionDemuxer);
                }
                else
                {
                    serverSessionPreambleReader.SendFault(FramingEncodingString.UnsupportedModeFault);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.FramingModeNotSupported, FramingMode.Duplex)));
                }
            }
            else
            {
                serverSessionPreambleReader.SendFault(FramingEncodingString.EndpointNotFoundFault);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.DuplexSessionListenerNotFound, via.ToString())));
            }
        }

        internal IConnectionOrientedTransportFactorySettings OnGetTransportFactorySettings(Uri via)
        {
            return GetChannelListener(via);
        }

        internal override void Register(TransportChannelListener channelListener)
        {
            AddressTable.RegisterUri(channelListener.Uri, channelListener.HostNameComparisonModeInternal,
                (TChannelListener)channelListener);

            channelListener.SetMessageReceivedCallback(new Action(OnMessageReceived));
        }

        internal override void Unregister(TransportChannelListener channelListener)
        {
            EnsureRegistered(AddressTable, (TChannelListener)channelListener, channelListener.HostNameComparisonModeInternal);
            AddressTable.UnregisterUri(channelListener.Uri, channelListener.HostNameComparisonModeInternal);
            channelListener.SetMessageReceivedCallback(null);
        }

        internal void SetMessageReceivedCallback(Action messageReceivedCallback)
        {
            this.messageReceivedCallback = messageReceivedCallback;
        }

        void OnMessageReceived()
        {
            Action callback = this.messageReceivedCallback;
            if (callback != null)
            {
                callback();
            }
        }
    }
}
