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

    internal class ClientUdpOutputChannel : UdpOutputChannel
    {
        private EndpointAddress to;

        public ClientUdpOutputChannel(ChannelManagerBase factory, IPEndPoint remoteEndPoint, MessageEncoder encoder, BufferManager bufferManager, UdpSocket[] sendSockets, UdpRetransmissionSettings retransmissionSettings, EndpointAddress to, Uri via, bool isMulticast)
            : base(factory, encoder, bufferManager, sendSockets, retransmissionSettings, via, isMulticast)
        {
            Fx.Assert(to != null, "to address can't be null for this constructor...");
            Fx.Assert(remoteEndPoint != null, "remoteEndPoint can't be null");
            
            this.RemoteEndPoint = remoteEndPoint;
            this.to = to;
        }

        public IPEndPoint RemoteEndPoint
        {
            get;
            private set;
        }

        protected override UdpSocket[] GetSendSockets(Message message, out IPEndPoint remoteEndPoint, out Exception exceptionToBeThrown)
        {
            UdpSocket[] socketList = null;
            remoteEndPoint = this.RemoteEndPoint;
            exceptionToBeThrown = null;

            if (this.IsMulticast)
            {
                // always send on all sockets...
                socketList = this.SendSockets;
            }
            else
            {
                Fx.Assert(this.SendSockets.Length == 1, "Unicast Send socket list on client should always be 1 item long");
                socketList = this.SendSockets;
            }

            return socketList;
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
