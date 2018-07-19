//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]
    public class UdpTransportSettings
    {
        int maxPendingMessageCount;
        
        internal UdpTransportSettings(UdpTransportBindingElement udpTransportBindingElement)
        {
            this.maxPendingMessageCount = UdpConstants.Defaults.MaxPendingMessageCount;
            this.UdpTransportBindingElement = udpTransportBindingElement;
        }

        public int DuplicateMessageHistoryLength
        {
            get
            {
                return this.UdpTransportBindingElement.DuplicateMessageHistoryLength;
            }
            set
            {
                this.UdpTransportBindingElement.DuplicateMessageHistoryLength = value;
            }
        }

        public int MaxPendingMessageCount
        {
            get
            {
                return this.maxPendingMessageCount;
            }
            set
            {
                this.maxPendingMessageCount = value;
                this.UdpTransportBindingElement.MaxPendingMessagesTotalSize = this.MaxReceivedMessageSize * this.MaxPendingMessageCount;
            }
        }

        public int MaxMulticastRetransmitCount 
        {
            get
            {
                return this.UdpTransportBindingElement.RetransmissionSettings.MaxMulticastRetransmitCount;
            }
            set
            {
                this.UdpTransportBindingElement.RetransmissionSettings.MaxMulticastRetransmitCount = value;
            }
        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Unicast is a valid name.")]
        public int MaxUnicastRetransmitCount
        {
            get
            {
                return this.UdpTransportBindingElement.RetransmissionSettings.MaxUnicastRetransmitCount;
            }
            set
            {
                this.UdpTransportBindingElement.RetransmissionSettings.MaxUnicastRetransmitCount = value;
            }
        }

        public string MulticastInterfaceId
        {
            get
            {
                return this.UdpTransportBindingElement.MulticastInterfaceId;
            }
            set
            {
                this.UdpTransportBindingElement.MulticastInterfaceId = value;
            }
        }

        public int SocketReceiveBufferSize
        {
            get
            {
                return this.UdpTransportBindingElement.SocketReceiveBufferSize;
            }
            set
            {
                this.UdpTransportBindingElement.SocketReceiveBufferSize = value;
            }
        }

        public long MaxReceivedMessageSize
        {
            get
            {
                return this.UdpTransportBindingElement.MaxReceivedMessageSize;
            }
            set
            {
                this.UdpTransportBindingElement.MaxReceivedMessageSize = value;
            }
        }

        public long MaxBufferPoolSize
        {
            get
            {
                return this.UdpTransportBindingElement.MaxBufferPoolSize;
            }
            set
            {
                this.UdpTransportBindingElement.MaxBufferPoolSize = value;
            }
        }

        public int TimeToLive 
        {
            get
            {
                return this.UdpTransportBindingElement.TimeToLive;
            }
            set
            {
                this.UdpTransportBindingElement.TimeToLive = value;
            }
        }

        internal UdpTransportBindingElement UdpTransportBindingElement
        {
            get;
            private set;
        }
    }
}
